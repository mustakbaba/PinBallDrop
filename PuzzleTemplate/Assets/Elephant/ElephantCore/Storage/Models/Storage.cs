using System;

namespace ElephantSDK
{
    [Serializable]
    public class Storage
    {
        public int version;
        public string storageData;

        public static Storage FromDownloadResponse(StorageDownloadResponse response)
        {
            return new Storage
            {
                version = response.version,
                storageData = response.storage_data
            };
        }
    }
    
    [Serializable]
    public class StorageDownloadResponse
    {
        public int version;
        public string storage_data;
    }
    
    [Serializable]
    public class StorageUploadRequest : BaseData
    {       
        public int version; 
        public string storage_data;
        
        public static StorageUploadRequest FromStorage(Storage storage)
        {
            var storageUploadRequest = new StorageUploadRequest
            {
                version = storage.version,
                storage_data = storage.storageData
            };
            
            storageUploadRequest.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());

            return storageUploadRequest;
        }
    }
}