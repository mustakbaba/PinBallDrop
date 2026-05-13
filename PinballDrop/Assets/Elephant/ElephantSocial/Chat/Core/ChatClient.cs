using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Chat.Interface;
using ElephantSocial.Chat.Model;
using ElephantSDK;
using Newtonsoft.Json;

namespace ElephantSocial.Chat.Core
{
    public sealed class ChatClient : IDisposable
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly JsonSerializerSettings _jsonSettings;
        private bool _disposed;

        private readonly Queue<Tuple<string, string, object, string>> _pendingMessages = new();
        private bool _processingPendingMessages;

        public event EventHandler<ConnectionStatusEventArgs> ConnectionStatusChanged;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;
        public event EventHandler<MessageCreatedEventArgs> MessageCreated;
        public event EventHandler<MessageUpdatedEventArgs> MessageUpdated;
        public event EventHandler<MessageDeletedEventArgs> MessageDeleted;
        public event EventHandler<HistoryReceivedEventArgs> HistoryReceived;
        public event EventHandler<MessageAcknowledgedEventArgs> MessageAcknowledged;
        public event EventHandler<RequestDeniedEventArgs> RequestDenied;

        public ChatClient(IWebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient ?? throw new ArgumentNullException(nameof(webSocketClient));

            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            _webSocketClient.OnOpen += HandleWebSocketOpen;
            _webSocketClient.OnClose += HandleWebSocketClose;
            _webSocketClient.OnError += HandleWebSocketError;
            _webSocketClient.OnMessage += HandleWebSocketMessage;
        }

        public WebSocketState ConnectionState => _webSocketClient.State;

