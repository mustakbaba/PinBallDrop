using System;
using System.Collections.Generic;
using ElephantUniTask.Threading.Tasks;
using ElephantSDK;
using ElephantSocial.Core;
using ElephantSocial.Model;
using ElephantSocial.Team.Model.Enum;
using ElephantSocial.Team.Model.Response;

namespace ElephantSocial.Team
{
    public static class TeamService
    {
        public static event Action OnTeamJoined;
        public static event Action OnTeamLeft;
        public static event Action OnTeamCreated;
        
        public static async UniTask<Player> GetPlayerAsync()
        {
            try
            {
                var response = await TeamApi.Instance.GetPlayerAsync();
                return response;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error getting my team: {ex.Message}");
                throw new TeamOperationException("Failed to retrieve team information", ex);
            }
        }
        
        public static async UniTask<string> GetMyTeamIdAsync()
        {
            try
            {
                var response = await TeamApi.Instance.GetPlayerAsync();
                return response?.team?.TeamId ?? string.Empty;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error getting my team ID: {ex.Message}");
                return string.Empty;
            }
        }
        
        public static async UniTask<List<TeamResponse>> GetTeamsAsync(string searchTerm = "")
        {
            try
            {
                var response = await TeamApi.Instance.ListTeamsAsync(searchTerm);
                return response ?? new List<TeamResponse>();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error getting teams: {ex.Message}");
                throw new TeamOperationException("Failed to retrieve teams list", ex);
            }
        }
        
