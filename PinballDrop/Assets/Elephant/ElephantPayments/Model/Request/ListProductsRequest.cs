using System;

namespace ElephantSDK
{
    [Serializable]
    public class ListProductsRequest : BaseData
    {
        public static ListProductsRequest Create()
        {
            var request = new ListProductsRequest();
            request.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            return request;
        }
    }
}