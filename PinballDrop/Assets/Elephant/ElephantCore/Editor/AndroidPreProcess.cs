#if UNITY_ANDROID
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GooglePlayServices;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ElephantSDK
{
    public class ManageBillingDependencyPreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            UpdateBillingDependency();
        }

        private static bool IsPurchasingInstalled()
        {
            var projectRootPath = Path.GetDirectoryName(Application.dataPath);
            var manifestPath = Path.Combine(projectRootPath, "Packages/manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.Log("Couldn't find manifest in: " + manifestPath);
                return false;
            }

            var jsonText = File.ReadAllText(manifestPath);
            var containsPurchasing = jsonText.Contains("com.unity.purchasing");

            Debug.Log(containsPurchasing ? "Purchasing exists in manifest." : "Purchasing does not exist in manifest.");

            return containsPurchasing;
        }

        private void UpdateBillingDependency()
        {
            var xmlFilePath = Path.Combine(Application.dataPath, "Elephant/ElephantCore/Editor/ElephantDependencies.xml");
            var xmlDoc = XDocument.Load(xmlFilePath);
            if (xmlDoc.Root == null)
                return;
            var androidPackages = xmlDoc.Root.Element("androidPackages");
            
            const string billingSpec = "com.android.billingclient:billing:7.1.1";
            
            if (androidPackages == null)
                return;
            var billingDependency = androidPackages.Elements("androidPackage")
                .FirstOrDefault(e => e.Attribute("spec")?.Value == billingSpec);

            if (IsPurchasingInstalled())
            {
                if (billingDependency == null)
                    return;
                billingDependency.Remove();
                xmlDoc.Save(xmlFilePath);
                PlayServicesResolver.Resolve();
                Debug.Log("Billing dependency removed from ElephantDependencies.xml");
            }
            else
            {
                if (billingDependency != null)
                    return;
                androidPackages.Add(new XElement("androidPackage", new XAttribute("spec", billingSpec)));
                xmlDoc.Save(xmlFilePath);
                PlayServicesResolver.Resolve();
                Debug.Log("Billing dependency added to ElephantDependencies.xml");
            }
        }
    }
}
#endif