using UnityEngine;
using UnityEditor;
using System.IO;
using ElephantSDK;
using ElephantSDK.Editor;

public class ElephantLogoEditorTool : EditorWindow
{
    private Texture2D rollicLogo;
    private Texture2D userLogo;
    private Color backgroundColor;
    private string logoPath = "Assets/Elephant/ElephantCore/UI/Textures/Resources/rollic_pnw_white_logo.png";
    private const string LogoEditorSettings = "Assets/Resources/ElephantResources/LogoEditorSettings.json";
    private const string StudioLogoAssetPath = "Assets/Resources/ElephantResources/StudioLogo.png";
    private bool showPreview = true;
    private GUIStyle headerStyle;
    private bool rollicLogoEnabled = true;

    [System.Serializable]
    private class ElephantBrandingSettings
    {
        public bool isRollicLogoEnabled = ElephantBrandingDefaults.RollicLogoEnabledByDefault;
        public Color backgroundColor = ElephantBrandingDefaults.RollicBackgroundColor;
    }
    
    [MenuItem("Elephant/Logo Editor")]
    public static void ShowWindow()
    {
        GetWindow<ElephantLogoEditorTool>("Logo Editor");
    }

    private void OnEnable()
    {
        // Set window size
        minSize = new Vector2(800, 500);
        
        // Prepare header style
        headerStyle = new GUIStyle();
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.margin = new RectOffset(10, 10, 10, 10);
        
        // Load Rollic logo
        rollicLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);
        if (rollicLogo == null)
        {
            Debug.LogWarning("Rollic logo not found: " + logoPath);
        }

