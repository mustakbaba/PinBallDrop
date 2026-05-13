using System.IO;
using UnityEngine;
using UnityEditor;

namespace ElephantSDK.Editor
{
    public static class ElephantSplashScreenUpdater
    {
        public static void UpdateSplashScreen()
        {
            // Main Splash Screen settings
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;

            // Background color (#301867 - dark purple)
            Color backgroundColor = new Color(
                (float)0x30 / 255f,
                (float)0x18 / 255f,
                (float)0x67 / 255f,
                1f);

            var isUpdated = false;
            if (PlayerSettings.SplashScreen.backgroundColor != backgroundColor)
            {
                PlayerSettings.SplashScreen.backgroundColor = backgroundColor;
                isUpdated = true;
            }

            if (PlayerSettings.SplashScreen.show)
            {
                PlayerSettings.SplashScreen.show = false;
                isUpdated = true;
            }

            // Set overlay opacity (0-1 range)
            if (!Mathf.Approximately(PlayerSettings.SplashScreen.overlayOpacity, 1f))
            {
                PlayerSettings.SplashScreen.overlayOpacity = 1f;
                isUpdated = true;
            }

            // Disable background blur
            if (PlayerSettings.SplashScreen.blurBackgroundImage)
            {
                PlayerSettings.SplashScreen.blurBackgroundImage = false;
                isUpdated = true;
            }

            // Splash style (Light on Dark)
            if (PlayerSettings.SplashScreen.unityLogoStyle != PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark)
            {
                PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;
                isUpdated = true;
            }

            // Set animation type to static
            if (PlayerSettings.SplashScreen.animationMode != PlayerSettings.SplashScreen.AnimationMode.Static)
            {
                PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
                isUpdated = true;
            }

            if (PlayerSettings.SplashScreen.logos.Length > 0)
            {
                // Clear logos and custom images - Using most aggressive method
                try
                {
                    // 1. First try the normal way
                    PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[0];

                    // 2. Go deeper and clear via SerializedObject
                    SerializedObject playerSettings =
                        new SerializedObject(
                            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
                    SerializedProperty splashScreenLogos = playerSettings.FindProperty("m_SplashScreenLogos");

                    if (splashScreenLogos != null)
                    {
                        splashScreenLogos.ClearArray();
                        playerSettings.ApplyModifiedProperties();
                    }

                    // 3. Force clear cache
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                    // 4. More aggressive cleaning method
                    string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";

                    // Mark file as editable
                    if (!AssetDatabase.IsOpenForEdit(projectSettingsPath, out string message))
                    {
                        if (!AssetDatabase.MakeEditable(projectSettingsPath))
                        {
                            Debug.LogWarning("Could not make ProjectSettings.asset editable: " + message);
                        }
                    }

                    // 5. Save settings - try again
                    EditorUtility.SetDirty(playerSettings.targetObject);
                    AssetDatabase.SaveAssets();

                    Debug.Log(
                        "Logo cleaning process completed. Restarting Unity is still recommended for persistence.");
                    isUpdated = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error during logo cleaning: " + e.Message);
                }
            }

            if (isUpdated)
            {
                // Save changes
                AssetDatabase.SaveAssets();

                Debug.Log("Elephant: Splash screen settings updated successfully.");

                // Suggest restart to user
                if (EditorUtility.DisplayDialog("Splash Screen Updated",
                        "Splash screen settings have been successfully updated. However, you may need to restart the Unity Editor for logo changes to fully apply.\n\nWould you like to restart now?",
                        "Yes, Restart Now",
                        "No, Later"))
                {
                    // Restart the Editor
                    EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                }
            }
        }
    }
}