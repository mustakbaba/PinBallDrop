using System;
using System.Collections.Generic;
using System.Linq;

namespace ElephantSDK
{
    [Serializable]
    public class AdConfig
    {
        public string mopub_keyword = "";
        public bool ad_callback_logs = false;
        public bool ad_event_enabled = true;
        public bool backup_ads_enabled = false;
        public string backup_interstitial_ad_unit = "";
        public string backup_rewarded_ad_unit = "";
        public InterstitialAdLogic interstitial_ad_logic = new();
        public NetworkIds networks = null;
        public bool network_id_manipulation_enabled = true;
        public List<AdConfigParameter> parameters = new();
        public bool bidfloor_enabled = false;
        public bool bidfloor_int_enabled = false;
        public bool bidfloor_rw_enabled = false;
        public bool bidfloor_test_flow_enabled = false;
        
        public List<string> GetList(string key, List<string> def = null)
        {
            if(parameters.Count <= 0)  return def;

            AdConfigParameter adConfigParameter =
                parameters.Find(item => item.key.Equals(key));

            if (adConfigParameter == null) return def;
           

            var value = adConfigParameter.value;
            var list = value.Split(',').ToList();

            return list.Count > 0 ? list : def;
        }

        [Serializable]
        public class InterstitialAdLogic
        {
            public int reduce_value = 0;
            public int display_time_interval = 0;
            public int first_level_to_display = -1;
            public int level_frequency = -1;
        }

        [Serializable]
        public class AdConfigParameter
        {
            public string key;
            public string value;
        }
    }
}