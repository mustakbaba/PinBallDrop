using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Chat.Interface;
using ElephantSDK;

namespace ElephantSocial.Chat.Core
{
    public class UnityWebSocketClient : IWebSocketClient, IDisposable
    {
        private ClientWebSocket _webSocket;
        private volatile Interface.WebSocketState _state = Interface.WebSocketState.Closed;
        private readonly object _stateLock = new object();
        private CancellationTokenSource _receiveCts;
        
        private const int ReceiveBufferSize = 8192;
        private List<byte> _messageBuffer = new List<byte>();
        
        public event Action OnOpen;
        public event Action<string> OnClose;
        public event Action<string> OnError;
        public event Action<byte[]> OnMessage;

        public Interface.WebSocketState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        public UnityWebSocketClient()
        {
            _webSocket = new ClientWebSocket();
            _receiveCts = new CancellationTokenSource();
            
            _webSocket.Options.KeepAliveInterval = TimeSpan.Zero;
        }

        public async UniTask ConnectAsync(string url, Dictionary<string, string> headers = null)
        {
            ElephantLog.Log("UnityWebSocketClient", $"ConnectAsync called with URL: {url}");

            lock (_stateLock)
            {
                if (_state == Interface.WebSocketState.Open || _state == Interface.WebSocketState.Connecting)
                    return;

                _state = Interface.WebSocketState.Connecting;
            }
            
            if (_webSocket != null && _webSocket.State != System.Net.WebSockets.WebSocketState.None)
            {
                try
                {
                    (_webSocket)?.Dispose();
                    _webSocket = null;
                }
                catch (Exception ex)
                {
                    ElephantLog.Log("UnityWebSocketClient", $"Non-critical error disposing old WebSocket: {ex.Message}");
                }
            }
            
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetBuffer(65536, 65536);
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    _webSocket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }

            try
            {
                Uri uri = new Uri(url);
                await _webSocket.ConnectAsync(uri, CancellationToken.None);
                
                lock (_stateLock)
                {
                    _state = Interface.WebSocketState.Open;
                }
                
                OnOpen?.Invoke();
                
                _receiveCts = new CancellationTokenSource();
                ReceiveLoopAsync(_receiveCts.Token).Forget();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("UnityWebSocketClient", $"Connection failed: {ex.Message}");
                
                lock (_stateLock)
                {
                    _state = Interface.WebSocketState.Closed;
                }
                
                OnError?.Invoke($"Connection Failed: {ex.Message}");
            }
        }

        private async UniTask ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[ReceiveBufferSize];
            _messageBuffer.Clear();
            
            try
            {
                while (!cancellationToken.IsCancellationRequested && 
                       _webSocket != null &&
                       _webSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var segment = new ArraySegment<byte>(buffer);
                    
                    WebSocketReceiveResult result;
                    try 
                    {
                        result = await _webSocket.ReceiveAsync(segment, cancellationToken);
                    }
                    catch (WebSocketException wsEx)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        
                        ElephantLog.Log("UnityWebSocketClient", $"WebSocket error: {wsEx.Message}");
                        await HandleCloseAsync("WebSocket error: " + wsEx.Message);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        
                        ElephantLog.LogError("UnityWebSocketClient", $"Error during receive: {ex.Message}");
                        await HandleCloseAsync("Error: " + ex.Message);
                        break;
                    }
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        ElephantLog.Log("UnityWebSocketClient", $"Received close frame: {result.CloseStatus}");
                        await HandleCloseAsync(result.CloseStatus?.ToString() ?? "Connection closed by server");
                        break;
                    }
                    
                    if (result.MessageType == WebSocketMessageType.Binary && result.Count > 0 && buffer[0] == 0x89)
                    {
                        ElephantLog.Log("UnityWebSocketClient", "Received ping frame (unexpected)");
                        continue;
                    }
                    
                    for (int i = 0; i < result.Count; i++)
                    {
                        _messageBuffer.Add(buffer[i]);
                    }
                    
