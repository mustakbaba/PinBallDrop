#if UNITY_IOS
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ElephantSDK
{
    public class iOSPreProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        private static string minOSVersion = "15.0";

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) 
            {
                return;
            }

            PlayerSettings.iOS.targetOSVersionString = minOSVersion;
        }
    }
}
#endif
