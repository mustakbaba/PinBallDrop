using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

namespace ElephantSdkManager.Util
{
    public static class FacebookSettingsManager 
    {
        private const string FacebookSettingsPath = "Assets/FacebookSDK/SDK/Resources";
        
        private static bool IsFacebookSdkAvailable()
        {
            var facebookSettingsType = Type.GetType("Facebook.Unity.Settings.FacebookSettings, Facebook.Unity.Settings");
            var manifestModType = Type.GetType("Facebook.Unity.Editor.ManifestMod, Facebook.Unity.Editor");
            return facebookSettingsType != null && manifestModType != null;
        }

        public static void SetupFacebookSettings(string appId, string clientToken)
        {
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientToken))
            {
                Debug.LogError("Facebook App ID or Client Token is empty. Skipping Facebook settings setup.");
                return;
            }

            if (!IsFacebookSdkAvailable())
            {
                Debug.LogWarning("Facebook SDK is not installed. Facebook settings will be configured after Facebook SDK installation.");
                EditorPrefs.SetString("ElephantSDK_PendingFacebookAppId", appId);
                EditorPrefs.SetString("ElephantSDK_PendingFacebookClientToken", clientToken);
                return;
            }

            try
            {
                SetupFacebookSettingsInternal(appId, clientToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating Facebook Settings: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void SetupFacebookSettingsInternal(string appId, string clientToken)
        {
            if (!Directory.Exists(FacebookSettingsPath))
            {
                Directory.CreateDirectory(FacebookSettingsPath);
                Debug.Log("Created Facebook SDK Resources directory");
            }

            var facebookSettingsType = Type.GetType("Facebook.Unity.Settings.FacebookSettings, Facebook.Unity.Settings");
            var manifestModType = Type.GetType("Facebook.Unity.Editor.ManifestMod, Facebook.Unity.Editor");

            if (facebookSettingsType == null || manifestModType == null)
            {
                Debug.LogError("Could not find Facebook SDK types through reflection.");
                return;
            }

            var nullableInstanceProperty = facebookSettingsType.GetProperty("NullableInstance", BindingFlags.Public | BindingFlags.Static);
            var instance = nullableInstanceProperty?.GetValue(null) as ScriptableObject;

            if (instance == null)
            {
                CreateFacebookSettingsAsset(facebookSettingsType);
                instance = nullableInstanceProperty?.GetValue(null) as ScriptableObject;
            }

            if (instance == null)
            {
                Debug.LogError("Could not create or find FacebookSettings instance.");
                return;
            }

            var appIdsProperty = facebookSettingsType.GetProperty("AppIds", BindingFlags.Public | BindingFlags.Static);
            var appLabelsProperty = facebookSettingsType.GetProperty("AppLabels", BindingFlags.Public | BindingFlags.Static);
            var clientTokensProperty = facebookSettingsType.GetProperty("ClientTokens", BindingFlags.Public | BindingFlags.Static);
            var appLinkSchemesProperty = facebookSettingsType.GetProperty("AppLinkSchemes", BindingFlags.Public | BindingFlags.Static);

            var appIds = appIdsProperty?.GetValue(null) as System.Collections.Generic.List<string>;
            var appLabels = appLabelsProperty?.GetValue(null) as System.Collections.Generic.List<string>;
            var clientTokens = clientTokensProperty?.GetValue(null) as System.Collections.Generic.List<string>;
            var appLinkSchemes = appLinkSchemesProperty?.GetValue(null) as System.Collections.IList;

            if (appIds == null || appLabels == null || clientTokens == null || appLinkSchemes == null)
            {
                Debug.LogError("Could not access Facebook settings properties.");
                return;
            }

            if (appIds.Count == 0)
            {
                appIds.Add("0");
                appLabels.Add("New App");
                clientTokens.Add(string.Empty);
                
                var urlSchemesType = facebookSettingsType.GetNestedType("UrlSchemes");
                if (urlSchemesType != null)
                {
                    var urlSchemesInstance = Activator.CreateInstance(urlSchemesType);
                    appLinkSchemes.Add(urlSchemesInstance);
                }
            }

            appIds[0] = appId;
            clientTokens[0] = clientToken;

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();

            var generateManifestMethod = manifestModType.GetMethod("GenerateManifest", BindingFlags.Public | BindingFlags.Static);
            generateManifestMethod?.Invoke(null, null);

            AssetDatabase.Refresh();

            Debug.Log($"Successfully updated Facebook Settings:\nApp ID: {appId}\nClient Token: {clientToken}");
            
            EditorPrefs.DeleteKey("ElephantSDK_PendingFacebookAppId");
            EditorPrefs.DeleteKey("ElephantSDK_PendingFacebookClientToken");
        }

        private static void CreateFacebookSettingsAsset(Type facebookSettingsType)
        {
            try
            {
                var instance = ScriptableObject.CreateInstance(facebookSettingsType);
                var fullPath = Path.Combine(FacebookSettingsPath, "FacebookSettings.asset");

                AssetDatabase.CreateAsset(instance, fullPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Created new FacebookSettings asset");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create FacebookSettings asset: {ex.Message}");
                throw;
            }
        }

        [InitializeOnLoadMethod]
        private static void ApplyPendingFacebookSettings()
        {
            if (!IsFacebookSdkAvailable())
                return;

            var pendingAppId = EditorPrefs.GetString("ElephantSDK_PendingFacebookAppId", "");
            var pendingClientToken = EditorPrefs.GetString("ElephantSDK_PendingFacebookClientToken", "");

            if (!string.IsNullOrEmpty(pendingAppId) && !string.IsNullOrEmpty(pendingClientToken))
            {
                Debug.Log("Applying pending Facebook settings...");
                SetupFacebookSettings(pendingAppId, pendingClientToken);
            }
        }
    }
}