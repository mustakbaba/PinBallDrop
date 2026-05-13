using System;
using Newtonsoft.Json;
using ElephantSocial.Model;

namespace ElephantSocial.HonorWall
{
    [Serializable]
    public class HonorWallGrantRequest : SocialBaseData
    {
        [JsonProperty("id")] 
        public int id;
    }
}