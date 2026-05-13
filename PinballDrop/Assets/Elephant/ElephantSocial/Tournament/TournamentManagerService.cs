using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ElephantSDK;
using ElephantSocial.Tournament.Network;
using ElephantSocial.Tournament.Model;
using UnityEngine;

namespace ElephantSocial.Tournament
{
    internal class TournamentManagerService
    {
        #region Events and Public Methods

        public event Action OnTournamentsUpdated;

        public TournamentManagerService()
        {
            _tournamentManagerState = new TournamentManagerState();
            _config = new UpdateConfig();

            TournamentResult.OnTournamentClaimed += HandleTournamentClaimed;
            Tournament.OnTournamentJoined += HandleTournamentJoined;
        }

        public void Init(Action onComplete)
        {
            FetchTournamentData(
                isInitialFetch: true,
                () =>
                {
                    _tournamentManagerState.IsInitialized = true;
                    onComplete?.Invoke();
                    ElephantCore.Instance.StartCoroutine(UpdateTournamentsRoutine());
                }
            );
        }

        public List<Tournament> GetTournaments()
        {
            if (!EnsureInitialized())
                return null;

            return _tournamentManagerState.Tournaments;
        }

        public Tournament GetTournamentById(int tournamentId)
        {
            if (!EnsureInitialized())
                return null;

            return _tournamentManagerState.Tournaments.Find(t => t.TournamentId == tournamentId);
        }

        public TournamentResult GetTournamentResultById(int tournamentId)
        {
            if (!EnsureInitialized())
                return null;

            return _tournamentManagerState.TournamentResults
                .Where(t => t.TournamentId == tournamentId)
                .OrderByDescending(t => t.TournamentData.scheduleID)
                .FirstOrDefault();
        }

        public long GetServerTime()
        {
            var elapsedTime = Time.realtimeSinceStartup - _lastServerTimeUpdateTime;
            return _tournamentManagerState.ServerTime + (long)elapsedTime;
        }

        #endregion

        #region Private Fields and Classes

        private readonly TournamentRepository _tournamentRepository = new();
        private TournamentManagerState _tournamentManagerState;
        private readonly UpdateConfig _config;
        private float _lastServerTimeUpdateTime;
        private readonly TournamentOps _tournamentOps = new();

        private class TournamentManagerState
        {
            public List<Tournament> Tournaments = new();
            public List<TournamentResult> TournamentResults = new();
            public long ServerTime;
            public bool IsInitialized;
        }

        private class UpdateConfig
        {
            public float CheckInterval = RemoteConfig.GetInstance().GetFloat("tournament_update_interval", 30f);

            public float EndingSoonInterval =
                RemoteConfig.GetInstance().GetFloat("tournament_ending_soon_update_interval", 5f);
        }

        #endregion

        #region Data Fetching Methods

        public void IsOnline(Action<bool> onResult)
        {
            _tournamentOps.TournamentHealthCheck(
                () => onResult?.Invoke(true),
                () => onResult?.Invoke(false)
            );
        }

