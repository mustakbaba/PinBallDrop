using System;
using Newtonsoft.Json.Linq;

namespace ElephantSDK
{
    [Serializable]
    public class OpenData : BaseData 
    {
        public bool is_old_user;
        public bool gdpr_supported;
        public string hash;
        public string tc_string;
        public JObject ios_age_range_data;
        public JObject android_age_range_data;

        private OpenData()
        {
            
        }

        public static OpenData CreateOpenData()
        {
            var a = new OpenData();
            a.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            return a;
        }
    }
}