        public static async UniTask<TeamResponse> GetTeamAsync(string teamId)
        {
            try
            {
                return await TeamApi.Instance.GetTeamAsync(teamId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error getting team {teamId}: {ex.Message}");
                throw new TeamOperationException($"Failed to retrieve team {teamId}", ex);
            }
        }
        
        public static async UniTask<TeamResponse> CreateTeamAsync(
            string name, 
            int capacity, 
            int requiredLevel, 
            TeamType teamType, 
            int badge, 
            string description)
        {
            try
            {
                var team = await TeamApi.Instance.CreateTeamAsync(
                    name, 
                    description, 
                    capacity, 
                    requiredLevel, 
                    (int)teamType, 
                    badge);
                
                OnTeamCreated?.Invoke();
                OnTeamJoined?.Invoke();
                return team;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error creating team: {ex.Message}");
                throw new TeamOperationException("Failed to create team", ex);
            }
        }
        
        public static async UniTask<TeamResponse> JoinTeamAsync(string teamId)
        {
            try
            {
                var team = await TeamApi.Instance.JoinTeamAsync(teamId);
                OnTeamJoined?.Invoke();
                return team;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error joining team {teamId}: {ex.Message}");
                throw new TeamOperationException($"Failed to join team {teamId}", ex);
            }
        }
        
        public static async UniTask<TeamResponse> LeaveTeamAsync()
        {
            try
            {
                var team = await TeamApi.Instance.LeaveTeamAsync();
                TriggerTeamLeft();
                return team;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error leaving team: {ex.Message}");
                throw new TeamOperationException("Failed to leave team", ex);
            }
        }

        public static void TriggerTeamLeft()
        {
            OnTeamLeft?.Invoke();
        }
        
        public static async UniTask<TeamResponse> UpdateTeamAsync(
            string name, 
            int capacity, 
            int requiredLevel, 
            int teamType, 
            string description, 
            int badge)
        {
            try
            {
                return await TeamApi.Instance.UpdateTeamAsync(
                    name,
                    capacity,
                    requiredLevel,
                    teamType,
                    description,
                    badge);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error updating team: {ex.Message}");
                throw new TeamOperationException("Failed to update team", ex);
            }
        }
        
        public static async UniTask<TeamResponse> PromoteMemberAsync(string targetSocialId)
        {
            try
            {
                return await TeamApi.Instance.PromoteMemberAsync(targetSocialId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error promoting member: {ex.Message}");
                throw new TeamOperationException("Failed to promote team member", ex);
            }
        }
        
        public static async UniTask<TeamResponse> DemoteMemberAsync(string targetSocialId)
        {
            try
            {
                return await TeamApi.Instance.DemoteMemberAsync(targetSocialId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error demoting member: {ex.Message}");
                throw new TeamOperationException("Failed to demote team member", ex);
            }
        }
        
        public static async UniTask<TeamResponse> KickMemberAsync(string targetSocialId)
        {
            try
            {
                return await TeamApi.Instance.KickMemberAsync(targetSocialId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error kicking member: {ex.Message}");
                throw new TeamOperationException("Failed to kick team member", ex);
            }
        }
        
        public static async UniTask<List<TeamMember>> GetJoinRequestsAsync()
        {
            try
            {
                var joinRequestResponse = await TeamApi.Instance.GetJoinRequestsAsync();
                var teamMembers = new List<TeamMember>();
                
                foreach (var serverMember in joinRequestResponse)
                {
                    teamMembers.Add(TeamMember.FromServerModel(serverMember));
                }
                
                return teamMembers;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error getting join requests: {ex.Message}");
                throw new TeamOperationException("Failed to get team join requests", ex);
            }
        }
        
        public static async UniTask<List<string>> GetPlayerJoinRequestsAsync()
        {
            try
            {
                var joinRequestResponse = await TeamApi.Instance.GetPlayerJoinRequestsAsync();
                var teamIdList = new List<string>();
                
                foreach (var serverMember in joinRequestResponse)
                {
                    teamIdList.Add(serverMember.TeamId);
                }
                
                return teamIdList;
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error getting player join requests: {ex.Message}");
                throw new TeamOperationException("Failed to get team join requests", ex);
            }
        }
        
        public static async UniTask AcceptJoinRequestAsync(string targetSocialId)
        {
            try
            {
                await TeamApi.Instance.AcceptJoinRequestAsync(targetSocialId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error accepting join request: {ex.Message}");
                throw new TeamOperationException("Failed to accept join request", ex);
            }
        }
        
        public static async UniTask RejectJoinRequestAsync(string targetSocialId)
        {
            try
            {
                await TeamApi.Instance.RejectJoinRequestAsync(targetSocialId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error rejecting join request: {ex.Message}");
                throw new TeamOperationException("Failed to reject join request", ex);
            }
        }
        
        public static async UniTask IncrementStatAsync(int incrementValue, string statId)
        {
            try
            {
                await TeamApi.Instance.IncrementStat(incrementValue, statId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error incrementing objective: {ex.Message}");
                throw new TeamOperationException("Failed to increment objective", ex);
            }
        }
        
        public static async UniTask<bool> IsServerHealthyAsync()
        {
            try
            {
                return await TeamApi.Instance.IsServerHealthyAsync();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamService", $"Error checking server health: {ex.Message}");
                return false;
            }
        }
        
        public static Team FromTeamResponse(TeamResponse response)
        {
            var team = new Team
            {
                id = response.TeamId,
                name = response.Name,
                stars = response.Stars,
                capacity = response.Capacity,
                size = response.Size,
                description = response.Description,
                requiredLevel = response.RequiredLevel,
                weeklyHelps = response.WeeklyHelps,
                type = (TeamType)response.TeamType,
                badgeId = response.Badge,
                members = new List<TeamMember>(),
                count = response.OnlineCount,
                country = response.Country,
                TeamStat = response.TeamStat
            };

            if (response.TeamMembers != null)
            {
                foreach (var serverMember in response.TeamMembers)
                {
                    team.members.Add(TeamMember.FromServerModel(serverMember));
                }
            }

            return team;
        }
        
        public static void UpdateFromResponse(TeamResponse response, Team elephantTeam)
        {
            elephantTeam.name = response.Name;
            elephantTeam.stars = response.Stars;
            elephantTeam.capacity = response.Capacity;
            elephantTeam.size = response.Size;
            elephantTeam.description = response.Description;
            elephantTeam.requiredLevel = response.RequiredLevel;
            elephantTeam.weeklyHelps = response.WeeklyHelps;
            elephantTeam.type = (TeamType)response.TeamType;
            elephantTeam.badgeId = response.Badge;

            elephantTeam.members.Clear();
            if (response.TeamMembers == null) return;
            
            foreach (var serverMember in response.TeamMembers)
            {
                elephantTeam.members.Add(TeamMember.FromServerModel(serverMember));
            }
        }

        public static void TriggerTeamJoined()
        {
            OnTeamJoined?.Invoke();
        }
    }
}