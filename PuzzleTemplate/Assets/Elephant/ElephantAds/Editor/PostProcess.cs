#if UNITY_IOS
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace RollicGames.Editor
{
    public class PostProcess
    {
        [PostProcessBuild(46)]
        public static void OnPostprocessBuildForFramework(BuildTarget target, string pathToBuiltProject) {
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));
                
                EditBuildSettings(project);

                File.WriteAllText(projectPath, project.WriteToString());
            }
        }
        
        [PostProcessBuild(102)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));

                EditInfoPlist(pathToBuiltProject);

                // Write.
                File.WriteAllText(projectPath, project.WriteToString());
            }
        }

        private static void EditBuildSettings(PBXProject project)
        {
            string unityIphoneGuid = project.GetUnityMainTargetGuid();
            if (unityIphoneGuid != null)
            {
                project.SetBuildProperty(unityIphoneGuid, "ENABLE_BITCODE", "NO");
                project.SetBuildProperty(unityIphoneGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            }
            
            string unityFrameworkGuid = project.GetUnityFrameworkTargetGuid();
            if (unityFrameworkGuid != null)
            {
                project.SetBuildProperty(unityFrameworkGuid, "ENABLE_BITCODE", "NO");
                project.SetBuildProperty(unityIphoneGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            }

            string unityIphoneProjectGuid = project.ProjectGuid();
            if (unityIphoneProjectGuid != null)
            {
                project.SetBuildProperty(unityIphoneProjectGuid, "ENABLE_BITCODE", "NO");
            }
                
            string unityIphoneTestGuid = project.TargetGuidByName("Unity-iPhone Tests");
            if (unityIphoneTestGuid != null)
            {
                project.SetBuildProperty(unityIphoneTestGuid, "ENABLE_BITCODE", "NO");
            }
            
            string oneSignalGuid = project.TargetGuidByName("OneSignalNotificationService");
            if (oneSignalGuid != null)
            {
                project.SetBuildProperty(oneSignalGuid, "ENABLE_BITCODE", "NO");
            }
                
            string widgetGuid = project.TargetGuidByName("widgetExtension");
            if (widgetGuid != null)
            {
                project.SetBuildProperty(widgetGuid, "ENABLE_BITCODE", "NO");
            }
        }

        private static void EditInfoPlist(string pathToBuiltProject)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            
            PlistElementDict rootDict = plist.root;
            var rootValues = rootDict.values;
            
            // Consent Mode v2 (For Firebase)
            rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", true);
            rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", true);
            rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", true);
            rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", true);
            
            // Fyber
            rootValues.Remove("NSAllowsArbitraryLoadsInWebContent");
            rootValues.Remove("NSAllowsArbitraryLoadsForMedia");
            rootValues.Remove("NSAllowsLocalNetworking");
            rootDict.SetBoolean("NSAllowsArbitraryLoads", true);
            rootDict.SetString("NSCalendarsUsageDescription", "${PRODUCT_NAME} requests access to the Calendar");
            rootDict.SetString("NSLocationWhenInUseUsageDescription", "${PRODUCT_NAME} requests location access when used to provide a better advertising experience.");

            // SKAdNetwork
            var array = rootDict.CreateArray("SKAdNetworkItems");
            XElement[] elements = XDocument.Load(@"Assets/Elephant/ElephantAds/Resources/SkanIds.xml").Descendants("dict").ToArray();
            
            foreach (var element in elements)
            {
                var dict = array.AddDict();
                
                var key = "SKAdNetworkIdentifier";
                var networkId = element.Value.Replace("SKAdNetworkIdentifier", "");
                
                dict.SetString(key, networkId);
            }
            
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif