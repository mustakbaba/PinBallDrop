namespace ElephantSDK
{
    /// <summary>
    /// Manager for Age Range API (Declared Age Range on iOS, Google Play Age Signals on Android).
    /// </summary>
    public class AgeRangeManager
    {
        /// <summary>
        /// Check if the Age Range API is available on the current platform.
        /// </summary>
        public static bool IsApiAvailable()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return ElephantIOS.IsDeclaredAgeRangeAvailable();
/*#elif UNITY_ANDROID && !UNITY_EDITOR
            return ElephantAndroid.IsPlayAgeSignalsAvailable();*/
#else
            return false;
#endif
        }

        /// <summary>
        /// Request age range from the platform
        /// </summary>
        public static void RequestAgeRange()
        {
            var isAvailable = IsApiAvailable();
            var param = Params.New().Set("api_available", isAvailable.ToString());
            Elephant.Event("age_api_available", -1, param);
            
            if (!isAvailable)
            {
                ElephantLog.LogError("AGE_RANGE", "API is not available");
                return;
            }
            
#if UNITY_IOS && !UNITY_EDITOR
            ElephantIOS.RequestDeclaredAgeRange();
#elif UNITY_ANDROID && !UNITY_EDITOR
            //ElephantAndroid.RequestPlayAgeSignals();
#endif
        }
    }
}
