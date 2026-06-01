using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ElephantSdkManager.Util
{
    [Serializable]
    public class AdjustDeepLinkingBackup
    {
        public string iOSUrlIdentifier;
        public string[] iOSUrlSchemes;
        public string[] iOSUniversalLinksDomains;
        public string[] androidUriSchemes;
        public string[] androidAppLinksDomains;
        public string androidCustomActivityName;
    }

    public static class AdjustSettingsManager
    {
        private const string BackupFileName = "AdjustSettingsBackup.json";
        private const string BackupDirectory = "Assets/ElephantSdkManager/Resources";

        private static string BackupFilePath => Path.Combine(BackupDirectory, BackupFileName);

        private static bool IsAdjustSdkAvailable()
        {
            return GetAdjustSettingsType() != null;
        }

        private static Type GetAdjustSettingsType()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType("AdjustSdk.AdjustSettings");
                    if (type != null) return type;
                }
                catch
                {
                }
            }
            return null;
        }

        public static void BackupDeepLinkingSettings()
        {
            if (!IsAdjustSdkAvailable())
            {
                return;
            }

            try
            {
                var backup = GetCurrentDeepLinkingSettings();
                if (backup == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(backup.iOSUrlIdentifier) &&
                    (backup.iOSUrlSchemes == null || backup.iOSUrlSchemes.Length == 0) &&
                    (backup.iOSUniversalLinksDomains == null || backup.iOSUniversalLinksDomains.Length == 0) &&
                    (backup.androidUriSchemes == null || backup.androidUriSchemes.Length == 0) &&
                    (backup.androidAppLinksDomains == null || backup.androidAppLinksDomains.Length == 0) &&
                    string.IsNullOrEmpty(backup.androidCustomActivityName))
                {
                    return;
                }

                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                }

                var json = JsonUtility.ToJson(backup, true);
                File.WriteAllText(BackupFilePath, json);

                Debug.Log("[AdjustSettingsManager] Backed up Adjust deep linking settings");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdjustSettingsManager] Error backing up settings: {ex.Message}");
            }
        }

        public static void RestoreDeepLinkingSettings()
        {
            if (!IsAdjustSdkAvailable())
            {
                return;
            }

            if (!File.Exists(BackupFilePath))
            {
                return;
            }

            try
            {
                RestoreDeepLinkingSettingsInternal();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdjustSettingsManager] Error restoring settings: {ex.Message}");
            }
        }

        private static void RestoreDeepLinkingSettingsInternal()
        {
            var json = File.ReadAllText(BackupFilePath);
            var backup = JsonUtility.FromJson<AdjustDeepLinkingBackup>(json);

            if (backup == null)
            {
                return;
            }

            var adjustSettingsType = GetAdjustSettingsType();
            if (adjustSettingsType == null)
            {
                return;
            }

            var instanceProperty = adjustSettingsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var instance = instanceProperty?.GetValue(null) as ScriptableObject;

            if (instance == null)
            {
                return;
            }

            var iOSUrlIdentifierProperty = adjustSettingsType.GetProperty("iOSUrlIdentifier", BindingFlags.Public | BindingFlags.Static);
            if (iOSUrlIdentifierProperty != null && !string.IsNullOrEmpty(backup.iOSUrlIdentifier))
            {
                iOSUrlIdentifierProperty.SetValue(null, backup.iOSUrlIdentifier);
            }

            var iOSUrlSchemesProperty = adjustSettingsType.GetProperty("iOSUrlSchemes", BindingFlags.Public | BindingFlags.Static);
            if (iOSUrlSchemesProperty != null && backup.iOSUrlSchemes != null && backup.iOSUrlSchemes.Length > 0)
            {
                iOSUrlSchemesProperty.SetValue(null, backup.iOSUrlSchemes);
            }

            var iOSUniversalLinksDomainsProperty = adjustSettingsType.GetProperty("iOSUniversalLinksDomains", BindingFlags.Public | BindingFlags.Static);
            if (iOSUniversalLinksDomainsProperty != null && backup.iOSUniversalLinksDomains != null && backup.iOSUniversalLinksDomains.Length > 0)
            {
                iOSUniversalLinksDomainsProperty.SetValue(null, backup.iOSUniversalLinksDomains);
            }

            var androidUriSchemesProperty = adjustSettingsType.GetProperty("AndroidUriSchemes", BindingFlags.Public | BindingFlags.Static);
            if (androidUriSchemesProperty != null && backup.androidUriSchemes != null && backup.androidUriSchemes.Length > 0)
            {
                androidUriSchemesProperty.SetValue(null, backup.androidUriSchemes);
            }

            var androidAppLinksDomainsProperty = adjustSettingsType.GetProperty("AndroidAppLinksDomains", BindingFlags.Public | BindingFlags.Static);
            if (androidAppLinksDomainsProperty != null && backup.androidAppLinksDomains != null && backup.androidAppLinksDomains.Length > 0)
            {
                androidAppLinksDomainsProperty.SetValue(null, backup.androidAppLinksDomains);
            }

            var androidCustomActivityProperty = adjustSettingsType.GetProperty("AndroidCustomActivityName", BindingFlags.Public | BindingFlags.Static);
            if (androidCustomActivityProperty != null && !string.IsNullOrEmpty(backup.androidCustomActivityName))
            {
                androidCustomActivityProperty.SetValue(null, backup.androidCustomActivityName);
            }

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();

            Debug.Log("[AdjustSettingsManager] Restored Adjust deep linking settings");

            DeleteBackupFile();
        }

        private static AdjustDeepLinkingBackup GetCurrentDeepLinkingSettings()
        {
            var adjustSettingsType = GetAdjustSettingsType();
            if (adjustSettingsType == null)
            {
                return null;
            }

            var backup = new AdjustDeepLinkingBackup();

            try
            {
                var iOSUrlIdentifierProperty = adjustSettingsType.GetProperty("iOSUrlIdentifier", BindingFlags.Public | BindingFlags.Static);
                if (iOSUrlIdentifierProperty != null)
                {
                    backup.iOSUrlIdentifier = iOSUrlIdentifierProperty.GetValue(null) as string;
                }

                var iOSUrlSchemesProperty = adjustSettingsType.GetProperty("iOSUrlSchemes", BindingFlags.Public | BindingFlags.Static);
                if (iOSUrlSchemesProperty != null)
                {
                    backup.iOSUrlSchemes = iOSUrlSchemesProperty.GetValue(null) as string[];
                }

                var iOSUniversalLinksDomainsProperty = adjustSettingsType.GetProperty("iOSUniversalLinksDomains", BindingFlags.Public | BindingFlags.Static);
                if (iOSUniversalLinksDomainsProperty != null)
                {
                    backup.iOSUniversalLinksDomains = iOSUniversalLinksDomainsProperty.GetValue(null) as string[];
                }

                var androidUriSchemesProperty = adjustSettingsType.GetProperty("AndroidUriSchemes", BindingFlags.Public | BindingFlags.Static);
                if (androidUriSchemesProperty != null)
                {
                    backup.androidUriSchemes = androidUriSchemesProperty.GetValue(null) as string[];
                }

                var androidAppLinksDomainsProperty = adjustSettingsType.GetProperty("AndroidAppLinksDomains", BindingFlags.Public | BindingFlags.Static);
                if (androidAppLinksDomainsProperty != null)
                {
                    backup.androidAppLinksDomains = androidAppLinksDomainsProperty.GetValue(null) as string[];
                }

                var androidCustomActivityProperty = adjustSettingsType.GetProperty("AndroidCustomActivityName", BindingFlags.Public | BindingFlags.Static);
                if (androidCustomActivityProperty != null)
                {
                    backup.androidCustomActivityName = androidCustomActivityProperty.GetValue(null) as string;
                }
            }
            catch
            {
                return null;
            }

            return backup;
        }

        private static void DeleteBackupFile()
        {
            try
            {
                if (File.Exists(BackupFilePath))
                {
                    File.Delete(BackupFilePath);
                    var metaFile = BackupFilePath + ".meta";
                    if (File.Exists(metaFile))
                    {
                        File.Delete(metaFile);
                    }
                }
            }
            catch
            {
            }
        }
    }
}