        public async UniTask ConnectAsync(string url, string authToken)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrEmpty(authToken)) throw new ArgumentNullException(nameof(authToken));

            ElephantLog.Log("ChatClient", $"ConnectAsync called with URL: {url}");

            if (_webSocketClient.State == WebSocketState.Open)
            {
                ElephantLog.Log("ChatClient", "Already connected.");
                return;
            }

            var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {authToken}" } };

            try
            {
                await _webSocketClient.ConnectAsync(url, headers);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Connection failed: {ex.Message}");
                NotifyError("Connection Failed", ex.Message);
                NotifyConnectionStatusChanged(false, "Connection Exception");
                throw;
            }
        }

        public async UniTask DisconnectAsync()
        {
            if (_webSocketClient.State == WebSocketState.Closed || _webSocketClient.State == WebSocketState.Closing)
            {
                return;
            }

            try
            {
                await _webSocketClient.CloseAsync();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Disconnection error - {ex.Message}");
                NotifyError("Disconnection Error", ex.Message);
                NotifyConnectionStatusChanged(false, "Disconnection Exception");
            }
        }

        public async UniTask<AckPayloadWrapper> SendMessageAsync(
            string channelId,
            string messageType,
            object payload,
            bool waitForAck = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(channelId)) throw new ArgumentNullException(nameof(channelId));
            if (string.IsNullOrEmpty(messageType)) throw new ArgumentNullException(nameof(messageType));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            string sourceEventId = Guid.NewGuid().ToString();

            if (_webSocketClient.State != WebSocketState.Open)
            {
                _pendingMessages.Enqueue(
                    new Tuple<string, string, object, string>(channelId, messageType, payload, sourceEventId));

                ElephantLog.Log("ChatClient", $"WebSocket not open, message queued: {messageType} to {channelId}");

                if (_pendingMessages.Count > 50)
                {
                    ElephantLog.Log("ChatClient",
                        $"Pending messages queue is large: {_pendingMessages.Count} messages");
                }

                if (_webSocketClient.State == WebSocketState.Closed)
                {
                    NotifyError("Send Error",
                        "Not connected to the server. Message queued for delivery after reconnection.");
                }

                return null;
            }

            UniTaskCompletionSource<AckPayloadWrapper> ackTcs = null;
            EventHandler<MessageAcknowledgedEventArgs> ackHandler = null;

            if (waitForAck)
            {
                ackTcs = new UniTaskCompletionSource<AckPayloadWrapper>();
            }

            try
            {
                var messageWrapper = new
                {
                    id = sourceEventId,
                    type = messageType,
                    channel = channelId,
                    payload
                };

                string jsonMessage = JsonConvert.SerializeObject(messageWrapper, Formatting.None, _jsonSettings);

                ElephantLog.Log("ChatClient", $"Sending {messageType} to {channelId} with ID: {sourceEventId}");

                if (waitForAck)
                {
                    ackHandler = (sender, args) =>
                    {
                        try
                        {
                            ElephantLog.Log("ChatClient",
                                $"Received ack for channel {args.ChannelId}, client message ID: {args.AckInfo?.SourceEventId}, message ID: {sourceEventId}");

                            if (args.ChannelId == channelId &&
                                args.AckInfo != null &&
                                args.AckInfo.SourceEventId == sourceEventId)
                            {
                                ackTcs.TrySetResult(args.AckInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            ElephantLog.LogError("ChatClient", $"Error in ack handler: {ex.Message}");
                        }
                    };

                    MessageAcknowledged += ackHandler;
                }

                await _webSocketClient.SendTextAsync(jsonMessage);

                if (waitForAck)
                {
                    try
                    {
                        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        timeoutCts.CancelAfterSlim(TimeSpan.FromSeconds(10));

                        try
                        {
                            var result = await ackTcs.Task.AttachExternalCancellation(timeoutCts.Token);
                            ElephantLog.Log("ChatClient", $"Successfully received ack for message ID: {sourceEventId}");
                            return result;
                        }
                        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested &&
                                                                 !cancellationToken.IsCancellationRequested)
                        {
                            ElephantLog.LogError("ChatClient",
                                $"Acknowledgment timed out for message ID: {sourceEventId}");
                            throw new TimeoutException("Server acknowledgment timed out");
                        }
                    }
                    finally
                    {
                        if (ackHandler != null)
                        {
                            MessageAcknowledged -= ackHandler;
                        }
                    }
                }

                return null;
            }
            catch (JsonException jsonEx)
            {
                ElephantLog.LogError("ChatClient", $"JSON Serialization Error - {jsonEx.Message}");
                NotifyError("Serialization Error", jsonEx.Message);
                throw;
            }
            catch (TimeoutException)
            {
                ElephantLog.LogError("ChatClient",
                    $"Timeout waiting for acknowledgment for message ID: {sourceEventId}");
                throw;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Failed to send message - {ex.Message}");
                NotifyError("Send Error", ex.Message);

                if (!(ex is OperationCanceledException))
                {
                    _pendingMessages.Enqueue(
                        new Tuple<string, string, object, string>(channelId, messageType, payload, sourceEventId));
                }

                throw;
            }
            finally
            {
                if (waitForAck && ackHandler != null)
                {
                    MessageAcknowledged -= ackHandler;
                }
            }
        }

        private void ProcessPendingMessages()
        {
            ProcessPendingMessagesAsync().Forget();
        }

        private async UniTaskVoid ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
        {
            if (_processingPendingMessages)
                return;

            _processingPendingMessages = true;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_pendingMessages.Count == 0)
                        break;

                    var pendingMessage = _pendingMessages.Dequeue();

                    try
                    {
                        ElephantLog.Log("ChatClient",
                            $"Sending queued message: {pendingMessage.Item2} to {pendingMessage.Item1}");

                        await SendMessageAsync(
                            pendingMessage.Item1,
                            pendingMessage.Item2,
                            pendingMessage.Item3);
                    }
                    catch (OperationCanceledException)
                    {
                        _pendingMessages.Enqueue(pendingMessage);
                        break;
                    }
                    catch (Exception ex)
                    {
                        ElephantLog.LogError("ChatClient", $"Error sending queued message: {ex.Message}");
                    }

                    try
                    {
                        await UniTask.Delay(50, cancellationToken: cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Error processing pending messages: {ex.Message}");
            }
            finally
            {
                _processingPendingMessages = false;
            }
        }

        private void HandleWebSocketOpen()
        {
            NotifyConnectionStatusChanged(true);
            ProcessPendingMessages();
        }

        private void HandleWebSocketClose(string reason)
        {
            NotifyConnectionStatusChanged(false, reason);
        }

        private void HandleWebSocketError(string errorMessage)
        {
            NotifyError("WebSocket Error", errorMessage);
            if (_webSocketClient.State != WebSocketState.Closed && _webSocketClient.State != WebSocketState.Closing)
            {
                NotifyConnectionStatusChanged(false, "WebSocket Error Occurred");
            }
        }

        private void HandleWebSocketMessage(byte[] messageBytes)
        {
            try
            {
                if (messageBytes == null || messageBytes.Length == 0)
                {
                    ElephantLog.LogError("ChatClient", "Received empty message");
                    return;
                }

                string bytesHex = BitConverter.ToString(messageBytes.Take(Math.Min(10, messageBytes.Length)).ToArray());
                ElephantLog.Log("ChatClient", $"Received message bytes (first 10): {bytesHex}");

                string jsonMessage = System.Text.Encoding.UTF8.GetString(messageBytes);

                if (!jsonMessage.StartsWith("{") || !jsonMessage.EndsWith("}"))
                {
                    ElephantLog.LogError("ChatClient",
                        $"Received non-JSON message: {(jsonMessage.Length > 50 ? jsonMessage.Substring(0, 47) + "..." : jsonMessage)}");
                    return;
                }

                if (jsonMessage.Length > 200)
                {
                    ElephantLog.Log("ChatClient", $"Received: {jsonMessage.Substring(0, 197)}...");
                }
                else
                {
                    ElephantLog.Log("ChatClient", $"Received: {jsonMessage}");
                }

                var incomingWrapper = JsonConvert.DeserializeObject<IncomingMessageWrapper>(jsonMessage, _jsonSettings);
                if (incomingWrapper == null)
                {
                    ElephantLog.LogError("ChatClient", "Received message with invalid format");
                    return;
                }

                if (string.IsNullOrEmpty(incomingWrapper.Type))
                {
                    ElephantLog.LogError("ChatClient", "Received message with missing type");
                    return;
                }

                if (string.IsNullOrEmpty(incomingWrapper.Channel))
                {
                    ElephantLog.LogError("ChatClient", "Received message with missing channel");
                    return;
                }

                JsonSerializer serializer = JsonSerializer.Create(_jsonSettings);

                try
                {
                    ProcessMessageByType(incomingWrapper, serializer);
                }
                catch (Exception ex)
                {
                    ElephantLog.LogError("ChatClient",
                        $"Error processing message of type {incomingWrapper.Type}: {ex.Message}");
                }
            }
            catch (JsonException jsonEx)
            {
                ElephantLog.LogError("ChatClient", $"JSON Deserialization Error - {jsonEx.Message}");
                NotifyError("Message Parse Error", jsonEx.Message);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Error handling received message - {ex.Message}");
                NotifyError("Message Handling Error", ex.Message);
            }
        }

        private void ProcessMessageByType(IncomingMessageWrapper incomingWrapper, JsonSerializer serializer)
        {
            switch (incomingWrapper.Type.ToLowerInvariant())
            {
                case "message_created":
                    var createdPayload = incomingWrapper.Payload?.ToObject<MessageContentPayload>(serializer);
                    if (createdPayload != null)
                    {
                        MessageCreated?.Invoke(this, new MessageCreatedEventArgs
                        {
                            ChannelId = incomingWrapper.Channel,
                            Message = createdPayload
                        });
                    }

                    break;

                case "message_updated":
                    var updatedPayload = incomingWrapper.Payload?.ToObject<MessageContentPayload>(serializer);
                    if (updatedPayload != null)
                    {
                        MessageUpdated?.Invoke(this, new MessageUpdatedEventArgs
                        {
                            ChannelId = incomingWrapper.Channel,
                            Message = updatedPayload
                        });
                    }

                    break;

                case "message_deleted":
                    var deletedPayload = incomingWrapper.Payload?.ToObject<DeletedMessagePayload>(serializer);
                    if (deletedPayload != null)
                    {
                        MessageDeleted?.Invoke(this, new MessageDeletedEventArgs
                        {
                            ChannelId = incomingWrapper.Channel,
                            DeletionInfo = deletedPayload
                        });
                    }

                    break;

                case "acked":
                    var ackWrapper = incomingWrapper.Payload?.ToObject<AckPayloadWrapper>(serializer);
                    if (ackWrapper == null) break;
                    HandleAckMessage(incomingWrapper.Channel, ackWrapper, serializer);
                    break;

                case "denied":
                    var deniedPayload = incomingWrapper.Payload?.ToObject<DeniedPayload>(serializer);
                    if (deniedPayload != null)
                    {
                        RequestDenied?.Invoke(this, new RequestDeniedEventArgs
                        {
                            ChannelId = incomingWrapper.Channel,
                            DenialInfo = deniedPayload
                        });
                    }

                    break;

                default:
                    ElephantLog.Log("ChatClient", $"Received unhandled message type: {incomingWrapper.Type}");
                    break;
            }
        }

        private void HandleAckMessage(string channelId, AckPayloadWrapper ackWrapper, JsonSerializer serializer)
        {
            switch (ackWrapper.PayloadType?.ToLowerInvariant())
            {
                case "history_delivered":
                    var historyPayload = ackWrapper.Payload?.ToObject<HistoryDeliveredPayload>(serializer);
                    if (historyPayload != null)
                    {
                        HistoryReceived?.Invoke(this, new HistoryReceivedEventArgs
                        {
                            ChannelId = channelId,
                            History = historyPayload
                        });
                    }

                    break;

                default:
                    try
                    {
                        MessageAcknowledged?.Invoke(this, new MessageAcknowledgedEventArgs
                        {
                            ChannelId = channelId,
                            AckInfo = ackWrapper
                        });
                    }
                    catch (Exception ex)
                    {
                        ElephantLog.LogError("ChatClient", $"Error invoking MessageAcknowledged: {ex.Message}");
                    }

                    break;
            }
        }

        private void NotifyConnectionStatusChanged(bool isConnected, string reason = "")
        {
            try
            {
                ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs
                {
                    IsConnected = isConnected,
                    Reason = reason
                });
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Error in ConnectionStatusChanged handler: {ex.Message}");
            }
        }

        private void NotifyError(string title, string message)
        {
            try
            {
                ErrorOccurred?.Invoke(this, new ErrorEventArgs { Title = title, Message = message });
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("ChatClient", $"Error in ErrorOccurred handler: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                if (_webSocketClient != null)
                {
                    _webSocketClient.OnOpen -= HandleWebSocketOpen;
                    _webSocketClient.OnClose -= HandleWebSocketClose;
                    _webSocketClient.OnError -= HandleWebSocketError;
                    _webSocketClient.OnMessage -= HandleWebSocketMessage;

                    if (_webSocketClient.State == WebSocketState.Open ||
                        _webSocketClient.State == WebSocketState.Connecting)
                    {
                        UniTask.Create(async () => await _webSocketClient.CloseAsync()).Forget();
                    }

                    if (_webSocketClient is IDisposable disposableClient)
                    {
                        disposableClient.Dispose();
                    }
                }

                _pendingMessages.Clear();
            }

            _disposed = true;
        }

        ~ChatClient()
        {
            Dispose(false);
        }
    }
}