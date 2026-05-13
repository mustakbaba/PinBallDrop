using UnityEngine;
using System;
using ElephantSDK;
using Newtonsoft.Json;

namespace RollicGames.Advertisements
{
    public static class MaxVersionReporter
    {
        public static void ReportVersionsToCrashlytics()
        {
            try
            {
                var versionsAsset = Resources.Load<TextAsset>("MaxNetworkVersions");
                if (versionsAsset == null)
                {
                    ElephantLog.Log("MaxVersionReporter", "MAX network versions file not found");
                    return;
                }

                var versions = JsonConvert.DeserializeObject<NetworkVersionsData>(versionsAsset.text);
                if (versions?.Networks == null)
                {
                    ElephantLog.Log("MaxVersionReporter", "Failed to parse MAX network versions data");
                    return;
                }

                foreach (var kvp in versions.Networks)
                {
                    var network = kvp.Key.ToLower();
                    var adapter = ElephantCore.Instance?.FirebaseElephantAdapter;
                    if (adapter == null) continue;
#if UNITY_ANDROID
                    adapter.SetCustomKey($"max_network_{network}_android_adapter", kvp.Value.Android);
#elif UNITY_IOS
                    adapter.SetCustomKey($"max_network_{network}_ios_adapter", kvp.Value.Ios);
#endif
                }
            }
            catch (Exception e)
            {
                ElephantLog.LogError("MaxVersionReporter", $"Failed to report MAX versions to Crashlytics: {e.Message}");
            }
        }
    }
}