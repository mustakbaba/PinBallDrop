namespace ElephantSDK
{
    public class MDSRequest : BaseData
    {
        public string token;
        public string collectibleId;

        public static MDSRequest Create(string collectibleId = null)
        {
            var request = new MDSRequest();
            request.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            request.collectibleId = collectibleId;
            return request;
        }
    }
}