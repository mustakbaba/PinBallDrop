using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Chat.Model;
using ElephantSocial.Chat.Core;
using ElephantSocial.Chat.Interface;
using ElephantSocial.Team;
using ElephantSDK;
using ElephantSocial.Team.Model.Enum;

namespace ElephantSocial.Chat.TeamChat
{
    public class TeamChatManager
    {
        private static string _teamId;
        private readonly string _socialId;
        private readonly string _channelId;
        private readonly string _chatContextKey;
        private readonly List<ChatMessage> _messages = new();
        private readonly HashSet<string> _helpedMessageIds = new();
        private static bool _isInitializing = false;
        private bool _isInitialized;
        private bool _isLoadingHistory;
        private bool _disposed;
        private bool _isInitializingChannel = false;
        private static TeamChatManager _instance;

        public event Action<ChatMessage> OnNewMessage;
        public event Action<ChatMessage> OnMessageUpdate;
        public event Action<ChatMessage> OnMessageDelete;
        public event Action<bool> OnChatAvailableStatusChanged;
        public event Action<string, string> OnError;

        public event Action<TextMessage> OnTextMessageReceived;
        public event Action<HelpMessage> OnHelpMessageReceived;
        public event Action<RewardMessage> OnRewardMessageReceived;
        public event Action<JoinRequestMessage> OnJoinRequestMessageReceived;
        public event Action<JoinAcceptMessage> OnJoinAcceptMessageReceived;
        public event Action<JoinRejectMessage> OnJoinRejectMessageReceived;
        public event Action OnPlayerKicked;

