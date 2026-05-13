namespace ElephantSDK
{
    public static class PlatformServiceFactory
    {
        public static IPlatformService GetPlatformService()
        {
#if UNITY_IOS
            return new IOSPlatformService();
#elif UNITY_ANDROID
            return new AndroidPlatformService();
#else
            return new EditorPlatformService();
#endif
        }
    }
}