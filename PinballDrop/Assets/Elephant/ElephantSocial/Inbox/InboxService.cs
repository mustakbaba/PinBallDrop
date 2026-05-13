using System;
using System.Collections.Generic;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Inbox.Model;
using ElephantSDK;

namespace ElephantSocial.Inbox
{
    public static class InboxService
    {
        public static async UniTask<List<InboxItem>> GetInboxAsync()
        {
            try
            {
                var response = await InboxApi.Instance.GetInboxAsync();
                return response?.Items != null ? new List<InboxItem>(response.Items) : new List<InboxItem>();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("InboxService", $"Error getting inbox: {ex.Message}");
                throw new InboxOperationException("Failed to retrieve inbox", ex);
            }
        }
        
        public static async UniTask MarkAsReadAsync(int inboxItemId)
        {
            try
            {
                await InboxApi.Instance.MarkAsReadAsync(inboxItemId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("InboxService", $"Error marking inbox item as read: {ex.Message}");
                throw new InboxOperationException("Failed to mark inbox item as read", ex);
            }
        }
    }
    
    public class InboxOperationException : Exception
    {
        public InboxOperationException(string message) : base(message) { }
        public InboxOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}