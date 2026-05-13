using System;
using System.Collections.Generic;
using ElephantSocial.Tournament.Model;

namespace ElephantSocial.Tournament
{
    public static class TournamentManager
    {
        private static readonly TournamentManagerService Service = new();
        
        public static event Action OnTournamentsUpdated
        {
            add => Service.OnTournamentsUpdated += value;
            remove => Service.OnTournamentsUpdated -= value;
        }

        public static void Init(Action onInitialized)
        {
            Service.Init(onInitialized);
        }

        public static List<Tournament> GetTournaments()
        {
            return Service.GetTournaments();
        }

        public static Tournament GetTournamentById(int tournamentId)
        {
            return Service.GetTournamentById(tournamentId);
        }
        
        public static TournamentResult GetTournamentResultById(int tournamentId)
        {
            return Service.GetTournamentResultById(tournamentId);
        }

        public static long GetServerTime()
        {
            return Service.GetServerTime();
        }
        
        public static void IsOnline(Action<bool> onResult)
        {
            Service.IsOnline(onResult);
        }
    }
}