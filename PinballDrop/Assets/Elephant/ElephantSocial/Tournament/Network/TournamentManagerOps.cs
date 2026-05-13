using System;
using System.Collections;
using ElephantSDK;
using ElephantSocial.Tournament.Model;

namespace ElephantSocial.Tournament.Network
{
    public class TournamentManagerOps : SocialOps
    {
        public IEnumerator GetTournamentInitialConfig(Action<GenericResponse<TournamentInitResponse>> onResponse,
            Action<string> onError)
        {
            var data = new GeneralTournamentRequest();

            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<TournamentInitResponse>();
            var postWithResponse =
                networkManager.PostWithResponseSocial(
                    IsProductionEnvironment() ? SocialConst.TournamentInit : SocialConstDev.TournamentInit, bodyJson,
                    onResponse, onError);

            return postWithResponse;
        }
    }
}