        private void FetchTournamentData(bool isInitialFetch, Action onComplete = null)
        {
            _tournamentRepository.GetTournaments(
                tournamentsResponse =>
                {
                    var tournaments = tournamentsResponse.tournaments
                        .Select(tournamentData => new Tournament(tournamentData))
                        .ToList();
                    var hasChanges = CheckTournamentStates(tournaments, tournamentsResponse.serverTime);

                    if (hasChanges)
                    {
                        _tournamentRepository.GetMyTournaments(
                            myTournamentsResponse =>
                            {
                                _tournamentRepository.GetMyTournamentResults(
                                    resultsResponse =>
                                    {
                                        UpdateTournamentState(tournaments, tournamentsResponse.serverTime);
                                        
                                        if (myTournamentsResponse.myTournamentIds.Count > 0)
                                        {
                                            ProcessJoinedTournaments(myTournamentsResponse.myTournamentIds);
                                            var joinedTournaments = _tournamentManagerState.Tournaments.Where(t => t.HasJoined)
                                                .ToList();
                                            if (joinedTournaments.Any())
                                            {
                                                FetchBoardsForJoinedTournaments(joinedTournaments, null);
                                            }
                                        }
                                        
                                        var existingResults = _tournamentManagerState.TournamentResults;
                                        var updatedResults = new List<TournamentResult>();

                                        foreach (var newResultData in resultsResponse.myTournamentResults)
                                        {
                                            var existingResult = existingResults.FirstOrDefault(r =>
                                                r.TournamentId == newResultData.tournamentID &&
                                                r.TournamentData.scheduleID == newResultData.scheduleID);

                                            if (existingResult != null)
                                            {
                                                existingResult.TournamentData = newResultData;
                                                updatedResults.Add(existingResult);
                                            }
                                            else
                                            {
                                                updatedResults.Add(new TournamentResult(newResultData));
                                            }
                                        }

                                        _tournamentManagerState.TournamentResults = updatedResults;
                                        
                                        if (!isInitialFetch)
                                        {
                                            OnTournamentsUpdated?.Invoke();
                                        }

                                        onComplete?.Invoke();
                                    }
                                );
                            }
                        );
                    }
                    else
                    {
                        _tournamentManagerState.ServerTime = tournamentsResponse.serverTime;
                        _lastServerTimeUpdateTime = Time.realtimeSinceStartup;
                        onComplete?.Invoke();
                    }

                    SyncOfflineScores();
                }
            );
        }

        #endregion

        #region Tournament Processing Methods

        private void ProcessJoinedTournaments(List<TournamentAndScheduleId> tournamentIds)
        {
            foreach (var tournament in _tournamentManagerState.Tournaments)
            {
                tournament.HasJoined = tournamentIds.Any(id =>
                    id.tournamentId == tournament.TournamentId &&
                    id.scheduleId == tournament.TournamentData.scheduleID);
            }
        }

        private void FetchBoardsForJoinedTournaments(List<Tournament> joinedTournaments,
            Action onComplete)
        {
            var processedCount = 0;
            foreach (var tournament in joinedTournaments)
            {
                tournament.GetBoard(
                    _ =>
                    {
                        processedCount++;
                        if (processedCount == joinedTournaments.Count)
                        {
                            onComplete?.Invoke();
                        }
                    }
                );
            }
        }

        private void SyncOfflineScores()
        {
            var tournaments = TournamentDataStore.Instance.GetTournaments()?.tournaments
                              ?? new List<TournamentData>();

            foreach (var (tournamentId, scheduleId) in
                     TournamentDataStore.Instance.GetAllOfflineScoreTournaments())
            {
                var tournament = tournaments.FirstOrDefault(t =>
                    t.tournamentID == tournamentId &&
                    t.scheduleID == scheduleId);

                if (tournament?.tournamentState == TournamentState.Running)
                {
                    _tournamentRepository.SyncOfflineScores(tournamentId, scheduleId);
                }
                else
                {
                    TournamentDataStore.Instance.DeleteOfflineScores(tournamentId, scheduleId);
                }
            }
        }

        #endregion

        #region Update and State Management

        private IEnumerator UpdateTournamentsRoutine()
        {
            while (_tournamentManagerState.IsInitialized)
            {
                FetchTournamentData(isInitialFetch: false);
                yield return new WaitForSecondsRealtime(GetUpdateInterval());
            }
        }

        private bool CheckTournamentStates(List<Tournament> newTournaments, long time)
        {
            var existingTournaments = _tournamentManagerState.Tournaments;
            var hasChanges = false;

            foreach (var existingTournament in existingTournaments.Where(existingTournament =>
                         newTournaments.All(t => t.TournamentId != existingTournament.TournamentId)))
            {
                hasChanges = true;
            }

            foreach (var newTournament in newTournaments)
            {
                var existingTournament = existingTournaments.FirstOrDefault(t =>
                    t.TournamentId == newTournament.TournamentId);

                if (existingTournament != null)
                {
                    if (existingTournament.TournamentData.scheduleID != newTournament.TournamentData.scheduleID)
                    {
                        hasChanges = true;
                    }
                    else if (!AreTournamentDatasEqual(existingTournament.TournamentData, newTournament.TournamentData))
                    {
                        hasChanges = true;
                    }
                }
                else
                {
                    hasChanges = true;
                }
            }
            
            return hasChanges;
        }
        
