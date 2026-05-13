#if UNITY_ANDROID
using System.IO;
using System.Xml;
using AppLovinMax.Scripts.IntegrationManager.Editor;
using RollicGames.Advertisements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RollicGames.Editor
{
    public class PostProcessAndroid
    {
        [PostProcessBuild(207)]
        public static void UpdateLauncherManifest(BuildTarget target, string pathToBuiltProject)
        {
            var launcherManifestPath = Path.Combine(pathToBuiltProject, "launcher/src/main/AndroidManifest.xml");
            
            if (!File.Exists(launcherManifestPath)) return;
            
            string[] lines = File.ReadAllLines(launcherManifestPath);
            if (lines.Length == 0) return;
            
            File.Delete(launcherManifestPath);

            using (StreamWriter sw = File.AppendText(launcherManifestPath))
            {
                foreach (string line in lines)
                {
                    string newLine = "";
                    
                    if (line.Contains("com.google.android.gms.ads.APPLICATION_ID"))
                    {
                        newLine = line.Replace("com.google.android.gms.ads.APPLICATION_ID", "xxx");
                    }
                    else
                    {
                        newLine = line;
                    }
                    sw.WriteLine(newLine);
                }
            }
            
            var doc = new XmlDocument();
            doc.Load(launcherManifestPath);

            var manifestNode = doc.SelectSingleNode("/manifest");
            var applicationNode = manifestNode?.SelectSingleNode("application");

            if (applicationNode != null)
            {
                AddMetaData(applicationNode, "google_analytics_default_allow_analytics_storage", "true");
                AddMetaData(applicationNode, "google_analytics_default_allow_ad_storage", "true");
                AddMetaData(applicationNode, "google_analytics_default_allow_ad_user_data", "true");
                AddMetaData(applicationNode, "google_analytics_default_allow_ad_personalization_signals", "true");

                doc.Save(launcherManifestPath);
                Debug.Log("Meta-data tags added to AndroidManifest.xml");
            }

            SetupGoogleIDs();
            SetupApplovinEditor();
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
        
        private static void AddMetaData(XmlNode applicationNode, string name, string value)
        {
            if (applicationNode.OwnerDocument == null) return;

            // Define the namespace URI associated with the 'android' prefix
            var androidNamespace = "http://schemas.android.com/apk/res/android";

            // Create a 'meta-data' element
            var metaDataElement = applicationNode.OwnerDocument.CreateElement("meta-data");

            // Ensure the namespace is declared in the document. This might not be necessary
            // if your document already includes the namespace declaration.
            // This is just to ensure the 'android' prefix is recognized.
            if (applicationNode.OwnerDocument.DocumentElement != null &&
                string.IsNullOrEmpty(applicationNode.OwnerDocument.DocumentElement.GetAttribute("xmlns:android")))
            {
                applicationNode.OwnerDocument.DocumentElement.SetAttribute("xmlns:android", androidNamespace);
            }

            // Use SetAttribute with the namespace URI to correctly set "android:name" and "android:value"
            metaDataElement.SetAttribute("name", androidNamespace, name);
            metaDataElement.SetAttribute("value", androidNamespace, value);

            // Append the 'meta-data' element to the 'applicationNode'
            applicationNode.AppendChild(metaDataElement);
        }
    }
}
#endif
