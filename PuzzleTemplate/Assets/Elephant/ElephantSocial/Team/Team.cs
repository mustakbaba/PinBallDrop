using System.Collections.Generic;
using ElephantUniTask.Threading.Tasks;
using ElephantSDK;
using ElephantSocial.Core;
using ElephantSocial.Team.Model;
using ElephantSocial.Team.Model.Enum;

namespace ElephantSocial.Team
{
    public class Team
    {
        public string id;
        public string name;
        public int stars;
        public int capacity = 50;
        public int size;
        public string description;
        public int teamScore;
        public int requiredLevel;
        public int weeklyHelps;
        public TeamType type;
        public int badgeId;
        public List<TeamMember> members;
        public int count;
        public string country;
        public TeamStat TeamStat;

        public List<TeamMember> GetTeamMembers()
        {
            return members.FindAll(member =>
                member.role == TeamMemberRole.MEMBER ||
                member.role == TeamMemberRole.COLEADER ||
                member.role == TeamMemberRole.LEADER);
        }

        public async UniTask<bool> JoinAsync()
        {
            try
            {
                var response = await TeamService.JoinTeamAsync(id);
                TeamService.UpdateFromResponse(response, this);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to join team: {ex.Message}");
                return false;
            }
        }

        public bool IsMember()
        {
            return members.Exists(m =>
                m.id == Social.Instance.GetPlayer().socialId &&
                (m.role == TeamMemberRole.MEMBER ||
                 m.role == TeamMemberRole.COLEADER ||
                 m.role == TeamMemberRole.LEADER));
        }

        public async UniTask<bool> LeaveAsync()
        {
            try
            {
                await TeamService.LeaveTeamAsync();
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to leave team: {ex.Message}");
                return false;
            }
        }

        public async UniTask<bool> UpdateTeamInfoAsync(
            string name, 
            int capacity, 
            int requiredLevel, 
            TeamType teamType, 
            string description, 
            int badgeId)
        {
            try
            {
                var response = await TeamService.UpdateTeamAsync(
                    name,
                    capacity,
                    requiredLevel,
                    (int)teamType,
                    description,
                    badgeId);
                    
                TeamService.UpdateFromResponse(response, this);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to update team info: {ex.Message}");
                return false;
            }
        }

        public async UniTask<bool> KickMemberAsync(TeamMember member)
        {
            try
            {
                var response = await TeamService.KickMemberAsync(member.id);
                TeamService.UpdateFromResponse(response, this);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to kick member: {ex.Message}");
                return false;
            }
        }
        
        public async UniTask<List<TeamMember>> GetJoinRequestsAsync()
        {
            try
            {
                return await TeamService.GetJoinRequestsAsync();
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to get join requests: {ex.Message}");
                return new List<TeamMember>();
            }
        }
        
        public async UniTask<bool> AcceptJoinRequestAsync(string requestingSocialId)
        {
            try
            {
                await TeamService.AcceptJoinRequestAsync(requestingSocialId);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to accept join request: {ex.Message}");
                return false;
            }
        }
        
        public async UniTask<bool> RejectJoinRequestAsync(string requestingSocialId)
        {
            try
            {
                await TeamService.RejectJoinRequestAsync(requestingSocialId);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Failed to reject join request: {ex.Message}");
                return false;
            }
        }
        
        public async UniTask<bool> IncrementStatAsync(int incrementValue, string statId)
        {
            try
            {
                await TeamService.IncrementStatAsync(incrementValue, statId);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("Team", $"Error incrementing objective: {ex.Message}");
                return false;
            }
        }
    }
}