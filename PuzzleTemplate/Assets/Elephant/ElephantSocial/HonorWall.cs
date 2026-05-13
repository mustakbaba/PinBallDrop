using System;

namespace ElephantSocial.HonorWall
{
    public class HonorWall
    {
        private static readonly Lazy<HonorWall> _instance = new(() => new HonorWall());
        public static HonorWall Instance => _instance.Value;
        
        private readonly HonorWallInternal _honorWallInternal;

        private HonorWall()
        {
            _honorWallInternal = HonorWallInternal.GetInstance();
        }

        public void GetHonors(Action<HonorWallResponse> onSuccess, Action<string> onError)
        {
            _honorWallInternal.GetHonors(onSuccess, onError, onError);
        }

        public void GrantHonor(int honorId, Action onSuccess, Action<string> onError)
        {
            _honorWallInternal.GrantHonor(honorId, onSuccess, onError, onError);
        }
    }
}