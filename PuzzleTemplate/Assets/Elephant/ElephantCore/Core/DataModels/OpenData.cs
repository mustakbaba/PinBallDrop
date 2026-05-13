using System;

namespace ElephantSDK
{
    [Serializable]
    public class OpenData : BaseData 
    {
        public bool is_old_user;
        public bool gdpr_supported;
        public string hash;
        public string tc_string;

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