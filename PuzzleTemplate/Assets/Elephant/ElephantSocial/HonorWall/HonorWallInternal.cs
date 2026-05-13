using System;
using ElephantSDK;

namespace ElephantSocial.HonorWall
{
    internal class HonorWallInternal : SocialDataStore
    {
        private static HonorWallInternal _instance;
        private readonly HonorWallOps _honorWallOps;

        internal static HonorWallInternal GetInstance()
        {
            return _instance ??= new HonorWallInternal();
        }

        private HonorWallInternal()
        {
            _honorWallOps = new HonorWallOps();
        }

        public void GetHonors(Action<HonorWallResponse> onSuccess, Action<string> onFailed, Action<string> onError)
        {
            var getHonorsJob = _honorWallOps.GetHonors(
                response => HandleResponse(response,
                    successResponse => onSuccess?.Invoke(successResponse),
                    onError),
                failedResponse => HandleErrorResponse(failedResponse,
                    (errorCode, message) => onFailed?.Invoke(message)),
                onError
            );

            ElephantCore.Instance.StartCoroutine(getHonorsJob);
        }

        public void GrantHonor(int honorId, Action onSuccess, Action<string> onFailed, Action<string> onError)
        {
            var grantHonorJob = _honorWallOps.GrantHonor(
                honorId,
                response => HandleResponse(response,
                    successResponse => onSuccess?.Invoke(),
                    onError),
                failedResponse => HandleErrorResponse(failedResponse,
                    (errorCode, message) => onFailed?.Invoke(message)),
                onError
            );

            ElephantCore.Instance.StartCoroutine(grantHonorJob);
        }
    }
}