using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSDK
{
    public class CollectibleOps
    {
        public IEnumerator GetCollectibles(Action<List<Collectible>> onComplete, Action<string> onError)
        {
            var data = new BaseData();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<List<Collectible>>();
            var timeOut = RemoteConfig.GetInstance().GetInt("storage_timeout", 10);
            
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.CollectibleEp, bodyJson,
                response =>
                {
                    ElephantLog.Log("ELEPHANT-CollectibleOps", response.ToString());
                    if (response.data != null)
                    {
                        onComplete?.Invoke(response.data);
                    }
                    else
                    {
                        ElephantLog.LogError("ELEPHANT-CollectibleOps", "Response data is null");
                        onError?.Invoke("null");
                    }
                },
                error =>
                {
                    ElephantLog.LogError("ELEPHANT-CollectibleOps", error);
                    onError?.Invoke(error);
                }, timeOut);

            return postWithResponse;
        }
        
        public IEnumerator NotifyClaimed(string id)
        {
            var data = new BaseData();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<List<Collectible>>();
            
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.CollectibleEp + "/" + id + "/claim", bodyJson,
                response =>
                {
                    ElephantLog.Log("ELEPHANT-CollectibleOps", response.ToString());
                },
                error =>
                {
                    ElephantLog.LogError("ELEPHANT-CollectibleOps", error);
                });

            return postWithResponse;
        }
    }
}