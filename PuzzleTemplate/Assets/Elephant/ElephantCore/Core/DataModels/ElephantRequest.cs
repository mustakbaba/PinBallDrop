using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    [Serializable]
    public class ElephantRequest
    {
        public string url;
        public string data;
        public int tryCount;
        public long lastTryTS;
        public bool isOffline;
        public int statusCode;
       
        
        public ElephantRequest(string url, BaseData data)
        {
            this.url = url;
            this.data = JsonConvert.SerializeObject(data);
        }
        
        
        
    }
    
}