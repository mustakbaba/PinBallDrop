using System;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Team.Model.Response;
using ElephantSDK;
using ElephantSocial.Model;
using ElephantSocial.Team.Model;
using ElephantSocial.Team.Model.Request;
using ElephantSocial.Team.Model.Response;
using ElephantSocial.Team.Network;

namespace ElephantSocial.Team
{
    public class TeamApi
    {
        private static readonly Lazy<TeamApi> _instance = new();
        public static TeamApi Instance => _instance.Value;

        public TeamApi()
        {
            _teamOps = new TeamOps();
        }

        private readonly TeamOps _teamOps;

        public UniTask<TeamsListResponse> ListTeamsAsync(string searchTerm = "")
        {
            try
            {
                return string.IsNullOrEmpty(searchTerm) ? _teamOps.SuggestTeamsAsync() : _teamOps.SearchTeamsAsync(searchTerm);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error listing teams: {ex.Message}");
                throw;
            }
        }
        
        public UniTask<Player> GetPlayerAsync()
        {
            try
            {
                return _teamOps.GetPlayerAsync();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error getting my team: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> GetTeamAsync(string teamId)
        {
            try
            {
                return _teamOps.GetTeamAsync(teamId);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error getting team {teamId}: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> CreateTeamAsync(string name, string description, int capacity, int requiredLevel, int teamType, int badge)
        {
            try
            {
                var request = new CreateTeamRequest
                {
                    Name = name,
                    Capacity = capacity,
                    RequiredLevel = requiredLevel,
                    TeamType = teamType,
                    Badge = badge,
                    Description = description
                };
    
                return _teamOps.CreateTeamAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error creating team: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> JoinTeamAsync(string teamId)
        {
            try
            {
                var request = new JoinTeamRequest
                {
                    TeamId = teamId
                };

                return _teamOps.JoinTeamAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error joining team {teamId}: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> LeaveTeamAsync()
        {
            try
            {
                var request = new LeaveTeamRequest();

                return _teamOps.LeaveTeamAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error leaving: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> UpdateTeamAsync(
            string name, 
            int capacity, 
            int requiredLevel, 
            int teamType, 
            string description, 
            int badge)
        {
            try
            {
                var request = new UpdateTeamRequest
                {
                    Name = name,
                    Capacity = capacity,
                    RequiredLevel = requiredLevel,
                    TeamType = teamType,
                    Description = description,
                    Badge = badge,
                };

                return _teamOps.UpdateTeamAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> PromoteMemberAsync(string targetSocialId)
        {
            try
            {
                var request = new PromoteMemberRequest
                {
                    TargetSocialId = targetSocialId
                };

                return _teamOps.PromoteMemberAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> DemoteMemberAsync(string targetSocialId)
        {
            try
            {
                var request = new DemoteMemberRequest
                {
                    TargetSocialId = targetSocialId,
                };

                return _teamOps.DemoteMemberAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error demoting member: {ex.Message}");
                throw;
            }
        }

        public UniTask<TeamResponse> KickMemberAsync(string targetSocialId)
        {
            try
            {
                var request = new KickMemberRequest
                {
                    TargetSocialId = targetSocialId
                };

                return _teamOps.KickMemberAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error kicking member: {ex.Message}");
                throw;
            }
        }
        
        public UniTask<JoinRequestsResponse> GetJoinRequestsAsync()
        {
            try
            {
                var request = new JoinRequestsRequest();
                
                return _teamOps.GetJoinRequestsAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error getting join requests: {ex.Message}");
                throw;
            }
        }
        
        public UniTask<PlayerJoinRequestsResponse> GetPlayerJoinRequestsAsync()
        {
            try
            {
                var request = new PlayerJoinRequestsRequest();
                
                return _teamOps.GetPlayerJoinRequestsAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error getting player join requests: {ex.Message}");
                throw;
            }
        }
        
        public UniTask AcceptJoinRequestAsync(string targetSocialId)
        {
            try
            {
                var request = new AcceptJoinRequest
                {
                    TargetSocialId = targetSocialId
                };

                return _teamOps.AcceptJoinRequestAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error accepting join request: {ex.Message}");
                throw;
            }
        }
        
        public UniTask RejectJoinRequestAsync(string targetSocialId)
        {
            try
            {
                var request = new RejectJoinRequest
                {
                    TargetSocialId = targetSocialId
                };

                return _teamOps.RejectJoinRequestAsync(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error rejecting join request: {ex.Message}");
                throw;
            }
        }
        
        public UniTask IncrementStat(int incrementValue, string statId)
        {
            try
            {
                var request = new IncrementStatRequest()
                {
                    IncrementValue = incrementValue,
                    StatId = statId
                };

                return _teamOps.IncrementStat(request);
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error incrementing objective: {ex.Message}");
                throw;
            }
        }

        public UniTask<bool> IsServerHealthyAsync()
        {
            try
            {
                return _teamOps.IsServerHealthyAsync();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TeamApi", $"Error checking server health: {ex.Message}");
                return UniTask.FromResult(false);
            }
        }
    }
}