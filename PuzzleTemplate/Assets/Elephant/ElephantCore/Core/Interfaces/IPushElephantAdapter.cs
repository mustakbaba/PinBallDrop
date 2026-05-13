using System;

namespace ElephantSDK
{
    public interface IPushElephantAdapter : IElephantAdapter
    {
        void SetDeviceToken(string token);
        
        void SendPushNotificationOpenEvent(string combinedIds);

        void ReceiveNotificationPermission(string response);
        
        void AskPushPermission();
    }
}