using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace RollicGames.Advertisements.Editor
{
    public class MaxVersionRecorderEditor
    {
        private const string VersionsAssetPath = "Assets/Elephant/ElephantAds/MAX/Resources/MaxNetworkVersions.json";
        private const string MaxMediationPath = "MaxSdk/Mediation/";

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var versions = CollectNetworkVersions();
            SaveVersions(versions);
            Debug.Log($"Saved versions for {versions.Networks.Count} MAX networks");
        }

        private static NetworkVersionsData CollectNetworkVersions()
        {
            var networks = new Dictionary<string, NetworkVersionInfo>();

            var mediationFiles = AssetDatabase.FindAssets("Dependencies t:TextAsset")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.Contains(MaxMediationPath) && path.EndsWith("Dependencies.xml"));

            foreach (var dependencyPath in mediationFiles)
            {
                var parts = dependencyPath.Split('/');
                int networkIndex = Array.FindIndex(parts, x => x == "Mediation") + 1;
                if (networkIndex < parts.Length)
                {
                    string networkName = parts[networkIndex];
                    var versionInfo = ExtractNetworkVersions(dependencyPath);
                    networks[networkName] = versionInfo;
                    Debug.Log($"Found {networkName}: Android {versionInfo.Android}, iOS {versionInfo.Ios}");
                }
            }

            return new NetworkVersionsData { Networks = networks };
        }

        private static NetworkVersionInfo ExtractNetworkVersions(string dependencyPath)
        {
            var doc = XDocument.Load(dependencyPath);
            var deps = doc.Element("dependencies");
            var versionInfo = new NetworkVersionInfo();

            var androidPackages = deps?.Element("androidPackages");
            if (androidPackages != null)
            {
                var adapterPackage = androidPackages.Descendants("androidPackage")
                    .FirstOrDefault(e => e.Attribute("spec")?.Value.StartsWith("com.applovin") == true);
                versionInfo.Android = adapterPackage != null
                    ? adapterPackage.Attribute("spec").Value.Split(':').Last().Trim('[', ']')
                    : "";
            }

            var iosPods = deps?.Element("iosPods");
            if (iosPods != null)
            {
                var adapterPod = iosPods.Descendants("iosPod")
                    .FirstOrDefault(e => e.Attribute("name")?.Value.StartsWith("AppLovin") == true);
                versionInfo.Ios = adapterPod != null ? adapterPod.Attribute("version").Value : "";
            }

            return versionInfo;
        }

        private static void SaveVersions(NetworkVersionsData versions)
        {
            string json = JsonConvert.SerializeObject(versions, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(VersionsAssetPath));
            File.WriteAllText(VersionsAssetPath, json);
            AssetDatabase.Refresh();
        }
    }
}