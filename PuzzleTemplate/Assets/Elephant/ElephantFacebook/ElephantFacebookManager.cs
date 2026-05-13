using System.Collections.Generic;
using Facebook.Unity;

namespace ElephantSDK
{
    public class ElephantFacebookManager : IFacebookElephantAdapter
    {
        public void ActivateFacebook(string facebookAppId, string clientId)
        {
            ElephantLog.Log("FACEBOOK-ELEPHANT", "ActivateFacebook is Called");
            if (!FB.IsInitialized)
            {
                FB.Init(ElephantThirdPartyIds.FacebookAppId, clientToken: ElephantThirdPartyIds.FacebookClientToken, onInitComplete: OnFbInitComplete);
            }
            else
            {
                FB.ActivateApp();
                FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
                FB.Mobile.SetAdvertiserTrackingEnabled(false);
            }
        }
        
        private void OnFbInitComplete()
        {
            if (FB.IsInitialized) {
                FB.ActivateApp();
                FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
                FB.Mobile.SetAdvertiserTrackingEnabled(false);
            } else {
                ElephantLog.Log("ELEPHANT INIT","Failed to Initialize the Facebook SDK");
            }
        }

        public void AllowDataTracking()
        {
            ElephantLog.Log("FACEBOOK-ELEPHANT", "AllowDataTracking is Called");
            if (!FB.IsInitialized) return;
            FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
            FB.Mobile.SetAdvertiserTrackingEnabled(true);
        }
        
        public void LogAppEvent(string eventName, float? valueToSum, Dictionary<string, object> parameters)
        {
            FB.LogAppEvent(eventName, valueToSum, parameters);
        }
    }
}