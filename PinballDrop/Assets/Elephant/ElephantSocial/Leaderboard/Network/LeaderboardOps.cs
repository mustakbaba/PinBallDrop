using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using ElephantSocial.Leaderboard.Model;
using ElephantSocial.Model;

namespace ElephantSocial.Leaderboard.Network
{
    public class LeaderboardOps : SocialOps
    {
        public IEnumerator UpdateScore(BoardPlayer boardPlayer, int leaderboardId, string operation, int score,
            Action<GenericResponse<BoardPlayer>> onResponse,
            Action<string> onError)
        {
            LeaderboardPlayerRequest data = new LeaderboardPlayerRequest(leaderboardId, boardPlayer, operation);
            data.score = score;
            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<BoardPlayer>();
            //TODO: {id} must be updated after
            var url = (IsProductionEnvironment() ? SocialConst.PlayerScoreEp : SocialConstDev.PlayerScoreEp).Replace(
                "{id}", leaderboardId.ToString());
            var postWithResponse =
                networkManager.PostWithResponseSocial(url, bodyJson, onResponse, onError);

            return postWithResponse;
        }

        public IEnumerator GetScore(int leaderboardId,
            Action<GenericResponse<BoardPlayer>> onResponse,
            Action<string> onError)
        {
            var data = new LeaderboardRequest(leaderboardId);
            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<BoardPlayer>();
            var postWithResponse =
                networkManager.PostWithResponseSocial(
                    IsProductionEnvironment() ? SocialConst.UserEp : SocialConstDev.UserEp, bodyJson, onResponse,
                    onError);

            return postWithResponse;
        }

        public IEnumerator GetLeaderboard(int leaderboardId,
            Action<GenericResponse<LeaderboardContainer>> onResponse,
            Action<string> onError)
        {
            var data = new LeaderboardRequest(leaderboardId);

            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<LeaderboardContainer>();
            var postWithResponse =
                networkManager.PostWithResponseSocial(
                    IsProductionEnvironment() ? SocialConst.BoardEp : SocialConstDev.BoardEp, bodyJson,
                    onResponse, onError);

            return postWithResponse;
        }
    }
}