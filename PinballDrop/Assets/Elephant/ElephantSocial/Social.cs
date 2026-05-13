using System;
using ElephantUniTask.Threading.Tasks;
using ElephantSDK;
using ElephantSocial.Chat.TeamChat;
using ElephantSocial.Model;

namespace ElephantSocial
{
    public class Social
    {
        public Action OnInitializeCallback;
        public Action<string> OnInitializeFailedCallback;
        public SocialConfig SocialConfig => _socialInternal.SocialConfig;

        private static readonly Lazy<Social> _instance = new Lazy<Social>(() => new Social());

        // Private constructor to prevent instantiation
        private Social() { }

        // Public accessor to get the instance
        public static Social Instance => _instance.Value;

        private SocialInternal _socialInternal = new SocialInternal();

        public void Init(SocialConfig socialConfig)
        {
            _socialInternal.Init(socialConfig, () =>
            {
                if (RemoteConfig.GetInstance().GetBool("elephant_chat_enabled", false))
                {
                    TeamChatManager.GetCurrentTeamChatAsync().Forget();
                }
                OnInitializeCallback?.Invoke();
            }, error =>
            {
                OnInitializeFailedCallback?.Invoke(error);
            });
        }
        
        public void UpdatePlayer(Player newPlayer, Action onSuccess, Action<string> onFailed, Action<string> onError)
        {
            _socialInternal.UpdatePlayer(newPlayer, onSuccess, onFailed, onError);
        }

        public Player GetPlayer()
        {
            return _socialInternal.GetPlayer();
        }

        public void GetPlayer(string socialId, Action<Player> response, Action<string> onFailed, Action<string> onError)
        {
            _socialInternal.GetPlayer(socialId, response, onFailed, onError);
        }
    }
}