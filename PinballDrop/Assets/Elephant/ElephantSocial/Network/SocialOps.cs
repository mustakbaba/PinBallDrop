using System;
using System.Collections;
using ElephantSDK;
using ElephantSocial.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace ElephantSocial
{
    public class SocialOps : GenericResponseOps
    {
        public IEnumerator GetPlayer(Action<GenericResponse<Player>> onResponse,
            Action<string> onError)
        {
            var data = new PlayerRequest();
            
            var timeout = RemoteConfig.GetInstance().GetInt("social_player_timeout", 5);

            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<Player>();
            var postWithResponse =
                networkManager.PostWithResponseSocial(IsProductionEnvironment() ? SocialConst.PlayerEp : SocialConstDev.PlayerEp,
                    bodyJson, onResponse, onError, timeout);

            return postWithResponse;
        }
        
        public IEnumerator UpdatePlayer(Player player, Action<GenericResponse<Player>> onResponseSuccess, Action<UnityWebRequest> onFailedResponse,
            Action<string> onError)
        {
            var data = new PlayerUpdateRequest(player);

            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<Player>();
            var postWithResponse =
                networkManager.PostWithResponseSocial(IsProductionEnvironment() ? SocialConst.PlayerUp : SocialConstDev.PlayerUp,
                    bodyJson, onResponseSuccess, onError, 0, false, onFailedResponse);

            return postWithResponse;
        }
    }
}