        private void UpdateTournamentState(List<Tournament> newTournaments, long time)
        {
            var existingTournaments = _tournamentManagerState.Tournaments;
            var updatedTournaments = new List<Tournament>();

            foreach (var existingTournament in existingTournaments.Where(existingTournament =>
                         newTournaments.All(t => t.TournamentId != existingTournament.TournamentId)))
            {
                existingTournament.TournamentData.tournamentState = TournamentState.Completed;
            }

            foreach (var newTournament in newTournaments)
            {
                var existingTournament = existingTournaments.FirstOrDefault(t =>
                    t.TournamentId == newTournament.TournamentId);

                if (existingTournament != null)
                {
                    if (existingTournament.TournamentData.scheduleID != newTournament.TournamentData.scheduleID)
                    {
                        existingTournament.HasJoined = false;
                        existingTournament.TournamentData = newTournament.TournamentData;
                        updatedTournaments.Add(existingTournament);
                    }
                    else if (!AreTournamentDatasEqual(existingTournament.TournamentData, newTournament.TournamentData))
                    {
                        existingTournament.TournamentData = newTournament.TournamentData;
                        updatedTournaments.Add(existingTournament);
                    }
                    else
                    {
                        updatedTournaments.Add(existingTournament);
                    }
                }
                else
                {
                    updatedTournaments.Add(newTournament);
                }
            }
            
            _tournamentManagerState.Tournaments = updatedTournaments;
            _tournamentManagerState.ServerTime = time;
            _lastServerTimeUpdateTime = Time.realtimeSinceStartup;
        }

        private bool AreTournamentDatasEqual(TournamentData a, TournamentData b)
        {
            return a.tournamentID == b.tournamentID &&
                   a.scheduleID == b.scheduleID &&
                   a.startDateUnix == b.startDateUnix &&
                   a.endDateUnix == b.endDateUnix &&
                   a.tournamentState == b.tournamentState;
        }

        private float GetUpdateInterval()
        {
            var hasEndingSoonTournament = _tournamentManagerState.Tournaments.Any(t =>
            {
                var timeUntilEnd = t.EndTime - _tournamentManagerState.ServerTime;
                return timeUntilEnd > 0 && timeUntilEnd <= (_config.CheckInterval);
            });

            return hasEndingSoonTournament ? _config.EndingSoonInterval : _config.CheckInterval;
        }

        #endregion

        #region Utility Methods

        private void HandleTournamentClaimed(int tournamentId, int scheduleId)
        {
            _tournamentRepository.GetMyTournamentResults(
                resultsResponse =>
                {
                    var existingResults = _tournamentManagerState.TournamentResults;
                    var updatedResults = new List<TournamentResult>();

                    foreach (var newResultData in resultsResponse.myTournamentResults)
                    {
                        var existingResult = existingResults.FirstOrDefault(r =>
                            r.TournamentId == newResultData.tournamentID &&
                            r.TournamentData.scheduleID == newResultData.scheduleID);

                        if (existingResult != null)
                        {
                            existingResult.TournamentData = newResultData;
                            updatedResults.Add(existingResult);
                        }
                        else
                        {
                            updatedResults.Add(new TournamentResult(newResultData));
                        }
                    }

                    _tournamentManagerState.TournamentResults = updatedResults;
                    OnTournamentsUpdated?.Invoke();
                }
            );
        }

        private void HandleTournamentJoined()
        {
            _tournamentRepository.GetMyTournaments(
                myTournamentsResponse =>
                {
                    if (myTournamentsResponse.myTournamentIds.Count > 0)
                    {
                        ProcessJoinedTournaments(myTournamentsResponse.myTournamentIds);
                        var joinedTournament = _tournamentManagerState.Tournaments.Find(t =>
                            t.HasJoined);

                        if (joinedTournament != null)
                        {
                            FetchBoardsForJoinedTournaments(new List<Tournament> { joinedTournament }, null);
                        }
                    }

                    OnTournamentsUpdated?.Invoke();
                }
            );
        }

        private bool EnsureInitialized()
        {
            if (_tournamentManagerState.IsInitialized)
                return true;

            ElephantLog.LogError("SocialTournament", "TournamentManager is not initialized.");
            return false;
        }

        #endregion
    }
}