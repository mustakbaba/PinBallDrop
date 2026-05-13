namespace ElephantSDK
{
#if UNITY_IOS
public class IOSPlatformService : IPlatformService
{
    public void ShowPopUp(string title, string message)
    {
        // iOS-specific implementation
        ElephantIOS.showPopUpView(title, message, "", "", "", "", "", "", "");
    }
}
#elif UNITY_ANDROID
public class AndroidPlatformService : IPlatformService
{
    public void ShowPopUp(string title, string message)
    {
        // Android-specific implementation
        ElephantAndroid.ShowConsentDialogOnUiThread(title, message, "", "", "", "", "", "", "");
    }
}
#elif UNITY_EDITOR
    public class EditorPlatformService : IPlatformService
    {
        public void ShowPopUp(string title, string message)
        {
            // Editor-specific no-op or mock
            ElephantLog.Log("COMPLIANCE TEST", $"{title}: {message}");
        }
    }
#else
    public class EditorPlatformService : IPlatformService
    {
        public void ShowPopUp(string title, string message)
        {
            // Editor-specific no-op or mock
            ElephantLog.Log("COMPLIANCE TEST", $"{title}: {message}");
        }
    }
#endif
}