using System;

namespace ElephantSDK
{
    [Serializable]
    public class DeviceTokenRequest : BaseData
    {
        public string device_token;
        
        public static DeviceTokenRequest CreateDeviceTokenRequest(string token)
        {
            var a = new DeviceTokenRequest();
            a.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            a.device_token = token;
            return a;
        }
    }
}