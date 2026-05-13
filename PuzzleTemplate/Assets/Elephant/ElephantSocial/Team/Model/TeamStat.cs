using System;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model
{
    [Serializable]
    public class TeamStat
    {
        [JsonProperty("previous_stats")] 
        public Objective[] PreviousObjectives;
    
        [JsonProperty("current_stats")] 
        public Objective[] CurrentObjectives;
    }

    [Serializable]
    public class Objective
    {
        [JsonProperty("id")] 
        public string Id;
        [JsonProperty("name")] 
        public string Name;
        [JsonProperty("description")] 
        public string Description;
        [JsonProperty("value")] 
        public long Value;
        [JsonProperty("goal")] 
        public long Goal;
        [JsonProperty("end_date")] 
        public long EndDate;
    }
}