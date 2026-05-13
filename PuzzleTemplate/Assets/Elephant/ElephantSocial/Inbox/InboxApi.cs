using System;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Inbox.Model.Request;
using ElephantSocial.Inbox.Model.Response;
using ElephantSDK;

namespace ElephantSocial.Inbox
{
    public class InboxApi
    {
        private static readonly Lazy<InboxApi> _instance = new();
        public static InboxApi Instance => _instance.Value;

        public InboxApi()
        {
            _inboxOps = new InboxOps();
        }

        private readonly InboxOps _inboxOps;

        public UniTask<InboxResponse> GetInboxAsync()
        {
            try
            {
                var request = new GetInboxRequest();

                return _inboxOps.GetInboxAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("InboxApi", $"Error getting inbox: {ex.Message}");
                throw;
            }
        }
    
        public UniTask MarkAsReadAsync(int inboxItemId)
        {
            try
            {
                var request = new MarkAsReadRequest
                {
                    Id = inboxItemId
                };

                return _inboxOps.MarkAsReadAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("InboxApi", $"Error marking as read: {ex.Message}");
                throw;
            }
        }
    }
}