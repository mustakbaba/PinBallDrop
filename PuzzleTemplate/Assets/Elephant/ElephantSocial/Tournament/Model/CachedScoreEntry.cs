using System;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class CachedScoreEntry
    {
        public int Score { get; set; }
        public int TournamentId { get; set; }
        public int ScheduleId { get; set; }
        public long Date { get; set; }
        public bool Online { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CachedScoreEntry other)
            {
                return Score == other.Score &&
                       TournamentId == other.TournamentId &&
                       ScheduleId == other.ScheduleId &&
                       Date == other.Date &&
                       Online == other.Online;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Score, TournamentId, ScheduleId, Date, Online);
        }
    }
}