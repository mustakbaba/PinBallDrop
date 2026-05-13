using System;
using System.Collections;
using Newtonsoft.Json;
namespace ElephantSDK
{
    public class StorageOps
    {
        public IEnumerator StorageDownload(int version, Action<StorageDownloadResponse> onComplete, Action<string> onError)
        {
            var data = StorageDownloadRequest.Create(version);
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<StorageDownloadResponse>();
            
            var timeOut = RemoteConfig.GetInstance().GetInt("storage_timeout", 10);

            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.StorageDownloadEp, bodyJson,
                response =>
                {
                    ElephantLog.Log("ELEPHANT-StorageOps", response.ToString());
                    if (response.data != null)
                    {
                        onComplete?.Invoke(response.data);
                    }
                    else
                    {
                        ElephantLog.LogError("ELEPHANT-StorageOps", "Response data is null");
                        onComplete?.Invoke(new StorageDownloadResponse());
                    }
                },
                error =>
                {
                    ElephantLog.LogError("ELEPHANT-StorageOps", error);
                    onError?.Invoke(error);
                }, timeout: timeOut);

            return postWithResponse;
        }

        public IEnumerator StorageUpload(Storage storage)
        {
            var data = StorageUploadRequest.FromStorage(storage);
            var json = JsonConvert.SerializeObject(data);
            var bodyJson =
                JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<StorageSyncResponse>();
            
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.StorageSyncEp, bodyJson,
                response =>
                {
                    ElephantLog.Log("StorageOps", response.ToString());
                },
                error =>
                {
                    ElephantLog.LogError("StorageOps", error);
                }, isPut: true);

            return postWithResponse;
        }
    }
}