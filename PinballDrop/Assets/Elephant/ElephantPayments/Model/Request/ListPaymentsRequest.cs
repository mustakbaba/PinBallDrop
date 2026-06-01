using System;

namespace ElephantSDK
{
    [Serializable]
    public class ListPaymentsRequest : BaseData
    {
        public static ListPaymentsRequest Create()
        {
            var request = new ListPaymentsRequest();
            request.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            return request;
        }
    }
}