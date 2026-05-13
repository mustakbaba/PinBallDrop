using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using ElephantSocial.CachingSystem;
using ElephantSocial.Model;
using UnityEngine.Networking;

namespace ElephantSocial
{
    public class SocialInternal : SocialDataStore
    {
        public SocialConfig SocialConfig { get; private set; } = new SocialConfig(ElephantEnvironment.Production);
        private readonly SocialOps _socialOps = new SocialOps();
        private readonly SocialIdPlayerCache _socialIdPlayerCache = new SocialIdPlayerCache();

        private bool IsInitialized => _isInitialized;
        private bool _isInitialized;
        private const string PlayerDataStoreKey = "PlayerDataStoreKey";
        private bool _isPlayerLoaded;

        private Player _player;
        private Player Player
        {
            get
            {
                if (_isPlayerLoaded)
                    return _player;

                _isPlayerLoaded = true;
                _player = Load<Player>(PlayerDataStoreKey) ?? new Player();
                return _player;
            }
            set
            {
                _player = value;
                Save(PlayerDataStoreKey, _player);
                _isPlayerLoaded = true;
            }
        }

        public void Init(SocialConfig socialConfig, Action onSuccess, Action<string> onError)
        {
            SocialConfig = socialConfig;
            if (IsInitialized)
            {
                onError?.Invoke("Multiple init requested");
                ElephantLog.LogError("Social", "Multiple init requested");
                return;
            }

            InitPlayer(() => { onSuccess?.Invoke(); }, error =>
            {
                if (Player != null && !string.IsNullOrEmpty(Player.socialId))
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onError?.Invoke(error);
                    ElephantLog.LogError("Social", error);
                }
            });
        }

        private void InitPlayer(Action onResponse, Action<string> onError)
        {
            var getPlayerJob = _socialOps.GetPlayer(
                response =>
                {
                    Player = response.data;
                    onResponse?.Invoke();
                    _isInitialized = true;
                }, error =>
                {
                    ElephantLog.LogError("Social", error);
                    _isInitialized = false;
                    onError?.Invoke(error);
                });

            ElephantCore.Instance.StartCoroutine(getPlayerJob);
        }
        
        public Player GetPlayerAsync()
        {
            return Player;
        }

        public Player GetPlayer()
        {
            return Player.Clone();
        }

        public void GetPlayer(string socialId, Action<Player> response, Action<string> onFailed, Action<string> onError)
        {
            _socialIdPlayerCache.GetPlayer(socialId, response, onFailed, onError);
        }

        public void UpdatePlayer(Player newPlayer, Action onSuccess, Action<string> onFailed,
            Action<string> onError)
        {
            var updateNameJob = _socialOps.UpdatePlayer(newPlayer,
                response => HandleResponse(response,
                    successResponse =>
                    {
                        Player = successResponse;
                        onSuccess?.Invoke();
                    }, onError),
                failedResponse => HandleErrorResponse(failedResponse,
                    (errorCode, message) => onFailed?.Invoke(message)),
                onError
            );

            ElephantCore.Instance.StartCoroutine(updateNameJob);
        }
    }
}