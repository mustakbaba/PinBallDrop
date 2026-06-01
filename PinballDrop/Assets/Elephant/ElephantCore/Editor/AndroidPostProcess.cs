#if UNITY_ANDROID
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace ElephantSDK
{
    public class AndroidPostProcess : IPostGenerateGradleAndroidProject
    {
        private string _manifestFilePath;
        
        public int callbackOrder => 1;
        
        private static bool isPushEnabled = true;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // If needed, add condition checks on whether you need to run the modification routine.
            // For example, specific configuration/app options enabled
            var androidManifest = new AndroidManifest(GetManifestPath(path));

            androidManifest.SetAdIdPermission();
            
            //FOR HAPTIC BUG
            androidManifest.SetVibratePermission();
            
            if (isPushEnabled)
            {
                androidManifest.SetNotificationPermission();
                androidManifest.SetFirebaseMessagingService();
            }
            
            androidManifest.UpdateUnityPlayerActivityName("com.rollic.elephantsdk.ElephantActivity");
            
            androidManifest.SetGwpAsanMode();

            androidManifest.RemoveToolsNode();

            androidManifest.AddWebViewControllerActivity();
            
            // Add your XML manipulation routines
            androidManifest.Save();
        }
        
        private string GetManifestPath(string basePath)
        {
			var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();
            return _manifestFilePath;
        }
    }
    
    internal class AndroidXmlDocument : XmlDocument
        {
            private string m_Path;
            protected XmlNamespaceManager nsMgr;
            public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
            public AndroidXmlDocument(string path)
            {
                m_Path = path;
                using (var reader = new XmlTextReader(m_Path))
                {
                    reader.Read();
                    Load(reader);
                }
                nsMgr = new XmlNamespaceManager(NameTable);
                nsMgr.AddNamespace("android", AndroidXmlNamespace);
                nsMgr.AddNamespace("tools", "http://schemas.android.com/tools");
            }

            public string Save()
            {
                return SaveAs(m_Path);
            }

            public string SaveAs(string path)
            {
                using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
                {
                    writer.Formatting = Formatting.Indented;
                    Save(writer);
                }
                return path;
            }
        }

        internal class AndroidManifest : AndroidXmlDocument
        {
            private readonly XmlElement _applicationElement;

            public AndroidManifest(string path) : base(path)
            {
                _applicationElement = SelectSingleNode("/manifest/application") as XmlElement;
            }

            private XmlAttribute CreateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            internal void SetAdIdPermission()
            {
                var manifest = SelectSingleNode("/manifest");
                XmlElement child = CreateElement("uses-permission");
                manifest.AppendChild(child);
                XmlAttribute newAttribute = CreateAndroidAttribute("name", "com.google.android.gms.permission.AD_ID");
                child.Attributes.Append(newAttribute);
            }
                        
            internal void SetNotificationPermission()
            {
                var manifest = SelectSingleNode("/manifest");
                XmlElement child = CreateElement("uses-permission");
                manifest.AppendChild(child);
                XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.POST_NOTIFICATIONS");
                child.Attributes.Append(newAttribute);
            }
            
            internal void SetVibratePermission()
            {
                var manifest = SelectSingleNode("/manifest");
                var existingPermission = SelectSingleNode("/manifest/uses-permission[@android:name='android.permission.VIBRATE']", nsMgr);
    
                if (existingPermission == null)
                {
                    XmlElement child = CreateElement("uses-permission");
                    manifest.AppendChild(child);
                    XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.VIBRATE");
                    child.Attributes.Append(newAttribute);
                }
            }

            internal void SetFirebaseMessagingService()
            {
                var applicationNode = SelectSingleNode("/manifest/application");
                
                XmlElement serviceElement = CreateElement("service");
                applicationNode.AppendChild(serviceElement);
                
                XmlAttribute serviceNameAttribute = CreateAndroidAttribute("name", "com.rollic.elephantsdk.MyFirebaseMessagingService");
                serviceElement.Attributes.Append(serviceNameAttribute);

                XmlAttribute exportedAttribute = CreateAndroidAttribute("exported", "false");
                serviceElement.Attributes.Append(exportedAttribute);

                XmlElement intentFilterElement = CreateElement("intent-filter");
                serviceElement.AppendChild(intentFilterElement);

                XmlElement actionElement = CreateElement("action");
                intentFilterElement.AppendChild(actionElement);

                XmlAttribute actionNameAttribute = CreateAndroidAttribute("name", "com.google.firebase.MESSAGING_EVENT");
                actionElement.Attributes.Append(actionNameAttribute);
            }
            
            internal void UpdateUnityPlayerActivityName(string newActivityName)
            {
                var xpath = "/manifest/application/activity[@android:name='com.unity3d.player.UnityPlayerActivity']";
                var unityPlayerActivityNode = SelectSingleNode(xpath, nsMgr);
        
                if (unityPlayerActivityNode is XmlElement activityElement)
                {
                    activityElement.SetAttribute("name", AndroidXmlNamespace, newActivityName);
                }
            }
            
            internal void RemoveToolsNode()
            {
                //Find and remove property node
                var propertyNode = SelectSingleNode("/manifest/application/property[@tools:node='removeAll']", nsMgr);
                propertyNode?.ParentNode?.RemoveChild(propertyNode);
            }
            
            internal void SetGwpAsanMode()
            {
                if (_applicationElement != null)
                {
                    _applicationElement.SetAttribute("gwpAsanMode", AndroidXmlNamespace, "always");
                }
            }

            internal void AddWebViewControllerActivity()
            {
                if (_applicationElement == null) 
                {
                    return;
                }


                XmlElement newActivity = CreateElement("activity");
                newActivity.SetAttribute("name", AndroidXmlNamespace, "com.rollic.elephantsdk.WebViewController");
                newActivity.SetAttribute("launchMode", AndroidXmlNamespace, "singleTop");
                newActivity.SetAttribute("exported", AndroidXmlNamespace, "false");
                newActivity.SetAttribute("excludeFromRecents", AndroidXmlNamespace, "true");
                newActivity.SetAttribute("taskAffinity", AndroidXmlNamespace, "");
                newActivity.SetAttribute("hardwareAccelerated", AndroidXmlNamespace, "true");
                newActivity.SetAttribute("theme", AndroidXmlNamespace, "@style/Theme.AppCompat.DayNight.NoActionBar");

                _applicationElement.AppendChild(newActivity);
                
                Debug.Log("Added WebViewController activity to manifest.");
            }
        }
}
#endif