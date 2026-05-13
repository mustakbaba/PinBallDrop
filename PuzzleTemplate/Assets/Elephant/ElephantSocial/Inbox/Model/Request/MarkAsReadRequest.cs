using System;
using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Inbox.Model.Request
{
    [Serializable]
    public class MarkAsReadRequest : SocialBaseData
    {
        [JsonProperty("id")] 
        public int Id;
    }
}