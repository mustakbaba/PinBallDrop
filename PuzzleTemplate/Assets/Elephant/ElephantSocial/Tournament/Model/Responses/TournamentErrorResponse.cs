using System;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentErrorResponse
    {
        [JsonProperty("error_code")] 
        public long ErrorCode;
        [JsonProperty("msg")] 
        public string Message;
        
        public TournamentErrorResponse(long errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}