using System;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentListMatchesRequest : BaseTournamentRequest
    {
        public TournamentListMatchesRequest(int tournamentId, int scheduleID) 
            : base(tournamentId, scheduleID)
        {
            
        }
    }
}
