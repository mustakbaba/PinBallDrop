using System;
using System.Threading;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Chat.Interface;
using ElephantSocial.Chat.Model;
using ElephantSocial.Chat.Util;
using ElephantSDK;
using ElephantSocial.Core;

namespace ElephantSocial.Chat.Core
{
    public static class ElephantChat
    {
        private static ChatClient _chatClientInstance;
        private static IWebSocketClient _nativeClientInstance;
        private static bool _isInitialized = false;
        private static bool _isConnecting = false; 
        private static CancellationTokenSource _connectionCts; 

        public static ChatClient Client => _chatClientInstance;
        public static WebSocketState ConnectionState => _chatClientInstance?.ConnectionState ?? WebSocketState.Closed;
        public static bool IsInitialized => _isInitialized;

        public static event Action<bool> OnConnectionStatusChanged;
        
        private static bool _isAttemptingReconnection = false;
        private static int _reconnectionAttempts = 0;
        private const int MaxReconnectionAttempts = 5; 
        private const int ReconnectionBaseDelayMs = 2000; 
        private const int ReconnectionMaxDelayMs = 30000; 
        private static CancellationTokenSource _reconnectionCts;
        private static bool _explicitDisconnect = false;

        public static bool IsAttemptingReconnection => _isAttemptingReconnection;

