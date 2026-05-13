using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using ElephantSocial.Tournament.Model;
using ElephantSocial.Model;
using UnityEngine.Networking;

namespace ElephantSocial.Tournament.Network
{
    public class TournamentOps : GenericResponseOps
    {
        private IEnumerator MakeRequest<T>(string url, object data,
            Action<GenericResponse<T>> onResponse, Action<string> onError, int timeout = 30) where T : new()
        {
            timeout = RemoteConfig.GetInstance().GetInt("tournament_base_timeout", 30);

            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<T>();
            return networkManager.PostWithResponseSocial(
                url, 
                bodyJson,
                response => HandleResponse(response,
                    r => onResponse?.Invoke(response),
                    onError),
                onError,
                timeout,
                false,
                request => HandleErrorResponse(request, (code, message) => onError?.Invoke($"Error {code}: {message}"))
            );
        }

        public IEnumerator AddScore(int score, int tournamentId, int scheduleId,
            int timeout,
            Action<GenericResponse<TournamentAddScoreResponse>> onResponse,
            Action<string> onError,
            Action<UnityWebRequest> onFailedResponse
            )
        {
            var data = new TournamentAddScoreRequest(tournamentId, scheduleId, score);
            var bodyJson = PrepareBodyJson(data);
            var url = IsProductionEnvironment()
                ? SocialConst.TournamentAddScoreEp
                : SocialConstDev.TournamentAddScoreEp;
            var networkManager = new GenericNetworkManager<TournamentAddScoreResponse>();
            return networkManager.PostWithResponseSocial(url, bodyJson, 
                onResponse, onError, timeout, false, onFailedResponse);
        }

        public IEnumerator AddScores(int tournamentId, int scheduleId, List<PlayerScore> scores,
            int timeout,
            Action<GenericResponse<TournamentBulkAddScoresResponse>> onResponse,
            Action<string> onError,
            Action<UnityWebRequest> onFailedResponse
            )
        {
            var data = new TournamentAddScoresRequest(tournamentId, scheduleId, scores);
            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<TournamentBulkAddScoresResponse>();
            var url = IsProductionEnvironment()
                ? SocialConst.TournamentAddScoresEp
                : SocialConstDev.TournamentAddScoresEp;
            return networkManager.PostWithResponseSocial(url, bodyJson, 
                onResponse, onError, timeout, false, onFailedResponse);
        }

        public IEnumerator GetBoard(int tournamentId, int scheduleId,
            Action<GenericResponse<TournamentBoardResponse>> onResponse, Action<string> onError)
        {
            var data = new TournamentRequest(tournamentId, scheduleId);
            var url = IsProductionEnvironment()
                ? SocialConst.TournamentGetBoardEp
                : SocialConstDev.TournamentGetBoardEp;
            return MakeRequest(url, data, onResponse, onError);
        }

        public IEnumerator AddMatch(int tournamentId, int scheduleId, List<ScoreUpdate> scoreUpdates,
            Action<GenericResponse<object>> onResponse, Action<string> onError)
        {
            var data = new TournamentAddMatchRequest(tournamentId, scheduleId, scoreUpdates);
            var url = IsProductionEnvironment() ? SocialConst.TournamentAddMatchEp : SocialConstDev.TournamentAddMatchEp;
            return MakeRequest(url, data, onResponse, onError);
        }

        public IEnumerator ListMatches(int tournamentId, int scheduleId, Action<GenericResponse<List<TournamentMatchItem>>> onResponse, Action<string> onError)
        {
            var data = new TournamentListMatchesRequest(tournamentId, scheduleId);
            var url = IsProductionEnvironment() ? SocialConst.TournamentListMatchesEp : SocialConstDev.TournamentListMatchesEp;
            return MakeRequest(url, data, onResponse, onError);
        }

        public IEnumerator JoinTournament(int tournamentId, int scheduleId, int segmentId,
            Action<GenericResponse<TournamentJoinResponse>> onResponse, Action<string> onError)
        {
            var data = new TournamentJoinRequest(tournamentId, scheduleId, segmentId);
            var url = IsProductionEnvironment() ? SocialConst.TournamentJoinEp : SocialConstDev.TournamentJoinEp;
            return MakeRequest(url, data, onResponse, onError);
        }

        public IEnumerator ClaimTournament(int tournamentId, int scheduleId,
            Action<GenericResponse<TournamentFinishResponse>> onResponse, Action<string> onError)
        {
            var data = new TournamentFinishRequest(tournamentId, scheduleId);
            var url = IsProductionEnvironment() ? SocialConst.TournamentClaimEp : SocialConstDev.TournamentClaimEp;
            return MakeRequest(url, data, onResponse, onError);
        }

        #region TournamentManager

        public IEnumerator GetTournaments(Action<GenericResponse<TournamentsResponse>> onResponse, Action<string> onError)
        {
            var data = new GeneralTournamentRequest();            
            var url = IsProductionEnvironment() ? SocialConst.TournamentsAll : SocialConstDev.TournamentsAll;
            return MakeRequest(url, data, onResponse, onError);
        }
        
        public IEnumerator GetMyTournaments(Action<GenericResponse<MyTournamentsResponse>> onResponse, Action<string> onError)
        {
            var data = new GeneralTournamentRequest();            
            var url = IsProductionEnvironment() ? SocialConst.TournamentsMine : SocialConstDev.TournamentsMine;
            return MakeRequest(url, data, onResponse, onError);
        }
        
        public IEnumerator GetMyTournamentResults(Action<GenericResponse<MyTournamentResultsResponse>> onResponse, Action<string> onError)
        {
            var data = new GeneralTournamentRequest();            
            var url = IsProductionEnvironment() ? SocialConst.TournamentsResult : SocialConstDev.TournamentsResult;
            return MakeRequest(url, data, onResponse, onError);
        }

        #endregion
        
        public void TournamentHealthCheck(Action onSuccess, Action onError)
        {
            ElephantCore.Instance.StartCoroutine(CheckServerHealth(onSuccess, onError));
        }
        
        private IEnumerator CheckServerHealth(Action onSuccess, Action onError)
        {
            var url = IsProductionEnvironment() ? SocialConst.HealthCheckEp : SocialConstDev.HealthCheckEp;
            var healthCheckTimeout = RemoteConfig.GetInstance().GetInt("tournament_health_check_timeout", 5);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = healthCheckTimeout;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                onError?.Invoke();
                ElephantLog.LogError("TOURNAMENT",$"Server health check failed: {request.error}");
            }
            else
            {
                onSuccess?.Invoke();
                ElephantLog.Log("TOURNAMENT","Server is healthy: " + request.downloadHandler.text);
            }
        }
    }
}