using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using ElephantSocial.Model;
using UnityEngine.Networking;

namespace ElephantSocial
{
    public class SocialIdPlayerCache : GenericResponseOps
    {
        private readonly Dictionary<string, Player> _cachedPlayers = new Dictionary<string, Player>();

        public void GetPlayer(string socialId, Action<Player> onResponse, Action<string> onFailed,
            Action<string> onError)
        {
            // Checking socialId is cached before
            if (IsPlayerCached(socialId, out Player cachedPlayer))
            {
                // Cached player found 
                onResponse?.Invoke(cachedPlayer);
                return;
            }

            // Requesting from API
            void OnFailedResponse(UnityWebRequest failedResponse) =>
                HandleErrorResponse(failedResponse, (errorCode, message) => onFailed?.Invoke(message));

            var getPlayerJob = GetPlayerWithSocialID(socialId, response =>
                {
                    CachePlayer(socialId, response.data);
                    onResponse?.Invoke(response.data);
                }, OnFailedResponse,
                error =>
                {
                    ElephantLog.LogError("Social", error);
                    onError?.Invoke(error);
                });

            ElephantCore.Instance.StartCoroutine(getPlayerJob);
        }

        private bool IsPlayerCached(string socialId, out Player cachedPlayer)
        {
            if (_cachedPlayers.TryGetValue(socialId, out var player))
            {
                cachedPlayer = player;
                return true;
            }

            cachedPlayer = null;
            return false;
        }

        private void CachePlayer(string socialId, Player player)
        {
            _cachedPlayers[socialId] = player;
        }

        private IEnumerator GetPlayerWithSocialID(string socialId, Action<GenericResponse<Player>> onResponse, Action<UnityWebRequest> onFailedResponse,
            Action<string> onError)
        {
            var data = new PlayerRequest();
            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<Player>();
            var url = IsProductionEnvironment() ? SocialConst.GetPlayerWithSocialIDEp : SocialConstDev.GetPlayerWithSocialIDEp;
            url = url.Replace("{socialId}", socialId);
            var postWithResponse = networkManager.PostWithResponseSocial(url, bodyJson, onResponse, onError, 0, false, onFailedResponse);
            return postWithResponse;
        }
    }
}