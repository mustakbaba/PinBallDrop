using System;

namespace ElephantSDK
{
    [Serializable]
    public class PersonalizedAdsPayload
    {
        public string title;
        public string content;
        public string privacy_policy_text;
        public string privacy_policy_url;
        public string decline_text_action_button;
        public string agree_text_action_button;
        public string consent_text_action_button;
    }
}