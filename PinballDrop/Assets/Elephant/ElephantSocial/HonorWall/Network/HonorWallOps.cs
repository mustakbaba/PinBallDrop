using System;
using System.Collections;
using ElephantSDK;
using ElephantSocial.Model;
using UnityEngine.Networking;

namespace ElephantSocial.HonorWall
{
    public class HonorWallOps : GenericResponseOps
    {
        public IEnumerator GetHonors(
            Action<GenericResponse<HonorWallResponse>> onResponse,
            Action<UnityWebRequest> onFailedResponse,
            Action<string> onError)
        {
            var data = new SocialBaseData();
            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<HonorWallResponse>();
            
            var url = IsProductionEnvironment()
                ? SocialConst.HonorWallEp
                : SocialConstDev.HonorWallEp;
                
            var postWithResponse = networkManager.PostWithResponseSocial(
                url, 
                bodyJson, 
                onResponse, 
                onError, 
                0, 
                false,
                onFailedResponse);

            return postWithResponse;
        }

        public IEnumerator GrantHonor(
            int honorId,
            Action<GenericResponse<HonorWallGrantResponse>> onResponse,
            Action<UnityWebRequest> onFailedResponse,
            Action<string> onError)
        {
            var data = new HonorWallGrantRequest { id = honorId };
            var bodyJson = PrepareBodyJson(data);
            var networkManager = new GenericNetworkManager<HonorWallGrantResponse>();
            
            var url = IsProductionEnvironment()
                ? SocialConst.HonorWallGrantEp
                : SocialConstDev.HonorWallGrantEp;
                
            var postWithResponse = networkManager.PostWithResponseSocial(
                url, 
                bodyJson, 
                onResponse, 
                onError, 
                0, 
                false,
                onFailedResponse);

            return postWithResponse;
        }
    }
}