using System;
using System.Collections.Generic;
using ElephantSDK;
using ElephantSocial.Model;
using ElephantSocial.Tournament.Model;

namespace ElephantSocial.Tournament
{
    public class Tournament
    {
        #region Events

        internal static event Action OnTournamentJoined;

        #endregion

        #region Properties

        public TournamentData TournamentData;
        private readonly TournamentRepository _tournamentRepository;

        public int TournamentId => TournamentData.tournamentID;
        public bool HasJoined;
        public long StartTime => TournamentData.startDateUnix;
        public long EndTime => TournamentData.endDateUnix;

        public Tournament(TournamentData tournamentData)
        {
            TournamentData = tournamentData;
            _tournamentRepository = new TournamentRepository();
        }

        #endregion

        #region Public Methods

        public bool IsRunning()
        {
            return TournamentData.tournamentState == TournamentState.Running;
        }

        public void Join(Action<List<BoardPlayer>> onResponse, Action<string> onError, int segmentId = 0)
        {
            if (TournamentData.tournamentState != TournamentState.Running)
            {
                onError?.Invoke("Tournament is not in a running state.");
                return;
            }

            _tournamentRepository.JoinTournament(
                TournamentId,
                TournamentData.scheduleID,
                segmentId,
                tournamentJoinResponse =>
                {
                    HasJoined = true;
                    OnTournamentJoined?.Invoke();
                    onResponse?.Invoke(tournamentJoinResponse.boardPlayers);
                },
                message => { onError?.Invoke(message); }
            );
        }

        public void AddScore(int score, Action onResponse, Action<string> onError)
        {
            if (!HasJoined || !IsRunning())
            {
                onError?.Invoke("Cannot add score to this tournament.");
                TournamentDataStore.Instance.DeleteOfflineScores(TournamentId, TournamentData.scheduleID);
                return;
            }
        
            _tournamentRepository.AddScore(
                TournamentId,
                TournamentData.scheduleID,
                score,
                serverScore =>
                {
                    GetBoard(
                        boardPlayers =>
                        {
                            var socialId = Social.Instance.GetPlayer().socialId;
                            foreach (var boardPlayer in boardPlayers)
                            {
                                if (socialId == boardPlayer.socialId)
                                {
                                    boardPlayer.score += serverScore;
                                    var tournamentBoardResponse = new TournamentBoardResponse
                                    {
                                        boardPlayers = boardPlayers
                                    };
                                    TournamentDataStore.Instance.SetTournamentBoardResponse(
                                        TournamentId, TournamentData.scheduleID, tournamentBoardResponse);
                                }
                            }

                            onResponse?.Invoke();
                        }
                    );
                },
                error => { onError?.Invoke(error); }
            );
        }

        public void AddMatch(List<ScoreUpdate> scoreUpdates, Action onResponse, Action<string> onError)
        {
            if (!HasJoined || !IsRunning())
            {
                onError?.Invoke("Cannot add match to this tournament.");
                return;
            }

            _tournamentRepository.AddMatch(
                TournamentId,
                TournamentData.scheduleID,
                scoreUpdates,
                onResponse,
                error => { onError?.Invoke(error); }
            );
        }
        
        public void ListMatches(Action<List<TournamentMatchItem>> onResponse, Action<string> onError)
        {
            _tournamentRepository.ListMatches(
                TournamentId,
                TournamentData.scheduleID,
                onResponse, onError);
        }

        public void GetBoard(Action<List<BoardPlayer>> onResponse)
        {
            _tournamentRepository.GetBoard(
                TournamentId,
                TournamentData.scheduleID,
                boardPlayers => { onResponse?.Invoke(boardPlayers.boardPlayers); }
            );
        }

        public long GetRemainingSeconds()
        {
            var serverTime = TournamentManager.GetServerTime();

            return TournamentData.tournamentState switch
            {
                TournamentState.Pending => StartTime - serverTime < 0 ? 0 : StartTime - serverTime,
                TournamentState.Running => EndTime - serverTime < 0 ? 0 : EndTime - serverTime,
                _ => 0
            };
        }

        #endregion
    }
}