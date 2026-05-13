using System;
using System.Collections.Generic;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentRequest : BaseTournamentRequest
    {
        public TournamentRequest(int tournamentId, int scheduleID) 
            : base(tournamentId, scheduleID)
        {
            
        }
    }
}