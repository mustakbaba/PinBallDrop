using System;
using Newtonsoft.Json;

namespace ElephantSocial.Inbox.Model.Response
{
    [Serializable]
    public class InboxResponse
    {
        [JsonProperty("items")] 
        public InboxItem[] Items;
    }
}