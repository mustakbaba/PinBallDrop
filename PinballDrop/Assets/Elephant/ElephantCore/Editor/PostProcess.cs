#if UNITY_IOS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace ElephantSDK
{
    public class PostProcess
    {
        private static bool isPushEnabled = true;
        
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));
                
#if UNITY_2019_3_OR_NEWER
                string xcodeTarget = project.GetUnityFrameworkTargetGuid();
#else
                string xcodeTarget = project.TargetGuidByName("Unity-iPhone");
#endif
                project.AddFrameworkToProject(xcodeTarget, "StoreKit.framework", true);
                project.AddFrameworkToProject(xcodeTarget, "AppTrackingTransparency.framework", true);
                project.AddFrameworkToProject(xcodeTarget, "WebKit.framework", false);
                project.AddFrameworkToProject(xcodeTarget, "AdSupport.framework", true);
                
                // Set Swift Libraries embedding to NO
                project.SetBuildProperty(xcodeTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

                EditInfoPlist(pathToBuiltProject);

                File.WriteAllText(projectPath, project.WriteToString());
                                
                AddCapabilities(projectPath, project);
            }
        }

        static void AddCapabilities(string projPath, PBXProject proj)
        {
            var manager = new ProjectCapabilityManager(
                projPath,
                "Unity-iPhone.entitlements",
                targetGuid: proj.GetUnityMainTargetGuid()
            );
            if (isPushEnabled)
            {
                manager.AddPushNotifications(true);
                manager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            }
            manager.WriteToFile();
        }
        
        private static void EditInfoPlist(string pathToBuiltProject)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            
            PlistElementDict rootDict = plist.root;

            // ATT
            rootDict.SetString("NSUserTrackingUsageDescription", "Your data will only be used to deliver personalized ads to you.");
            
            // Attribution report endpoint
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://adjust-skadnetwork.com");
            
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif