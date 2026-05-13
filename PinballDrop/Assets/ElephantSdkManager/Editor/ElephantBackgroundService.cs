using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ElephantSdkManager.Util;

namespace ElephantSdkManager
{
    [InitializeOnLoad]
    public class ElephantBackgroundService
    {
        private static readonly Dictionary<string, string> CleanupRules = new()
        {
            { "Plugins/Android/Helpshift.aar", "2025.04.0" },
        };
        
        static ElephantBackgroundService()
        {
            AssetDatabase.importPackageCompleted += OnPackageImported;
        }
        
        private static void OnPackageImported(string packageName)
        {
            if (packageName.ToLower().Contains("elephant") || packageName.ToLower().Contains("gamekit"))
            {
                PerformCleanup();
            }
        }

        private static void PerformCleanup()
        {
            try
            {
                Debug.Log("[Elephant] Checking for cleanup...");
                var currentVersion = VersionUtils.GetGameKitVersion();
                if (string.IsNullOrEmpty(currentVersion)) return;

                GC.Collect();
                Resources.UnloadUnusedAssets();
                
                foreach (var (filePath, versionThreshold) in CleanupRules)
                {
                    if (VersionUtils.CompareVersions(currentVersion, versionThreshold) <= 0) 
                        continue;
                        
                    var fullPath = Path.Combine(Application.dataPath, filePath);
                    if (!File.Exists(fullPath)) continue;
                    
                    try
                    {
                        File.Delete(fullPath);
                        var metaFilePath = fullPath + ".meta";
                        if (File.Exists(metaFilePath))
                            File.Delete(metaFilePath);
                                    
                        Debug.Log($"[Elephant] Cleaned up file: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Elephant] Failed to delete {filePath}: {ex.Message}");
                        EditorApplication.delayCall += () => RetryCleanup(filePath);
                    }
                }
                
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Elephant] Error during cleanup: {ex.Message}");
            }
        }
        
        private static void RetryCleanup(string filePath)
        {
            var fullPath = Path.Combine(Application.dataPath, filePath);
            if (!File.Exists(fullPath)) return;
            
            try
            {
                GC.Collect();
                Resources.UnloadUnusedAssets();
                    
                File.Delete(fullPath);
                var metaFilePath = fullPath + ".meta";
                if (File.Exists(metaFilePath))
                    File.Delete(metaFilePath);
                
                Debug.Log($"[Elephant] Successfully deleted {filePath} on retry");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Elephant] Retry failed for {filePath}: {ex.Message}");
            }
        }
    }
}