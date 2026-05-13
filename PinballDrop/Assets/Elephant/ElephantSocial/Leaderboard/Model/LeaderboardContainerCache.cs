using System;
using ElephantSocial.CachingSystem;
using UnityEngine;

namespace ElephantSocial.Leaderboard
{
    public class LeaderboardContainerCache : GenericCachingSystem<LeaderboardContainer>
    { 
        private LeaderboardContainerCache(Action<Action<LeaderboardContainer>, Action<string>> dataRequestAction, int cachingIntervalSeconds) 
            : base(dataRequestAction, cachingIntervalSeconds)
        {
            
        }
        
        public static LeaderboardContainerCache CreateInstance(Action<Action<LeaderboardContainer>, Action<string>> dataRequestAction, int cachingIntervalSeconds)
        {
            return new LeaderboardContainerCache(dataRequestAction, cachingIntervalSeconds);
        }
    }
}