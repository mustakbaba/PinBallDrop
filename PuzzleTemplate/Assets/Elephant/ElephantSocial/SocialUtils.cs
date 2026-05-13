using System;
using ElephantSocial.Tournament.Model;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace ElephantSocial
{
    public class SocialUtils
    {
        public static TournamentErrorResponse GetTournamentErrorResponse(UnityWebRequest request)
        {
            try
            {
                return JsonConvert.DeserializeObject<TournamentErrorResponse>(request.downloadHandler.text) ?? new TournamentErrorResponse(0, "Error code is null");
            }
            catch (Exception e)
            {
                return new TournamentErrorResponse(-1, "Error parsing response, error: " + e.Message);
            }
        }
    }
}