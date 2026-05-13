using System.Collections.Generic;

namespace ElephantSDK
{
    public interface IFacebookElephantAdapter : IElephantAdapter
    {
        void ActivateFacebook(string facebookAppId, string clientId);
        
        void AllowDataTracking();
        
        void LogAppEvent(string eventName, float? valueToSum, Dictionary<string, object> parameters);
    }
}