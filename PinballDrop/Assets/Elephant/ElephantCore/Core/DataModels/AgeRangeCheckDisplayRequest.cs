using System;

namespace ElephantSDK
{
    [Serializable]
    public class AgeRangeCheckDisplayRequest : BaseData
    {
        public static AgeRangeCheckDisplayRequest Create(long sessionId)
        {
            var req = new AgeRangeCheckDisplayRequest();
            req.FillBaseData(sessionId);
            return req;
        }
    }
}

