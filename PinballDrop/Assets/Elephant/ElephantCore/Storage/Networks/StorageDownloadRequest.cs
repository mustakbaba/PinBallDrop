using System;

namespace ElephantSDK
{
    [Serializable]
    public class StorageDownloadRequest: BaseData
    {
        public string client_id;
        public int version;
        
        public static StorageDownloadRequest Create(int storageVersion)
        {
            var storageDownloadRequest = new StorageDownloadRequest
            {
                client_id = ElephantCore.Instance.userId,
                version = storageVersion
            };
            storageDownloadRequest.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            
            return storageDownloadRequest;
        }
    }
}