        /// <summary>
        /// Returns the current team's chat manager if a player is in a team, otherwise null.
        /// </summary>
        public static async UniTask<TeamChatManager> GetCurrentTeamChatAsync()
        {
            try
            {
                if (_isInitializing)
                {
                    await UniTask.WaitUntil(() => _isInitializing == false);
                }

                _isInitializing = true;

                if (!ElephantChat.IsInitialized)
                {
                    await ElephantChat.ConnectAsync();
                }

                var teamId = await TeamManager.GetMyTeamId();

                if (_instance != null && teamId == _teamId && _instance._isInitialized)
                {
                    _isInitializing = false;
                    return _instance;
                }

                _teamId = teamId;

                if (!string.IsNullOrEmpty(_teamId))
                {
                    _instance = await GetTeamChatAsync();
                    _isInitializing = false;
                    return _instance;
                }

                ElephantLog.Log("TeamChatManager", "Player is not in a team");
                return null;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamChatManager", $"Error in GetCurrentTeamChatAsync: {ex.Message}");
                return null;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        /// <summary>
        /// Retrieves chat history messages.
        /// </summary>
        public async UniTask<List<ChatMessage>> GetHistoryAsync(
            int limit = 100,
            string fromMessageId = "",
            CancellationToken cancellationToken = default)
        {
            if (!_isInitialized || ElephantChat.ConnectionState != WebSocketState.Open)
            {
                OnError?.Invoke("Connection Error", "Not connected to chat");
                return new List<ChatMessage>();
            }

            if (_isLoadingHistory)
                return new List<ChatMessage>();

            _isLoadingHistory = true;

            try
            {
                var requestId = Guid.NewGuid().ToString();
                var messages = new List<ChatMessage>();
                var historyTask = new UniTaskCompletionSource<List<ChatMessage>>();

                EventHandler<HistoryReceivedEventArgs> historyHandler = null;
                historyHandler = (_, args) =>
                {
                    if (args.ChannelId != _channelId) return;

                    try
                    {
                        if (args.History?.Messages != null)
                        {
                            foreach (var payload in args.History.Messages)
                            {
                                if (payload.PayloadType?.ToLowerInvariant() == "join_request")
                                    continue;

                                var message = ConvertPayloadToChatMessage(payload);
                                if (message != null)
                                {
                                    messages.Add(message);
                                }
                            }
                        }

                        historyTask.TrySetResult(messages);
                    }
                    catch (Exception ex)
                    {
                        historyTask.TrySetException(ex);
                    }
                };

                ElephantChat.Client.HistoryReceived += historyHandler;

                try
                {
                    var payload = new { from_message_id = fromMessageId, limit, request_id = requestId };

                    var historyRequestTask =
                        ElephantChat.Client.SendMessageAsync(_channelId, "history_requested", payload);
                    var joinRequestsTask = GetJoinRequestsInParallel();
                    await historyRequestTask;

                    var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfterSlim(TimeSpan.FromSeconds(10));

                    try
                    {
                        var historyResult = await historyTask.Task.AttachExternalCancellation(timeoutCts.Token);
                        var joinRequests = await joinRequestsTask;

                        if (joinRequests?.Count > 0)
                        {
                            historyResult.AddRange(joinRequests);
                        }

                        foreach (var message in historyResult.Where(message => _messages.All(m => m.ID != message.ID)))
                        {
                            _messages.Add(message);
                            if (message is HelpMessage helpMessage && _helpedMessageIds.Contains(message.ID))
                            {
                                helpMessage.Help();
                            }
                        }

                        _messages.Sort((a, b) =>
                        {
                            var aIsJoinRequest = a is JoinRequestMessage;
                            var bIsJoinRequest = b is JoinRequestMessage;

                            if (aIsJoinRequest && !bIsJoinRequest)
                                return 1;

                            if (!aIsJoinRequest && bIsJoinRequest)
                                return -1;

                            return String.Compare(a.ID, b.ID, StringComparison.Ordinal);
                        });

                        return historyResult;
                    }
                    catch (OperationCanceledException)
                    {
                        ElephantLog.Log("TeamChatManager", "History request timed out");
                        throw;
                    }
                }
                finally
                {
                    ElephantChat.Client.HistoryReceived -= historyHandler;
                }
            }
            catch (OperationCanceledException e)
            {
                ElephantLog.Log("TeamChatManager", $"History request canceled: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                OnError?.Invoke("History Error", "Failed to get message history: " + ex.Message);
                return new List<ChatMessage>();
            }
            finally
            {
                _isLoadingHistory = false;
            }
        }

        private async UniTask<List<ChatMessage>> GetJoinRequestsInParallel()
        {
            try
            {
                var team = await TeamManager.GetTeam(_teamId);

                if (team.id != null)
                {
                    var currentMember = team.GetTeamMembers().Find(m => m.id == _socialId);

                    if (currentMember != null &&
                        (currentMember.role == TeamMemberRole.LEADER ||
                         currentMember.role == TeamMemberRole.COLEADER))
                    {
                        var joinRequests = await team.GetJoinRequestsAsync();

                        return joinRequests.Select(request => new JoinRequestMessage
                        {
                            ID = $"join_request_{request.id}_{DateTime.UtcNow.Ticks}",
                            RequestingSocialId = request.id,
                            RequestingPlayerName = request.name,
                            RequestingProfilePicture = request.profilePicture,
                            CreatedAt = DateTime.UtcNow.Ticks,
                            UpdatedAt = DateTime.UtcNow.Ticks,
                            Sender = new TeamMember { id = request.id, name = request.name }
                        }).Cast<ChatMessage>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamChatManager", $"Error fetching join requests: {ex.Message}");
            }

            return new List<ChatMessage>();
        }

        /// <summary>
        /// Sends a text message to the chat.
        /// </summary>
        public async UniTask SendTextMessageAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                OnError?.Invoke("Input Error", "Message cannot be empty");
                return;
            }

            if (!_isInitialized || ElephantChat.ConnectionState != WebSocketState.Open)
            {
                OnError?.Invoke("Connection Error", "Not connected to chat");
                return;
            }

            try
            {
                var ack = await ElephantChat.Client.SendMessageAsync(
                    _channelId,
                    "new_message",
                    new { body = text },
                    true,
                    cancellationToken
                );

                if (ack == null)
                {
                    OnError?.Invoke("Send Error", "Failed to receive acknowledgment from server");
                }
            }
            catch (OperationCanceledException e)
            {
                ElephantLog.Log("TeamChatManager", $"SendTextMessageAsync canceled: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Send Error", "Failed to send message. Reason: " + ex.Message);
            }
        }

        /// <summary>
        /// Requests help with a specified amount.
        /// </summary>
        public async UniTask RequestHelpAsync(int requested, CancellationToken cancellationToken = default)
        {
            if (requested <= 0)
            {
                OnError?.Invoke("Input Error", "Help request amount must be positive");
                return;
            }

            if (!_isInitialized || ElephantChat.ConnectionState != WebSocketState.Open)
            {
                OnError?.Invoke("Connection Error", "Not connected to chat");
                return;
            }

            try
            {
                var ack = await ElephantChat.Client.SendMessageAsync(
                    _channelId,
                    "request_help",
                    new { requested },
                    true,
                    cancellationToken
                );

                if (ack == null)
                {
                    OnError?.Invoke("Send Error", "Failed to receive acknowledgment for help request");
                }
            }
            catch (OperationCanceledException e)
            {
                ElephantLog.Log("TeamChatManager", $"RequestHelpAsync canceled: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Send Error", "Failed to send help request. Reason: " + ex.Message);
            }
        }

        /// <summary>
        /// Sends help for a specific message ID.
        /// </summary>
        public async UniTask SendHelpAsync(string targetMessageId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(targetMessageId))
            {
                OnError?.Invoke("Input Error", "Target message ID is required");
                return;
            }

            if (!_isInitialized || ElephantChat.ConnectionState != WebSocketState.Open)
            {
                OnError?.Invoke("Connection Error", "Not connected to chat");
                return;
            }

            try
            {
                var ack = await ElephantChat.Client.SendMessageAsync(
                    _channelId,
                    "send_help",
                    new { target_message_id = targetMessageId },
                    true,
                    cancellationToken
                );

                if (ack == null)
                {
                    OnError?.Invoke("Send Error", "Failed to receive acknowledgment for help");
                    return;
                }

                _helpedMessageIds.Add(targetMessageId);
                if (_messages.FirstOrDefault(m => m.ID == targetMessageId) is HelpMessage helpMessage)
                {
                    helpMessage.Help();
                    OnMessageUpdate?.Invoke(helpMessage);
                    OnHelpMessageReceived?.Invoke(helpMessage);
                }
            }
            catch (OperationCanceledException e)
            {
                ElephantLog.Log("TeamChatManager", $"SendHelpAsync canceled: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Send Error", "Failed to send help. Reason: " + ex.Message);
            }
        }

        private TeamChatManager()
        {
            _channelId = $"team_{_teamId}";
            _chatContextKey = _channelId;
            _socialId = Social.Instance.GetPlayer()?.socialId ?? string.Empty;

            ElephantLog.Log("TeamChatManager", $"Initializing team chat for team {_teamId}");

            ElephantChat.Client.ConnectionStatusChanged += HandleConnectionStatusChanged;
            ElephantChat.Client.ErrorOccurred += HandleChatClientError;
            ElephantChat.Client.RequestDenied += HandleRequestDenied;
            ElephantChat.Client.MessageCreated += HandleMessageCreated;
            ElephantChat.Client.MessageUpdated += HandleMessageUpdated;
            ElephantChat.Client.MessageDeleted += HandleMessageDeleted;
            ElephantChat.OnConnectionStatusChanged += HandleElephantChatConnectionStatus;
        }

        ~TeamChatManager()
        {
            ElephantChat.Client.ConnectionStatusChanged -= HandleConnectionStatusChanged;
            ElephantChat.Client.ErrorOccurred -= HandleChatClientError;
            ElephantChat.Client.RequestDenied -= HandleRequestDenied;
            ElephantChat.Client.MessageCreated -= HandleMessageCreated;
            ElephantChat.Client.MessageUpdated -= HandleMessageUpdated;
            ElephantChat.Client.MessageDeleted -= HandleMessageDeleted;
            ElephantChat.OnConnectionStatusChanged -= HandleElephantChatConnectionStatus;
        }

        private static async UniTask<TeamChatManager> GetTeamChatAsync()
        {
            try
            {
                var chatManager = new TeamChatManager();
                await chatManager.InitializeChannelAsync();

                return chatManager;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamChatManager", $"Error creating team chat manager: {ex.Message}");
                throw;
            }
        }

        private async UniTask InitializeChannelAsync()
        {
            if (_isInitialized && _channelId != $"team_{_teamId}")
            {
                _messages.Clear();
                _helpedMessageIds.Clear();
                _isInitialized = false;
            }

            if (_isInitializingChannel)
            {
                ElephantLog.Log(_chatContextKey, $"Channel initialization for {_channelId} already in progress.");
                return;
            }

            if (_isInitialized && ElephantChat.ConnectionState == WebSocketState.Open)
            {
                ElephantLog.Log(_chatContextKey, $"Channel {_channelId} already initialized and ElephantChat is open.");
                OnChatAvailableStatusChanged?.Invoke(true);
                return;
            }

            _isInitializingChannel = true;
            try
            {
                if (ElephantChat.ConnectionState != WebSocketState.Open)
                {
                    if (_isInitialized)
                    {
                        _isInitialized = false;
                    }

                    OnChatAvailableStatusChanged?.Invoke(false);
                    OnError?.Invoke("Connection Error", "Chat service not connected for channel initialization.");
                    ElephantLog.Log(_chatContextKey,
                        $"Cannot initialize channel {_channelId}, ElephantChat is not open. State: {ElephantChat.ConnectionState}");
                    return;
                }

                ElephantLog.Log(_chatContextKey, $"Attempting to send 'connect' for channel: {_channelId}.");
                await ElephantChat.Client.SendMessageAsync(_channelId, "connect", new { });
                _isInitialized = true;
                OnChatAvailableStatusChanged?.Invoke(true);
                ElephantLog.Log(_chatContextKey,
                    $"Channel 'connect' sent successfully for {_channelId}. Marked as initialized.");
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                OnChatAvailableStatusChanged?.Invoke(false);
                OnError?.Invoke("Initialization Error",
                    $"Failed to connect to chat channel {_channelId}. Reason: " + ex.Message);
                ElephantLog.LogError(_chatContextKey,
                    $"Failed to send 'connect' for channel {_channelId}: {ex.Message}");
            }
            finally
            {
                _isInitializingChannel = false;
            }
        }

        private void HandleElephantChatConnectionStatus(bool isConnected)
        {
            if (_disposed) return;

            if (isConnected)
            {
                if (!_isInitialized)
                {
                    ElephantLog.Log(_chatContextKey,
                        $"ElephantChat connected. TeamChatManager (channel: {_channelId}) is not initialized. Attempting to initialize channel.");
                    InitializeChannelAsync().Forget();
                }
                else
                {
                    OnChatAvailableStatusChanged?.Invoke(true);
                    ElephantLog.Log(_chatContextKey,
                        $"ElephantChat connected. TeamChatManager (channel: {_channelId}) was already initialized.");
                }
            }
            else
            {
                if (_isInitialized)
                {
                    _isInitialized = false;
                    OnChatAvailableStatusChanged?.Invoke(false);
                    ElephantLog.Log(_chatContextKey,
                        $"ElephantChat disconnected. TeamChatManager (channel: {_channelId}) marked as not initialized.");
                }
                else
                {
                    OnChatAvailableStatusChanged?.Invoke(false);
                    ElephantLog.Log(_chatContextKey,
                        $"ElephantChat disconnected. TeamChatManager (channel: {_channelId}) was already not initialized.");
                }
            }
        }

        private void HandleConnectionStatusChanged(object sender, ConnectionStatusEventArgs args)
        {
            if (_disposed) return;

            if (!args.IsConnected)
            {
                _isInitialized = false;
            }

            OnChatAvailableStatusChanged?.Invoke(args.IsConnected && _isInitialized);
        }

        private void HandleChatClientError(object sender, ErrorEventArgs args)
        {
            if (_disposed) return;
            OnError?.Invoke(args.Title, args.Message);
        }

        private void HandleRequestDenied(object sender, RequestDeniedEventArgs args)
        {
            if (_disposed || args.ChannelId != _channelId) return;
            OnError?.Invoke("Request Denied", args.DenialInfo.Reason);
        }

        private void HandleMessageCreated(object sender, MessageCreatedEventArgs args)
        {
            if (_disposed || args.ChannelId != _channelId) return;

            try
            {
                switch (args.Message?.PayloadType?.ToLowerInvariant())
                {
                    case "kick":
                        OnPlayerKicked?.Invoke();
                        TeamService.TriggerTeamLeft();
                        return;
                    case "join_request":
                        RefreshJoinRequests();
                        return;
                    case "join_accept":
                        TeamService.TriggerTeamJoined();
                        return;
                }

                var message = ConvertPayloadToChatMessage(args.Message);
                if (message == null) return;

                if (_messages.Any(m => m.ID == message.ID)) return;

                _messages.Add(message);

                _messages.Sort((a, b) =>
                {
                    var aIsJoinRequest = a is JoinRequestMessage;
                    var bIsJoinRequest = b is JoinRequestMessage;

                    if (aIsJoinRequest && !bIsJoinRequest)
                        return 1;

                    if (!aIsJoinRequest && bIsJoinRequest)
                        return -1;

                    return String.Compare(a.ID, b.ID, StringComparison.Ordinal);
                });

                OnNewMessage?.Invoke(message);

                switch (message)
                {
                    case TextMessage textMessage:
                        OnTextMessageReceived?.Invoke(textMessage);
                        break;
                    case HelpMessage helpMessage:
                        OnHelpMessageReceived?.Invoke(helpMessage);
                        break;
                    case RewardMessage rewardMessage:
                        OnRewardMessageReceived?.Invoke(rewardMessage);
                        break;
                    case JoinAcceptMessage joinAccept:
                        OnJoinAcceptMessageReceived?.Invoke(joinAccept);
                        RefreshJoinRequests();
                        break;
                    case JoinRejectMessage joinReject:
                        OnJoinRejectMessageReceived?.Invoke(joinReject);
                        RefreshJoinRequests();
                        break;
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError(_chatContextKey, $"Error handling message created: {ex.Message}");
            }
        }

        private void HandleMessageUpdated(object sender, MessageUpdatedEventArgs args)
        {
            if (_disposed || args.ChannelId != _channelId) return;

            try
            {
                ElephantLog.Log(_chatContextKey, $"Message updated: {args.Message?.Id}");
                var message = ConvertPayloadToChatMessage(args.Message);
                if (message == null) return;

                int index = _messages.FindIndex(m => m.ID == message.ID);
                if (index >= 0)
                {
                    _messages[index] = message;
                }
                else
                {
                    _messages.Add(message);
                    _messages.Sort((a, b) => String.Compare(a.ID, b.ID, StringComparison.Ordinal));
                }

                OnMessageUpdate?.Invoke(message);

                switch (message)
                {
                    case TextMessage textMessage:
                        OnTextMessageReceived?.Invoke(textMessage);
                        break;
                    case HelpMessage helpMessage:
                        OnHelpMessageReceived?.Invoke(helpMessage);
                        break;
                    case RewardMessage rewardMessage:
                        OnRewardMessageReceived?.Invoke(rewardMessage);
                        break;
                    case JoinRequestMessage joinRequest:
                        OnJoinRequestMessageReceived?.Invoke(joinRequest);
                        break;
                    case JoinAcceptMessage joinAccept:
                        OnJoinAcceptMessageReceived?.Invoke(joinAccept);
                        break;
                    case JoinRejectMessage joinReject:
                        OnJoinRejectMessageReceived?.Invoke(joinReject);
                        break;
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError(_chatContextKey, $"Error handling message updated: {ex.Message}");
            }
        }

        private void HandleMessageDeleted(object sender, MessageDeletedEventArgs args)
        {
            if (_disposed || args.ChannelId != _channelId) return;

            try
            {
                ElephantLog.Log(_chatContextKey, $"Message deleted: {args.DeletionInfo?.Id}");
                if (args.DeletionInfo == null) return;

                ChatMessage deletedMessage = null;

                int index = _messages.FindIndex(m => m.ID == args.DeletionInfo.Id);
                if (index >= 0)
                {
                    deletedMessage = _messages[index];
                    _messages.RemoveAt(index);
                }

                if (deletedMessage != null)
                {
                    OnMessageDelete?.Invoke(deletedMessage);
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError(_chatContextKey, $"Error handling message deleted: {ex.Message}");
            }
        }

        private ChatMessage ConvertPayloadToChatMessage(MessageContentPayload payload)
        {
            if (payload == null) return null;

            try
            {
                ChatMessage message = null;
                var specificPayload = payload.SpecificPayload;

                switch (payload.PayloadType?.ToLowerInvariant())
                {
                    case "text":
                        if (specificPayload?.TextMessage != null)
                        {
                            message = new TextMessage
                            {
                                Text = specificPayload.TextMessage,
                                Type = ChatMessageType.TEXT
                            };
                        }

                        break;

                    case "help_request":
                        if (specificPayload?.HelpRequest != null)
                        {
                            var data = specificPayload.HelpRequest;
                            var helpMsg = new HelpMessage(data.Requested)
                            {
                                Received = data.Received,
                                Senders = data.Senders ?? new List<string>()
                            };

                            if (_helpedMessageIds.Contains(payload.Id)) helpMsg.Help();

                            message = helpMsg;
                            helpMsg.Type = ChatMessageType.HELP;
                        }

                        break;

                    case "reward":
                        if (specificPayload?.Reward != null)
                        {
                            var data = specificPayload.Reward;
                            var rewardMsg = new RewardMessage
                            {
                                RewardId = data.RewardId,
                                MaxReceiver = data.MaxReceiver,
                                Receivers = data.Receivers ?? new List<string>(),
                                Type = ChatMessageType.REWARD
                            };
                            message = rewardMsg;
                        }

                        break;

                    case "join_request":
                        if (specificPayload?.JoinRequest != null)
                        {
                            var data = specificPayload.JoinRequest;
                            var joinReq = new JoinRequestMessage
                            {
                                RequestingSocialId = data.SocialId,
                                RequestingPlayerName = data.PlayerName,
                                RequestingProfilePicture = data.ProfilePicture,
                                Type = ChatMessageType.JOIN_REQUEST
                            };
                            message = joinReq;
                        }

                        break;

                    case "join_accept":
                        if (specificPayload?.JoinAccept != null)
                        {
                            var data = specificPayload.JoinAccept;
                            var joinAccept = new JoinAcceptMessage
                            {
                                SocialId = data.SocialId,
                                PlayerName = data.PlayerName,
                                ProfilePicture = data.ProfilePicture,
                                TargetSocialId = data.TargetSocialId,
                                TargetPlayerName = data.TargetPlayerName,
                                TargetProfilePicture = data.TargetProfilePicture,
                                Type = ChatMessageType.JOIN_ACCEPT
                            };
                            message = joinAccept;
                        }

                        break;

                    case "join_reject":
                        if (specificPayload?.JoinReject != null)
                        {
                            var data = specificPayload.JoinReject;
                            var joinReject = new JoinRejectMessage
                            {
                                SocialId = data.SocialId,
                                PlayerName = data.PlayerName,
                                ProfilePicture = data.ProfilePicture,
                                TargetSocialId = data.TargetSocialId,
                                TargetPlayerName = data.TargetPlayerName,
                                TargetProfilePicture = data.TargetProfilePicture,
                                Type = ChatMessageType.JOIN_REJECT
                            };
                            message = joinReject;
                        }

                        break;

                    case "kick":
                        if (specificPayload?.Kick != null)
                        {
                            var data = specificPayload.Kick;
                            var kick = new KickMessage()
                            {
                                SocialId = data.SocialId,
                                PlayerName = data.PlayerName,
                                ProfilePicture = data.ProfilePicture,
                                TargetSocialId = data.TargetSocialId,
                                TargetPlayerName = data.TargetPlayerName,
                                TargetProfilePicture = data.TargetProfilePicture,
                                Type = ChatMessageType.KICK
                            };
                            message = kick;
                        }

                        break;
                }

                if (message == null) return null;
                message.ID = payload.Id;

                var name = payload.SenderName;

                if (payload.SenderId == "dashboard-admin" || payload.SenderId == "system-admin")
                {
                    name = payload.SenderId;
                }
                else if (string.IsNullOrEmpty(name))
                {
                    name = "Unknown";
                }

                message.Sender = new TeamMember
                {
                    id = payload.SenderId,
                    name = name,
                    profilePicture = payload.SenderProfilePicture
                };
                message.CreatedAt = payload.CreatedAt;
                message.UpdatedAt = payload.UpdatedAt;

                return message;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError(_chatContextKey, $"Error converting payload to chat message: {ex.Message}");
                return null;
            }
        }

        private void RefreshJoinRequests()
        {
            RefreshJoinRequestsAsync().Forget();
        }

        private async UniTaskVoid RefreshJoinRequestsAsync()
        {
            try
            {
                var team = await TeamManager.GetTeam(_teamId);

                if (string.IsNullOrEmpty(_teamId) || string.IsNullOrEmpty(_socialId))
                    return;

                var currentMember = team.GetTeamMembers().Find(m => m.id == _socialId);

                if (currentMember == null ||
                    (currentMember.role != TeamMemberRole.LEADER &&
                     currentMember.role != TeamMemberRole.COLEADER))
                    return;

                var existingRequestIds = new HashSet<string>();

                foreach (var msg in _messages.OfType<JoinRequestMessage>())
                {
                    existingRequestIds.Add(msg.RequestingSocialId);
                }

                _messages.RemoveAll(m => m is JoinRequestMessage);

                var joinRequests = await team.GetJoinRequestsAsync();

                foreach (var request in joinRequests)
                {
                    var joinRequestMsg = new JoinRequestMessage
                    {
                        ID = $"join_request_{request.id}_{DateTime.UtcNow.Ticks}",
                        RequestingSocialId = request.id,
                        RequestingPlayerName = request.name,
                        RequestingProfilePicture = request.profilePicture,
                        CreatedAt = DateTime.UtcNow.Ticks,
                        UpdatedAt = DateTime.UtcNow.Ticks,
                        Sender = new TeamMember
                        {
                            id = request.id,
                            name = request.name
                        }
                    };

                    _messages.Add(joinRequestMsg);

                    var isNewRequest = !existingRequestIds.Contains(request.id);

                    if (isNewRequest)
                    {
                        OnNewMessage?.Invoke(joinRequestMsg);
                        OnJoinRequestMessageReceived?.Invoke(joinRequestMsg);
                    }
                }

                _messages.Sort((a, b) =>
                {
                    var aIsJoinRequest = a is JoinRequestMessage;
                    var bIsJoinRequest = b is JoinRequestMessage;

                    if (aIsJoinRequest && !bIsJoinRequest)
                        return 1;

                    if (!aIsJoinRequest && bIsJoinRequest)
                        return -1;

                    return String.Compare(a.ID, b.ID, StringComparison.Ordinal);
                });
            }
            catch (Exception ex)
            {
                ElephantLog.LogError(_chatContextKey, $"Error refreshing join requests: {ex.Message}");
            }
        }
    }
}