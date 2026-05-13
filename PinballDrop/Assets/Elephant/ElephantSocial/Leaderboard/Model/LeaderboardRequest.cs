using System;
using ElephantSocial.Model;

namespace ElephantSocial.Leaderboard.Model
{
    [Serializable]
    public class LeaderboardRequest : BaseLeaderboardRequest
    {
        public LeaderboardRequest(int leaderboardId) : base(leaderboardId)
        {
        }
    }

    /// <summary>
    /// Represents the type of leaderboard to be displayed.
    /// </summary>
    public enum LeaderboardType
    {
        /// <summary>
        /// Displays the global leaderboard, worldwide, and level-based.
        /// </summary>
        Global,

        /// <summary>
        /// Displays the regional breakdown of the global leaderboard.
        /// </summary>
        Local,

        /// <summary>
        /// Displays a leaderboard of players who have finished the game and compete in a separate endgame mode.
        /// </summary>
        //EndGame,
    }
}