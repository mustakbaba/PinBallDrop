using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class LevelEditorWindow : EditorWindow
{
    LevelData selectedLevelData;
    Vector2 scrollPos; // SCROLL BAR pozisyonu

    static LevelEditorWindow _instance;

    static LevelEditorWindow()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Play mode'dan edit mode'a DÖNÜLDÜĞÜ AN
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (_instance != null && _instance.selectedLevelData != null)
            {
                Debug.Log("🌀 Play mode'dan çıkıldı, level tekrar yükleniyor...");
                LevelManager.Instance.LoadLevel(_instance.selectedLevelData);
            }
        }
    }

    [MenuItem("Sincapp/Level Editor Window")]
    static void Init()
    {
        _instance = GetWindow<LevelEditorWindow>("Level Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        GUILayout.Space(5);

        EditorGUILayout.LabelField("Selected Level", selectedLevelData != null ? selectedLevelData.name : "None");

        GUILayout.Space(5);

        GUILayout.Label("Save Level", EditorStyles.boldLabel);

        if (GUILayout.Button("📁 Save As New", GUILayout.Height(30)))
        {
            SaveAsNewLevel();
        }

        if (GUILayout.Button("💾 Save (Overwrite)", GUILayout.Height(30)))
        {
            if (selectedLevelData != null)
            {
                bool result = EditorUtility.DisplayDialog("Onay", "Levelin üzerine yazmak istediginize emin misiniz?",
                    "Evet", "Hayır");
                if (result)
                {
                    OverrideLevel(selectedLevelData);
                }
                else
                {
                    Debug.Log("Islem iptal edildi");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Save Error", "No level selected to overwrite!", "OK");
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Saved Levels", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300)); // 300px yükseklik örnek

        var guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/_Game/Scenes/Resources/Levels" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<LevelData>(path);

            if (GUILayout.Button(asset.name))
            {
                selectedLevelData = asset;
                LevelManager.Instance.LoadLevel(asset); // Tıklayınca direk yükler
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void SaveAsNewLevel()
    {
        var newLevel = ScriptableObject.CreateInstance<LevelData>();
        LevelManager.Instance.Save(newLevel);

        #region Save

        string[] files = System.IO.Directory.GetFiles("Assets/_Game/Scenes/Resources/Levels", "Level_*.asset");
        int maxIndex = 0;
        foreach (var file in files)
        {
            string names = System.IO.Path.GetFileNameWithoutExtension(file);
            string[] parts = names.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out int idx))
            {
                if (idx > maxIndex) maxIndex = idx;
            }
        }

        int nextIndex = maxIndex + 1;
        string assetPath = $"Assets/_Game/Scenes/Resources/Levels/Level_{nextIndex:D3}.asset";

        AssetDatabase.CreateAsset(newLevel, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ New Level Saved: {assetPath}");

        #endregion
    }

    private void OverrideLevel(LevelData levelToSave)
    {
        if (levelToSave == null)
        {
            Debug.LogError("No level selected to overwrite!");
            return;
        }

        LevelManager.Instance.Save(levelToSave);

        EditorUtility.SetDirty(levelToSave);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ Level Overwritten: {levelToSave.name}");
    }

}