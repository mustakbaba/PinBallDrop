using System;
using ElephantSDK;

namespace ElephantSocial
{
    [Serializable]
    public class SocialConfig
    {
        public ElephantEnvironment elephantEnvironment;

        public SocialConfig(ElephantEnvironment elephantEnvironment)
        {
            this.elephantEnvironment = elephantEnvironment;
        }
    }
}