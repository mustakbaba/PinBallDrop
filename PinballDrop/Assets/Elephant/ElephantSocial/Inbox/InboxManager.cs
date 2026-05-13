using System;
using System.Collections.Generic;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Inbox.Model;
using ElephantSDK;

namespace ElephantSocial.Inbox
{
    public static class InboxManager
    {
        public static async UniTask<List<InboxItem>> GetInbox()
        {
            try
            {
                var inbox = await InboxService.GetInboxAsync();
                return inbox;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("InboxManager", $"Error getting inbox: {ex.Message}");
                return new List<InboxItem>();
            }
        }
        
        public static async UniTask<bool> MarkAsRead(int inboxItemId)
        {
            try
            {
                await InboxService.MarkAsReadAsync(inboxItemId);
                return true;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("InboxManager", $"Error marking inbox item as read: {ex.Message}");
                return false;
            }
        }
    }
}