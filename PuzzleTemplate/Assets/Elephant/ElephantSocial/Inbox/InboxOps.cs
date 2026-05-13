using System;
using ElephantUniTask.Threading.Tasks;
using ElephantSocial.Inbox.Model.Request;
using ElephantSocial.Inbox.Model.Response;
using ElephantSDK;
using ElephantSocial;

namespace ElephantSocial.Inbox
{
    public class InboxOps : GenericResponseOps
    {
        private async UniTask<T> MakeRequestAsync<T>(string url, object data) where T : new()
        {
            var timeout = RemoteConfig.GetInstance().GetInt("inbox_base_timeout", 30);
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
        
        private async UniTask MakeVoidRequestAsync(string url, object data)
        {
            var timeout = RemoteConfig.GetInstance().GetInt("inbox_base_timeout", 30);
            var bodyJson = PrepareBodyJson(data);
            
            var utcs = new UniTaskCompletionSource();
            var networkManager = new GenericNetworkManager<object>();
            
            ElephantCore.Instance.StartCoroutine(
                networkManager.PostWithResponseSocial(
                    url, 
                    bodyJson,
                    response => 
                    {
                        if (response.responseCode == 200 || response.responseCode == 201)
                        {
                            utcs.TrySetResult();
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
            
            await utcs.Task;
        }
        
        public UniTask<InboxResponse> GetInboxAsync(GetInboxRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.GetInboxEp : SocialConstDev.GetInboxEp;
            return MakeRequestAsync<InboxResponse>(url, request);
        }
        
        public UniTask MarkAsReadAsync(MarkAsReadRequest request)
        {
            var url = IsProductionEnvironment() ? SocialConst.InboxReadEp : SocialConstDev.InboxReadEp;
            return MakeVoidRequestAsync(url, request);
        }
    }
}