        public static async UniTask<bool> ConnectAsync()
        {
            ElephantLog.Log("ElephantChat", "ConnectAsync called");
            if (!_isInitialized)
            {
                ElephantLog.Log("ElephantChat", "ConnectAsync: Not initialized, initializing now");
                InitializeWebSocket();
                if (!_isInitialized)
                {
                    ElephantLog.LogError("ElephantChat", "ConnectAsync: Initialization failed");
                    return false;
                }
            }
            
            _explicitDisconnect = false; 
            if (!_isAttemptingReconnection)
            {
                _reconnectionCts?.Cancel();
                _reconnectionAttempts = 0;
            }
            _isAttemptingReconnection = false;
            _reconnectionAttempts = 0;

            if (_isConnecting)
            {
                ElephantLog.Log("ElephantChat", "ConnectAsync: Connection already in progress.");
                return false; 
            }

            if (ConnectionState == WebSocketState.Open)
            {
                ElephantLog.Log("ElephantChat", "ConnectAsync: Already connected.");
                return true;
            }
                
            _isConnecting = true;
            _connectionCts?.Cancel();
            _connectionCts = new CancellationTokenSource();

            bool connectedSuccessfully = false;
            try
            {
                var player = Social.Instance?.GetPlayer();
                if (player == null)
                {
                    ElephantLog.LogError("ElephantChat", "ConnectAsync: Player not available.");
                    throw new InvalidOperationException("Player not available for ConnectAsync.");
                }
                var socialId = player.socialId;
                if (string.IsNullOrEmpty(socialId))
                {
                    ElephantLog.LogError("ElephantChat", "ConnectAsync: Player social ID is empty.");
                    throw new InvalidOperationException("Player social ID is empty for ConnectAsync.");
                }

                var userId = ElephantCore.Instance.userId;
                var gameId = ElephantCore.Instance.gameID;
                var secret = ElephantCore.Instance.gameSecret;
                var signedToken = JwtHelper.GenerateJwtToken(userId, gameId, secret);
                var url = GetChatUrl() + $"/ws/es?social_id={socialId}&token={signedToken}";
                ElephantLog.Log("ElephantChat", $"Connecting to WebSocket: {url}");
                
                var connectTimeoutCts = new CancellationTokenSource();
                connectTimeoutCts.CancelAfterSlim(TimeSpan.FromSeconds(15));
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_connectionCts.Token, connectTimeoutCts.Token);

                try
                {
                    await ConnectInternalAsync(url, socialId).AttachExternalCancellation(linkedCts.Token);
                    if (ConnectionState == WebSocketState.Open) 
                    {
                         ElephantLog.Log("ElephantChat", "ConnectAsync: Connection successful.");
                         connectedSuccessfully = true;
                    } else {
                         ElephantLog.LogError("ElephantChat", $"ConnectAsync: ConnectInternalAsync completed but state is {ConnectionState}.");
                    }
                }
                catch (OperationCanceledException ex)
                {
                    if (connectTimeoutCts.Token.IsCancellationRequested)
                        ElephantLog.LogError("ElephantChat", "ConnectAsync: Connection attempt timed out.");
                    else if (_connectionCts.Token.IsCancellationRequested)
                        ElephantLog.Log("ElephantChat", "ConnectAsync: Connection attempt cancelled by system/caller.");
                    else
                        ElephantLog.LogError("ElephantChat", $"ConnectAsync: Connection attempt cancelled: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ElephantLog.LogError("ElephantChat", $"ConnectAsync: Error during connection attempt: {ex.Message} - {ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                 ElephantLog.LogError("ElephantChat", $"ConnectAsync: Pre-connection error: {ex.Message}");
            }
            finally
            {
                _isConnecting = false;
            }
            return connectedSuccessfully;
        }
        
        private static void InitializeWebSocket()
        {
            ElephantLog.Log("ElephantChat", "Initialize called");
            
            if (_isInitialized)
            {
                ElephantLog.Log("ElephantChat", "Already initialized");
                return;
            }

            if (_isConnecting || _isAttemptingReconnection)
            {
                ElephantLog.Log("ElephantChat", "Initialization, connection, or reconnection already in progress");
                return;
            }
            
            _explicitDisconnect = false; 
            
            try
            {
                ElephantLog.Log("ElephantChat", "Creating WebSocket client instance...");
                _nativeClientInstance = new UnityWebSocketClient();
                _nativeClientInstance.OnError += (errorMsg) =>
                {
                    ElephantLog.LogError("ElephantChat", $"Native WebSocket error: {errorMsg}");
                };

                ElephantLog.Log("ElephantChat", "Creating ChatClient instance...");
                _chatClientInstance = new ChatClient(_nativeClientInstance);
                _chatClientInstance.ConnectionStatusChanged += HandleChatClientConnectionStatusChanged;
                _chatClientInstance.ErrorOccurred += HandleChatClientError;

                _isInitialized = true;
                ElephantLog.Log("ElephantChat", "Initialized successfully");
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ElephantChat", $"Initialization failed: {ex.Message}");
                CleanupResources();
            }
        }

        private static async UniTask ConnectInternalAsync(string url, string authToken)
        {
            var connectionTcs = new UniTaskCompletionSource<bool>();
            EventHandler<ConnectionStatusEventArgs> connectionHandler = null;

            connectionHandler = (sender, args) =>
            {
                if (connectionHandler != null) 
                {
                    _chatClientInstance.ConnectionStatusChanged -= connectionHandler;
                    connectionHandler = null; 
                }
                connectionTcs.TrySetResult(args.IsConnected);
            };
            _chatClientInstance.ConnectionStatusChanged += connectionHandler;

            try
            {
                await _chatClientInstance.ConnectAsync(url, authToken);
                bool connected = await connectionTcs.Task;
                if (!connected)
                {
                    throw new ConnectionException($"ConnectInternalAsync: WebSocket connection failed. Reason from ChatClient: {connectionTcs.Task.Status}");
                }
                ElephantLog.Log("ElephantChat", "ConnectInternalAsync: Connection reported as successful by ChatClient.");
            }
            catch (Exception ex)
            {
                if (connectionHandler != null)
                {
                    _chatClientInstance.ConnectionStatusChanged -= connectionHandler;
                }
                ElephantLog.LogError("ElephantChat", $"ConnectInternalAsync: Exception: {ex.Message}");
                throw; 
            }
        }
        
        private static void HandleChatClientError(object sender, ErrorEventArgs args)
        {
            ElephantLog.LogError("ElephantChat", $"ChatClient Error: {args.Title} - {args.Message}. Current State: {ConnectionState}");
        }

        private static void HandleChatClientConnectionStatusChanged(object sender, ConnectionStatusEventArgs args)
        {
            ElephantLog.Log("ElephantChat", $"HandleChatClientConnectionStatusChanged: IsConnected={args.IsConnected}, Reason='{args.Reason}'. Current State: {ConnectionState}, ExplicitDisconnect: {_explicitDisconnect}");
            
            OnConnectionStatusChanged?.Invoke(args.IsConnected);

            if (args.IsConnected)
            {
                _isAttemptingReconnection = false;
                _reconnectionAttempts = 0;
                _reconnectionCts?.Cancel();
                ElephantLog.Log("ElephantChat", "Connection established. Reconnection process reset.");
            }
            else
            {
                bool shouldAttemptReconnect = _isInitialized && 
                                       !_isConnecting && 
                                       !_isAttemptingReconnection && 
                                       !_explicitDisconnect;
                
                if (shouldAttemptReconnect)
                {
                    ElephantLog.Log("ElephantChat", "Connection lost. Initiating reconnection process due to ChatClient status change.");
                    _reconnectionAttempts = 0;
                    _isAttemptingReconnection = true;
                    StartReconnectionLoop();
                }
                else
                {
                     ElephantLog.Log("ElephantChat", $"Skipping reconnection. Initialized={_isInitialized}, Connecting={_isConnecting}, AttemptingRec={_isAttemptingReconnection}, ExplicitDisc={_explicitDisconnect}");
                }
            }
        }
        
        private static void StartReconnectionLoop()
        {
            StartReconnectionLoopAsync().Forget();
        }
        
        private static async UniTaskVoid StartReconnectionLoopAsync()
        {
            _reconnectionCts?.Cancel();
            _reconnectionCts = new CancellationTokenSource();
            CancellationToken token = _reconnectionCts.Token;

            ElephantLog.Log("ElephantChat", "Starting Reconnection Loop.");

            try
            {
                while (_reconnectionAttempts < MaxReconnectionAttempts && !token.IsCancellationRequested)
                {
                    if (ConnectionState == WebSocketState.Open || _explicitDisconnect || _isConnecting)
                    {
                        ElephantLog.Log("ElephantChat", $"Reconnection loop stopping: Open={ConnectionState == WebSocketState.Open}, ExplicitDisconnect={_explicitDisconnect}, IsConnecting={_isConnecting}");
                        break;
                    }
                    
                    _reconnectionAttempts++;
                    int delayMs = Math.Min((int)Math.Pow(2, _reconnectionAttempts) * (ReconnectionBaseDelayMs / 2), ReconnectionMaxDelayMs);
                    ElephantLog.Log("ElephantChat", $"Reconnection attempt {_reconnectionAttempts}/{MaxReconnectionAttempts}. Waiting {delayMs}ms.");

                    try
                    {
                        await UniTask.Delay(delayMs, cancellationToken: token);
                    }
                    catch (OperationCanceledException)
                    {
                        ElephantLog.Log("ElephantChat", "Reconnection delay task was cancelled.");
                        break;
                    }

                    if (token.IsCancellationRequested || ConnectionState == WebSocketState.Open || _explicitDisconnect || _isConnecting)
                    {
                        ElephantLog.Log("ElephantChat", $"Reconnection loop stopping after delay: Cancelled={token.IsCancellationRequested}, Open={ConnectionState == WebSocketState.Open}, ExplicitDisconnect={_explicitDisconnect}, IsConnecting={_isConnecting}");
                        break;
                    }
                    
                    ElephantLog.Log("ElephantChat", $"Reconnection Loop: Attempting ConnectAsync (attempt {_reconnectionAttempts}).");
                    bool connected = await ConnectAsync(); 
                    
                    if (connected)
                    {
                        ElephantLog.Log("ElephantChat", "Reconnection Loop: ConnectAsync reported success.");
                        break; 
                    }
                    ElephantLog.Log("ElephantChat", $"Reconnection Loop: ConnectAsync attempt {_reconnectionAttempts} failed.");
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ElephantChat", $"Error in reconnection loop: {ex.Message}");
            }
            finally 
            {
                _isAttemptingReconnection = false;
                if (_reconnectionAttempts >= MaxReconnectionAttempts)
                {
                    ElephantLog.LogError("ElephantChat", "Max reconnection attempts reached. Stopping reconnection process.");
                }
            }
        }

        private static void CleanupResources()
        {
            ElephantLog.Log("ElephantChat", "Cleaning up resources");
            if (_chatClientInstance != null)
            {
                _chatClientInstance.ConnectionStatusChanged -= HandleChatClientConnectionStatusChanged;
                _chatClientInstance.ErrorOccurred -= HandleChatClientError;
                _chatClientInstance.Dispose();
                _chatClientInstance = null;
            }
            _nativeClientInstance = null; 
            _isInitialized = false;
        }
        
        private static string GetChatUrl()
        {
            return Social.Instance.SocialConfig.elephantEnvironment == ElephantEnvironment.Production ? SocialConst.ChatEp : SocialConstDev.ChatEp;
        }
    }
}