using System;
using System.Collections.Generic;
using ElephantSDK;
using ElephantSocial.Model;

namespace ElephantSocial.Tournament
{
    public class TournamentResult
    {
        public TournamentData TournamentData;
        private readonly TournamentRepository _tournamentRepository;

        public int TournamentId => TournamentData.tournamentID;

        internal static event Action<int, int> OnTournamentClaimed;

        public TournamentResult(TournamentData tournamentData)
        {
            TournamentData = tournamentData;
            _tournamentRepository = new TournamentRepository();
        }

        public void GetBoard(Action<List<BoardPlayer>> onResponse)
        {
            _tournamentRepository.GetBoard(
                TournamentId,
                TournamentData.scheduleID,
                boardPlayers => { onResponse?.Invoke(boardPlayers.boardPlayers); }
            );
        }

        public void Claim(Action onResponse, Action<string> onError)
        {
            if (TournamentData.tournamentState != TournamentState.Completed)
            {
                onError?.Invoke("Tournament is not completed.");
                return;
            }

            _tournamentRepository.ClaimTournament(
                TournamentId,
                TournamentData.scheduleID,
                () =>
                {
                    onResponse?.Invoke();
                    OnTournamentClaimed?.Invoke(TournamentId, TournamentData.scheduleID);
                },
                message => { onError?.Invoke(message); }
            );
        }
    }
}