        LoadSettings();
        LoadExistingStudioLogo();
    }

    private void LoadExistingStudioLogo()
    {
        if (userLogo != null)
        {
            return;
        }
        if (!File.Exists(StudioLogoAssetPath))
        {
            return;
        }

        userLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(StudioLogoAssetPath);
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        // Left panel - Controls
        DrawControlPanel();

        // Right panel - Preview
        if (showPreview)
        {
            DrawPreviewPanel();
        }
        else
        {
            // Show empty area if preview is hidden
            GUILayout.BeginVertical("Box", GUILayout.Width(position.width / 2 - 5), GUILayout.Height(position.height));
            GUILayout.Label("Preview hidden", EditorStyles.centeredGreyMiniLabel);
            GUILayout.EndVertical();
        }
        
        GUILayout.EndHorizontal();

        Repaint();
    }

    private void DrawControlPanel()
    {
        GUILayout.BeginVertical("Box", GUILayout.Width(position.width / 2 - 5), GUILayout.Height(position.height));
        
        EditorGUILayout.LabelField("Logo Editor Tool", headerStyle);
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Branding", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select whether to show the Rollic logo.", MessageType.None);
        EditorGUILayout.Space(4);

        rollicLogoEnabled = EditorGUILayout.ToggleLeft("Show Rollic Logo", rollicLogoEnabled);
        EditorGUILayout.Space(8);

        if (!rollicLogoEnabled)
        {
            EditorGUILayout.HelpBox("Custom background is used when Rollic logo is disabled.", MessageType.None);
            EditorGUILayout.Space(4);
            backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
            EditorGUILayout.Space(6);
            if (GUILayout.Button("Reset Branding Settings", GUILayout.Height(22)))
            {
                rollicLogoEnabled = true;
                backgroundColor = ElephantBrandingDefaults.RollicBackgroundColor;
                ShowNotification(new GUIContent("Branding reset. Click Save to apply."));
            }

            EditorGUILayout.Space(8);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Studio Logo", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Pick a Texture2D to export as StudioLogo.png.", MessageType.None);
        EditorGUILayout.Space(4);

        userLogo = (Texture2D) EditorGUILayout.ObjectField(
            "Texture", userLogo, typeof(Texture2D), false
        );
        

        EditorGUILayout.Space(12);
        if (GUILayout.Button("Save", GUILayout.Height(32)))
        {
            Save();
        }
        
        GUILayout.EndVertical();
    }

    private void DrawPreviewPanel()
    {
        GUILayout.BeginVertical("Box", GUILayout.Width(position.width / 2 - 5), GUILayout.Height(position.height));
        
        // iPhone 13 dimensions (390 x 844)
        float iPhoneWidth = 390;
        float iPhoneHeight = 844;
        
        // Calculate scale factor to fit the iPhone preview in the available space
        float availableWidth = position.width / 2 - 30;
        float availableHeight = position.height - 40;
        
        float scaleFactorWidth = availableWidth / iPhoneWidth;
        float scaleFactorHeight = availableHeight / iPhoneHeight;
        
        // Use the smaller scale factor to ensure the entire iPhone fits
        float scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);
        
        // Calculate the scaled dimensions
        float scaledWidth = iPhoneWidth * scaleFactor;
        float scaledHeight = iPhoneHeight * scaleFactor;
        
        // Create rect for iPhone preview with margins to center it
        float leftMargin = (availableWidth - scaledWidth) / 2 + 15;
        float topMargin = (availableHeight - scaledHeight) / 2 + 20;
        
        Rect previewRect = new Rect(
            position.width / 2 + leftMargin,
            topMargin,
            scaledWidth,
            scaledHeight
        );
        
        // Draw background (this will be the iPhone screen)
        EditorGUI.DrawRect(previewRect, rollicLogoEnabled ? ElephantBrandingDefaults.RollicBackgroundColor : backgroundColor);
        
        // Label to indicate this is an iPhone 13 preview
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUI.LabelField(
            new Rect(previewRect.x, previewRect.y - 20, previewRect.width, 20),
            "iPhone 13 (390 x 844)",
            labelStyle
        );

        if (rollicLogoEnabled)
        {
            if (rollicLogo != null)
            {
                // Calculate positions for logo
                float logoWidth = scaledWidth * 0.5f;
                float logoHeight = logoWidth * (rollicLogo.height / (float)rollicLogo.width);
            
                // Position logo in center
                Rect logoRect = new Rect(
                    previewRect.x + (previewRect.width - logoWidth) / 2,
                    previewRect.y + (previewRect.height - logoHeight) / 2 - scaledHeight * 0.05f,
                    logoWidth,
                    logoHeight
                );
            
                // Draw Rollic logo
                GUI.DrawTexture(logoRect, rollicLogo, ScaleMode.ScaleToFit);
                // Draw user logo if one has been selected/dropped
                if (userLogo != null)
                {
                    // Create area for user logo
                    float userLogoWidth = scaledWidth * 0.6f;
                    float userLogoHeight = userLogoWidth * 0.4f;
                
                    var iconArea = new Rect(
                        previewRect.x + (previewRect.width - userLogoWidth) / 2,
                        logoRect.y + logoRect.height + scaledHeight * 0.15f,
                        userLogoWidth,
                        userLogoHeight
                    );
                
                    GUI.DrawTexture(iconArea, userLogo, ScaleMode.ScaleToFit);
                }
            }
        }
        else
        {
            if (userLogo != null)
            {
                // Create area for user logo
                float userLogoWidth = scaledWidth * 0.6f;
                float userLogoHeight = userLogoWidth * 0.4f;
                
                Rect iconArea = new Rect(
                    previewRect.x + (previewRect.width - userLogoWidth) / 2,
                    previewRect.y + (previewRect.height - userLogoHeight) / 2 - scaledHeight * 0.05f,
                    userLogoWidth,
                    userLogoHeight
                );
                
                GUI.DrawTexture(iconArea, userLogo, ScaleMode.ScaleToFit);
            }
        }
        
        GUILayout.EndVertical();
    }

    private void Save()
    {
        if (userLogo == null)
        {
            EditorUtility.DisplayDialog("Error", "Please drag a logo image to the white area first!", "OK");
            return;
        }

        // Get source asset path
        string sourcePath = AssetDatabase.GetAssetPath(userLogo);
        if (string.IsNullOrEmpty(sourcePath))
        {
            EditorUtility.DisplayDialog("Error", "Could not find the source file for the selected logo.", "OK");
            return;
        }

        // Check and create target directory
        string directoryPath = "Assets/Resources/ElephantResources";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            AssetDatabase.Refresh();
        }

        // Target file path
        string destinationPath = Path.Combine(directoryPath, "StudioLogo.png");

        try
        {
            var normalizedSource = sourcePath.Replace('\\', '/');
            var normalizedDestination = destinationPath.Replace('\\', '/');

            if (string.Equals(normalizedSource, normalizedDestination, System.StringComparison.Ordinal))
            {
                AssetDatabase.Refresh();
            }
            else
            {
                // Delete existing file if it exists
                if (File.Exists(destinationPath))
                {
                    AssetDatabase.DeleteAsset(destinationPath);
                }

                // Copy the file
                FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);
                AssetDatabase.Refresh();
            }
            
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(destinationPath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            if (!TrySaveLogoEditorSettings(out var settingsError))
            {
                EditorUtility.DisplayDialog(
                    "Save incomplete",
                    "Studio logo was written to:\n" + destinationPath +
                    "\n\nBranding settings could not be saved to:\n" + LogoEditorSettings +
                    "\n\n" + settingsError,
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "Success",
                "Logo saved to:\n" + destinationPath +
                "\n\nBranding settings saved to:\n" + LogoEditorSettings,
                "OK");
            ElephantSplashScreenUpdater.UpdateSplashScreen();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Error saving logo: " + e.Message, "OK");
            Debug.LogError("Error saving logo: " + e);
        }
    }

    private void LoadSettings()
    {
        rollicLogoEnabled = true;
        backgroundColor = ElephantBrandingDefaults.RollicBackgroundColor;

        try
        {
            if (!File.Exists(LogoEditorSettings))
            {
                return;
            }

            var json = File.ReadAllText(LogoEditorSettings);
            var settings = JsonUtility.FromJson<ElephantBrandingSettings>(json);
            if (settings == null)
            {
                return;
            }

            rollicLogoEnabled = settings.isRollicLogoEnabled;
            backgroundColor = settings.backgroundColor;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to load logo editor settings at {LogoEditorSettings}. Using defaults. Exception: {e.Message}");
        }
    }

    private bool TrySaveLogoEditorSettings(out string errorMessage)
    {
        errorMessage = null;
        try
        {
            var directoryPath = Path.GetDirectoryName(LogoEditorSettings);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var settings = new ElephantBrandingSettings
            {
                isRollicLogoEnabled = rollicLogoEnabled,
                backgroundColor = rollicLogoEnabled ? ElephantBrandingDefaults.RollicBackgroundColor : backgroundColor
            };

            var json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(LogoEditorSettings, json);
            AssetDatabase.Refresh();
            return true;
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
            Debug.LogWarning($"Failed to save logo editor settings at {LogoEditorSettings}. Exception: {e.Message}");
            return false;
        }
    }
}