                    if (result.EndOfMessage)
                    {
                        var messageBytes = _messageBuffer.ToArray();
                        _messageBuffer.Clear();
                        
                        try
                        {
                            OnMessage?.Invoke(messageBytes);
                        }
                        catch (Exception ex)
                        {
                            ElephantLog.LogError("UnityWebSocketClient", $"Error in OnMessage handler: {ex.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ElephantLog.Log("UnityWebSocketClient", "Receive operation cancelled");
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("UnityWebSocketClient", $"Error in receive loop: {ex.Message}");
                OnError?.Invoke($"Receive Error: {ex.Message}");
            }
            finally
            {
                if (_webSocket != null && _webSocket.State != System.Net.WebSockets.WebSocketState.Closed)
                {
                    await HandleCloseAsync("Receive loop ended");
                }
            }
        }

        private async UniTask HandleCloseAsync(string reason)
        {
            lock (_stateLock)
            {
                if (_state == Interface.WebSocketState.Closed)
                    return;
                
                _state = Interface.WebSocketState.Closed;
            }
            
            ElephantLog.Log("UnityWebSocketClient", $"Handling connection close: {reason}");
            
            try
            {
                OnClose?.Invoke(reason);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("UnityWebSocketClient", $"Error in OnClose handler: {ex.Message}");
            }
            
            if (_webSocket != null && 
                _webSocket.State != System.Net.WebSockets.WebSocketState.Closed)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                        "Client closed connection", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    ElephantLog.Log("UnityWebSocketClient", $"Non-critical error closing WebSocket: {ex.Message}");
                }
            }
        }

        public async UniTask SendTextAsync(string data)
        {
            if (_webSocket == null || _webSocket.State != System.Net.WebSockets.WebSocketState.Open)
            {
                throw new InvalidOperationException($"WebSocket is not open. State: {(_webSocket != null ? _webSocket.State.ToString() : "null")}");
            }
            
            try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                var segment = new ArraySegment<byte>(bytes);
                
                await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("UnityWebSocketClient", $"Send error: {ex.Message}");
                OnError?.Invoke($"Send Error: {ex.Message}");
                throw;
            }
        }

        public async UniTask CloseAsync()
        {
            if (_webSocket == null || 
                _webSocket.State == System.Net.WebSockets.WebSocketState.Closed)
            {
                lock (_stateLock)
                {
                    _state = Interface.WebSocketState.Closed;
                }
                return;
            }
            
            lock (_stateLock)
            {
                if (_state == Interface.WebSocketState.Closed || _state == Interface.WebSocketState.Closing)
                    return;
                
                _state = Interface.WebSocketState.Closing;
            }
            
            try
            {
                ElephantLog.Log("UnityWebSocketClient", "Closing WebSocket...");
                
                if (_receiveCts != null && !_receiveCts.IsCancellationRequested)
                {
                    _receiveCts.Cancel();
                }
                
                if (_webSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                        "Client closed connection", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("UnityWebSocketClient", $"Close error: {ex.Message}");
                OnError?.Invoke($"Close Error: {ex.Message}");
            }
            finally
            {
                lock (_stateLock)
                {
                    _state = Interface.WebSocketState.Closed;
                }
                
                if (_webSocket != null)
                {
                    (_webSocket as IDisposable)?.Dispose();
                    _webSocket = null;
                }
                
                ElephantLog.Log("UnityWebSocketClient", "CloseAsync finished");
            }
        }

        public void Dispose()
        {
            ElephantLog.Log("UnityWebSocketClient", "Dispose called");
            
            try
            {
                if (_receiveCts != null)
                {
                    _receiveCts.Cancel();
                    _receiveCts.Dispose();
                    _receiveCts = null;
                }
                
                if (_webSocket != null)
                {
                    (_webSocket as IDisposable)?.Dispose();
                    _webSocket = null;
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("UnityWebSocketClient", $"Error in Dispose: {ex.Message}");
            }
            
            GC.SuppressFinalize(this);
        }
    }
}