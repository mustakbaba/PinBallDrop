using System;
using ElephantSDK;
using UnityEngine.Networking;

namespace ElephantSocial
{
    public abstract class GenericResponseOps
    {
        private ElephantEnvironment ElephantEnvironment => Social.Instance.SocialConfig.elephantEnvironment;

        protected bool IsProductionEnvironment()
        {
            return ElephantEnvironment == ElephantEnvironment.Production;
        }

        protected string PrepareBodyJson<T>(T data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return json;
        }
        
        protected static void HandleResponse<T>(GenericResponse<T> response, Action<T> onResponse, Action<string> onError)
        {
            if (response == null)
            {
                onError( "Response is null");
                return;
            }

            if (response.responseCode == 200)
            {
                onResponse?.Invoke(response.data);
            }
            else if (response.responseCode == 201)
            {
                onResponse?.Invoke(default(T));
            }
            else
            {
                onError?.Invoke(response.errorMessage);
            }
        }
        
        protected static void HandleErrorResponse(UnityWebRequest request, Action<long, string> error)
        {
            var response = SocialUtils.GetTournamentErrorResponse(request);
            error?.Invoke(response.ErrorCode, response.Message);
        }
    }
}