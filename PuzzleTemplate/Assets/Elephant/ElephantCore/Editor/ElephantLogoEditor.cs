using UnityEngine;
using UnityEditor;
using System.IO;
using ElephantSDK.Editor;

public class ElephantLogoEditorTool : EditorWindow
{
    private Texture2D rollicLogo;
    private Texture2D userLogo;
    private Color backgroundColor = new Color(48f / 255f, 24f / 255f, 103f / 255f); // Dark purple color
    private string logoPath = "Assets/Elephant/ElephantCore/UI/Textures/Resources/rollic_pnw_white_logo.png";
    private bool showPreview = true;
    private GUIStyle headerStyle;

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

        // Repaint every frame
        Repaint();
    }

    private void DrawControlPanel()
    {
        GUILayout.BeginVertical("Box", GUILayout.Width(position.width / 2 - 5), GUILayout.Height(position.height));
        
        EditorGUILayout.LabelField("Logo Editor Tool", headerStyle);
        EditorGUILayout.Space(10);
        
        userLogo = (Texture2D) EditorGUILayout.ObjectField(
            "Logo", userLogo, typeof(Texture2D), false
        );
        
        // Save button
        if (GUILayout.Button("Save Logo", GUILayout.Height(30)))
        {
            SaveLogo();
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
            previewRect = new Rect(
                position.width / 2 + leftMargin,
                topMargin,
                scaledWidth,
                scaledHeight
            )
        );
        
        // Draw background with purple color (this will be the iPhone screen)
        EditorGUI.DrawRect(previewRect, backgroundColor);
        
        // Label to indicate this is an iPhone 13 preview
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUI.LabelField(
            new Rect(previewRect.x, previewRect.y - 20, previewRect.width, 20),
            "iPhone 13 (390 x 844)",
            labelStyle
        );
        
        // Draw Rollic logo and slogan
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
        else
        {
            // Show error if Rollic logo can't be loaded
            GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
            errorStyle.fontSize = 14;
            errorStyle.normal.textColor = Color.red;
            errorStyle.alignment = TextAnchor.MiddleCenter;
            
            Rect errorRect = new Rect(previewRect.x, previewRect.y + previewRect.height / 2, previewRect.width, 30);
            EditorGUI.LabelField(errorRect, "Logo couldn't be loaded!", errorStyle);
        }
        
        GUILayout.EndVertical();
    }

    private void SaveLogo()
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
        
        // Delete existing file if it exists
        if (File.Exists(destinationPath))
        {
            AssetDatabase.DeleteAsset(destinationPath);
        }

        try
        {
            // Copy the file
            FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);
            AssetDatabase.Refresh();
            
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(destinationPath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
            
            EditorUtility.DisplayDialog("Success", "Logo successfully saved to:\n" + destinationPath, "OK");
            ElephantSplashScreenUpdater.UpdateSplashScreen();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Error saving logo: " + e.Message, "OK");
            Debug.LogError("Error saving logo: " + e);
        }
    }
}