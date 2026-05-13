using System;
using System.Collections.Generic;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Team.Model.Enum;

namespace ElephantSocial.Team
{
    public static class TeamManager
    {
        public static event Action OnTeamJoined;
        public static event Action OnTeamLeft;
        public static event Action OnTeamCreated;
        
        static TeamManager()
        {
            TeamService.OnTeamJoined += () => OnTeamJoined?.Invoke();
            TeamService.OnTeamLeft += () => OnTeamLeft?.Invoke();
            TeamService.OnTeamCreated += () => OnTeamCreated?.Invoke();
        }
        
        public static async UniTask<List<Team>> SuggestedTeams()
        {
            try
            {
                var serverTeams = await TeamService.GetTeamsAsync();
                var teams = new List<Team>();
                
                foreach (var serverTeam in serverTeams)
                {
                    teams.Add(TeamService.FromTeamResponse(serverTeam));
                }
                
                return teams;
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error getting suggested teams: {ex.Message}");
                return new List<Team>();
            }
        }

        public static async UniTask<List<Team>> Search(string term)
        {
            try
            {
                var serverTeams = await TeamService.GetTeamsAsync(term);
                var teams = new List<Team>();
                
                foreach (var serverTeam in serverTeams)
                {
                    teams.Add(TeamService.FromTeamResponse(serverTeam));
                }
                
                return teams;
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error getting suggested teams: {ex.Message}");
                return new List<Team>();
            }
        }

        public static async UniTask<Team> CreateTeam(string name, int badge, string description, TeamType type, int requiredLevel)
        {
            try
            {
                int capacity = 50; // Default capacity
                
                var response = await TeamService.CreateTeamAsync(
                    name,
                    capacity, 
                    requiredLevel,
                    type,
                    badge,
                    description);
                
                return TeamService.FromTeamResponse(response);
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error creating team: {ex.Message}");
                throw;
            }
        }
        
        public static async UniTask<Team> GetTeam(string teamId)
        {
            try
            {
                var teamResponse = await TeamService.GetTeamAsync(teamId);
                return TeamService.FromTeamResponse(teamResponse);
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error getting team: {ex.Message}");
                return new Team();
            }
        }
        
        public static async UniTask<string> GetMyTeamId()
        {
            try
            {
                return await TeamService.GetMyTeamIdAsync();
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error getting my team ID: {ex.Message}");
                return "";
            }
        }
        
        public static async UniTask<List<string>> GetMyJoinRequests()
        {
            try
            {
                return await TeamService.GetPlayerJoinRequestsAsync();
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error getting my join requests : {ex.Message}");
                return new List<string>();
            }
        }
        
        public static async UniTask<bool> IsUserOnline()
        {
            try
            {
                return await TeamService.IsServerHealthyAsync();
            }
            catch (Exception ex)
            {
                ElephantSDK.ElephantLog.LogError("TeamManager", $"Error checking online status: {ex.Message}");
                return false;
            }
        }
    }
}