#if UNITY_IOS
using AppLovinMax.Scripts.IntegrationManager.Editor;
using RollicGames.Advertisements;
using UnityEditor;
using UnityEditor.Callbacks;

namespace RollicGames.Editor
{
    public class ApplovinPostProcess
    {
        [PostProcessBuild(45)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
        {
            if (target == BuildTarget.iOS)
            {
                SetupGoogleIDs();
                SetupApplovinEditor();
            }
        }
        
        private static void SetupGoogleIDs()
        {
            AppLovinSettings.Instance.AdMobIosAppId = RollicApplovinIDs.GoogleIosId;
            AppLovinSettings.Instance.AdMobAndroidAppId = RollicApplovinIDs.GoogleAndroidId;
        }
        
        private static void SetupApplovinEditor()
        {
            EditorPrefs.SetBool(AppLovinAutoUpdater.KeyAutoUpdateEnabled, false);
        }
    }
}
#endif