using System;
using System.Collections.Generic;
using System.Linq;
using ElephantSDK;
using ElephantSocial.Model;
using ElephantSocial.Tournament.Model;
using ElephantSocial.Tournament.Network;

namespace ElephantSocial.Tournament
{
    public class TournamentRepository
    {
        private readonly TournamentOps _tournamentOps = new();

        public void GetTournaments(Action<TournamentsResponse> onResponse)
        {
            var getAllTournamentJob = _tournamentOps.GetTournaments(
                response =>
                {
                    if (response?.data == null)
                    {
                        var cachedData = TournamentDataStore.Instance.GetTournaments();
                        onResponse?.Invoke(new TournamentsResponse
                        {
                            tournaments = cachedData?.tournaments ?? new List<TournamentData>(),
                            serverTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        });
                        return;
                    }
                    TournamentDataStore.Instance.SetTournaments(response.data);
                    onResponse?.Invoke(response.data);
                },
                error =>
                {
                    var cachedData = TournamentDataStore.Instance.GetTournaments();
                    onResponse?.Invoke(new TournamentsResponse
                    {
                        tournaments = cachedData?.tournaments ?? new List<TournamentData>(),
                        serverTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                });

            ElephantCore.Instance.StartCoroutine(getAllTournamentJob);
        }

        public void GetMyTournaments(Action<MyTournamentsResponse> onResponse)
        {
            var myTournamentsJob = _tournamentOps.GetMyTournaments(
                response =>
                {
                    if (response?.data == null)
                    {
                        var cachedIds = TournamentDataStore.Instance.GetMyTournamentsResponse();
                        onResponse?.Invoke(new MyTournamentsResponse
                        {
                            myTournamentIds = cachedIds?.myTournamentIds ?? new List<TournamentAndScheduleId>()
                        });
                        return;
                    }
                    TournamentDataStore.Instance.SetMyTournamentsResponse(response.data);
                    onResponse?.Invoke(response.data);
                },
                _ =>
                {
                    var cachedIds = TournamentDataStore.Instance.GetMyTournamentsResponse();
                    onResponse?.Invoke(new MyTournamentsResponse
                    {
                        myTournamentIds = cachedIds?.myTournamentIds ?? new List<TournamentAndScheduleId>()
                    });
                });

            ElephantCore.Instance.StartCoroutine(myTournamentsJob);
        }

        public void GetBoard(int tournamentId, int scheduleId, Action<TournamentBoardResponse> onResponse)
        {
            var job = _tournamentOps.GetBoard(
                tournamentId,
                scheduleId,
                response =>
                {
                    if (response?.data == null)
                    {
                        var cachedBoard = TournamentDataStore.Instance.GetTournamentBoardResponse(tournamentId, scheduleId);
                        onResponse?.Invoke(new TournamentBoardResponse
                        {
                            boardPlayers = cachedBoard?.boardPlayers ?? new List<BoardPlayer>()
                        });
                        return;
                    }
                    TournamentDataStore.Instance.SetTournamentBoardResponse(tournamentId, scheduleId, response.data);
                    onResponse?.Invoke(response.data);
                },
                _ =>
                {
                    var cachedBoard = TournamentDataStore.Instance.GetTournamentBoardResponse(tournamentId, scheduleId);
                    onResponse?.Invoke(new TournamentBoardResponse
                    {
                        boardPlayers = cachedBoard?.boardPlayers ?? new List<BoardPlayer>()
                    });
                }
            );

            ElephantCore.Instance.StartCoroutine(job);
        }

        public void GetMyTournamentResults(Action<MyTournamentResultsResponse> onResponse)
        {
            var job = _tournamentOps.GetMyTournamentResults(
                response =>
                {
                    if (response?.data == null)
                    {
                        var cachedBoard = TournamentDataStore.Instance.GetMyTournamentResults();
                        onResponse?.Invoke(new MyTournamentResultsResponse()
                        {
                            myTournamentResults = cachedBoard?.myTournamentResults ?? new List<TournamentData>()
                        });
                        return;
                    }
                    TournamentDataStore.Instance.SetMyTournamentResults(response.data);
                    onResponse?.Invoke(response.data);
                },
                _ =>
                {
                    var cachedBoard = TournamentDataStore.Instance.GetMyTournamentResults();
                    onResponse?.Invoke(new MyTournamentResultsResponse()
                    {
                        myTournamentResults = cachedBoard?.myTournamentResults ?? new List<TournamentData>()
                    });
                });

            ElephantCore.Instance.StartCoroutine(job);
        }

        public void JoinTournament(int tournamentId, int scheduleId, int segmentId,
            Action<TournamentJoinResponse> onResponse, Action<string> onError)
        {
            var job = _tournamentOps.JoinTournament(
                tournamentId,
                scheduleId,
                segmentId,
                response => { onResponse?.Invoke(response.data); },
                onError
            );

            ElephantCore.Instance.StartCoroutine(job);
        }

        public void ClaimTournament(int tournamentId, int scheduleId,
            Action onResponse, Action<string> onError)
        {
            var job = _tournamentOps.ClaimTournament(
                tournamentId,
                scheduleId,
                _ => { onResponse?.Invoke(); },
                onError
            );

            ElephantCore.Instance.StartCoroutine(job);
        }

        #region Scores and Matches

        public void AddScore(int tournamentId, int scheduleId, int score,
            Action<int> onSuccess, Action<string> onError)
        {
            var timeout = RemoteConfig.GetInstance().GetInt("tournament_add_score_timeout", 5);
            
            var job = _tournamentOps.AddScore(
                score,
                tournamentId,
                scheduleId,
                timeout: timeout,
                response =>
                {
                    if (response?.data == null)
                    {
                        TournamentDataStore.Instance.SaveOfflineScore(tournamentId, scheduleId, score);
                        onSuccess?.Invoke(score);
                        return;
                    }
                    onSuccess?.Invoke(response.data.addedScore);
                },
                error =>
                {
                    TournamentDataStore.Instance.SaveOfflineScore(tournamentId, scheduleId, score);
                    onSuccess?.Invoke(score);
                },
                failedRequest => 
                {
                    if (failedRequest?.responseCode is 4002 or 4003 or 4004 or 4005)
                    {
                        TournamentDataStore.Instance.DeleteOfflineScores(tournamentId, scheduleId);
                        onError?.Invoke("Tournament has ended");
                    }
                    else
                    {
                        TournamentDataStore.Instance.SaveOfflineScore(tournamentId, scheduleId, score);
                        onSuccess?.Invoke(score);
                    }
                });

            ElephantCore.Instance.StartCoroutine(job);
        }

        public void AddMatch(int tournamentId, int scheduleId, List<ScoreUpdate> updates,
            Action onSuccess, Action<string> onError)
        {
            var job = _tournamentOps.AddMatch(
                tournamentId,
                scheduleId,
                updates,
                _ => { onSuccess?.Invoke(); },
                onError
            );
            ElephantCore.Instance.StartCoroutine(job);
        }
        
        public void ListMatches(int tournamentId, int scheduleId, Action<List<TournamentMatchItem>> onResponse, Action<string> onError)
        {
            ElephantCore.Instance.StartCoroutine(
                _tournamentOps.ListMatches(
                    tournamentId,
                    scheduleId,
                    response =>
                    {
                        if (response?.data != null)
                        {
                            onResponse?.Invoke(response.data);
                        }
                        else
                        {
                            onResponse?.Invoke(new List<TournamentMatchItem>());
                        }
                    },
                    onError
                )
            );
        }

        public void SyncOfflineScores(int tournamentId, int scheduleId)
        {
            var scores = TournamentDataStore.Instance.GetOfflineScores(tournamentId, scheduleId);
            if (!scores.Any()) return;

            var playerScores = scores.Select(s => new PlayerScore
            {
                Score = s.Score,
                Date = s.Timestamp,
                Online = false
            }).ToList();
            
            var timeout = RemoteConfig.GetInstance().GetInt("tournament_add_scores_timeout", 5);

            var job = _tournamentOps.AddScores(
                tournamentId,
                scheduleId,
                playerScores,
                timeout: timeout,
                _ =>
                {
                    TournamentDataStore.Instance.DeleteOfflineScores(tournamentId, scheduleId);
                },
                error =>
                {
                    ElephantLog.LogError("TOURNAMENT", 
                        $"Failed to sync scores for Tournament {tournamentId}: {error}");
                },
                failedRequest =>
                {
                    if (failedRequest?.responseCode is 4002 or 4003 or 4004 or 4005)
                    {
                        TournamentDataStore.Instance.DeleteOfflineScores(tournamentId, scheduleId);
                    }
                });

            ElephantCore.Instance.StartCoroutine(job);
        }

        #endregion
    }
}