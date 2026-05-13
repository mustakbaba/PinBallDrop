using System;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Team.Model.Response;
using ElephantSDK;
using ElephantSocial.Model;
using ElephantSocial.Team.Model;
using ElephantSocial.Team.Model.Request;
using UnityEngine.Networking;

namespace ElephantSocial.Team.Network
{
    public class TeamOps : GenericResponseOps
    {
        private async UniTask<T> MakeRequestAsync<T>(string url, object data) where T : new()
        {
            var timeout = RemoteConfig.GetInstance().GetInt("team_base_timeout", 30);
            var bodyJson = PrepareBodyJson(data);
            
            var utcs = new UniTaskCompletionSource<T>();
            var networkManager = new GenericNetworkManager<T>();
            
            ElephantCore.Instance.StartCoroutine(
                networkManager.PostWithResponseSocial(
                    url, 
                    bodyJson,
                    response => 
                    {
                        if (response.responseCode == 200 || response.responseCode == 201)
                        {
                            utcs.TrySetResult(response.data);
                        }
                        else
                        {
                            utcs.TrySetException(new Exception(response.errorMessage));
                        }
                    },
                    error => utcs.TrySetException(new Exception(error)),
                    timeout,
                    false,
                    request => 
                    {
                        var response = SocialUtils.GetTournamentErrorResponse(request);
                        utcs.TrySetException(new Exception($"Error {response.ErrorCode}: {response.Message}"));
                    }
                )
            );
            
            return await utcs.Task;
        }
        
        public UniTask<Player> GetPlayerAsync()
        {
            var data = new PlayerRequest();
            var url = IsProductionEnvironment() ? SocialConst.PlayerEp : SocialConstDev.PlayerEp;
            return MakeRequestAsync<Player>(url, data);
        }
        
        public UniTask<TeamsListResponse> SuggestTeamsAsync()
        {
            var data = new SuggestTeamsRequest { Level = MetaDataUtils.GetInstance().GetCurrentLevel() };
            var url = IsProductionEnvironment() ? SocialConst.GetTeamsEp : SocialConstDev.GetTeamsEp;
            return MakeRequestAsync<TeamsListResponse>(url, data);
        }
        
        public UniTask<TeamsListResponse> SearchTeamsAsync(string searchTerm)
        {
            var data = new SearchTeamsRequest { SearchTerm = searchTerm };
            var url = IsProductionEnvironment() ? SocialConst.SearchTeamsEp : SocialConstDev.SearchTeamsEp;
            return MakeRequestAsync<TeamsListResponse>(url, data);
        }
        
        public UniTask<TeamResponse> GetTeamAsync(string teamId)
        {
            var data = new GetTeamRequest { TeamId = teamId };
            var url = IsProductionEnvironment() ? SocialConst.GetTeamDetailEp : SocialConstDev.GetTeamDetailEp;
            return MakeRequestAsync<TeamResponse>(url, data);
        }
        
        public UniTask<TeamResponse> CreateTeamAsync(CreateTeamRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.CreateTeamEp : SocialConstDev.CreateTeamEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask<TeamResponse> JoinTeamAsync(JoinTeamRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.JoinTeamEp : SocialConstDev.JoinTeamEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask<TeamResponse> LeaveTeamAsync(LeaveTeamRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.LeaveTeamEp : SocialConstDev.LeaveTeamEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask<TeamResponse> UpdateTeamAsync(UpdateTeamRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.UpdateTeamEp : SocialConstDev.UpdateTeamEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask<TeamResponse> PromoteMemberAsync(PromoteMemberRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.PromoteTeamMemberEp : SocialConstDev.PromoteTeamMemberEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask<TeamResponse> DemoteMemberAsync(DemoteMemberRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.DemoteTeamMemberEp : SocialConstDev.DemoteTeamMemberEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }

        public UniTask<TeamResponse> KickMemberAsync(KickMemberRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.KickTeamMemberEp : SocialConstDev.KickTeamMemberEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask<JoinRequestsResponse> GetJoinRequestsAsync(JoinRequestsRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.JoinRequestsEp : SocialConstDev.JoinRequestsEp;
            return MakeRequestAsync<JoinRequestsResponse>(url, request);
        }
        
        public UniTask<PlayerJoinRequestsResponse> GetPlayerJoinRequestsAsync(PlayerJoinRequestsRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.PlayerJoinRequestsEp : SocialConstDev.PlayerJoinRequestsEp;
            return MakeRequestAsync<PlayerJoinRequestsResponse>(url, request);
        }
        
        public UniTask AcceptJoinRequestAsync(AcceptJoinRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.AcceptJoinEp : SocialConstDev.AcceptJoinEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask RejectJoinRequestAsync(RejectJoinRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.RejectJoinEp : SocialConstDev.RejectJoinEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public UniTask IncrementStat(IncrementStatRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.IncrementStatEp : SocialConstDev.IncrementStatEp;
            return MakeRequestAsync<TeamResponse>(url, request);
        }
        
        public async UniTask<bool> IsServerHealthyAsync()
        {
            var url = IsProductionEnvironment() ? SocialConst.HealthCheckEp : SocialConstDev.HealthCheckEp;
            var healthCheckTimeout = RemoteConfig.GetInstance().GetInt("team_health_check_timeout", 5);

            try
            {
                using var request = UnityWebRequest.Get(url);
                request.timeout = healthCheckTimeout;
                
                await request.SendWebRequest().ToUniTask();
                
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    ElephantLog.LogError("TEAM", $"Server health check failed: {request.error}");
                    return false;
                }
                else
                {
                    ElephantLog.Log("TEAM", "Server is healthy: " + request.downloadHandler.text);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("TEAM", $"Health check exception: {ex.Message}");
                return false;
            }
        }
    }
}