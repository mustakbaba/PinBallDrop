using System;

namespace ElephantSDK
{
    [Serializable]
    public class InternalConfig
    { 
        public bool monitoring_enabled = true;
        public bool crash_log_enabled = false;
        public bool low_memory_logging_enabled = false;
        public bool idfa_consent_enabled = false;
        public int idfa_consent_type = 3;
        public int idfa_consent_delay = 0;
        public int idfa_consent_position = 0;
        public string consent_text_body = "To play \"{{name}}\" you must agree to the {{terms}} and {{privacy}} from the developer of this app.";
        public string consent_text_action_body = "Press \"{{button}}\" to start playing!";
        public string consent_text_action_button = "Agree to Terms";
        public string terms_of_service_text = "Terms and Conditions";
        public string terms_of_service_url = "https://www.rollicgames.com/terms";
        public string privacy_policy_text = "Privacy Policy";
        public string privacy_policy_url = "https://www.rollicgames.com/privacy";
        public string min_app_version = "";
        public float focus_interval = 300;
        public bool request_logic_enabled = true;
        public bool reachability_check_enabled = false;
        public bool memory_usage_enabled = false;
        public bool storage_remote_enabled = true;
        public bool helpshift_enabled = false;
        public bool ads_disabled = false;
        public bool check_internet_connection = true;
        public bool dynamic_events_enabled = true;
    }
}