using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class LevelEditorWindow : EditorWindow
{
    LevelData selectedLevelData;
    Vector2 scrollPos; // SCROLL BAR pozisyonu

    static LevelEditorWindow _instance;

    private void OnInspectorUpdate()
    {
        RefreshStats();
        Repaint();
    }

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

        GUILayout.Space(10);
        GUILayout.Label("Scene Stats", EditorStyles.boldLabel);


        if (GUILayout.Button("🎲 Randomize Ball Colors", GUILayout.Height(30)))
        {
            RandomizeBallColors();
        }

        if (GUILayout.Button("⚖️ Balance Balls to Bumpers", GUILayout.Height(30)))
        {
            BalanceBallsToBumpers();
        }

        DrawStats();

        EditorGUILayout.EndScrollView();
    }

    private Dictionary<ColorTypes, int> _ballStats = new Dictionary<ColorTypes, int>();
    private Dictionary<ColorTypes, int> _bumperStats = new Dictionary<ColorTypes, int>();

    private void BalanceBallsToBumpers()
    {
        var balls = FindObjectsOfType<BallController>();
        var bumperHolders = FindObjectsOfType<BumperHolderController>();
        if (balls.Length == 0 || bumperHolders.Length == 0) return;

        // Ball renklerinin toplam sayısını al
        var ballCounts = new Dictionary<ColorTypes, int>();
        foreach (var ball in balls)
        {
            if (!ballCounts.ContainsKey(ball.Properties.ObjectColor))
                ballCounts[ball.Properties.ObjectColor] = 0;
            ballCounts[ball.Properties.ObjectColor] += ball.Properties.BallAmount;

            if (ball.Properties.BallBlocker == BallController.BallBlockers.MultiBall)
            {
                if (!ballCounts.ContainsKey(ball.Properties.MultiColor))
                    ballCounts[ball.Properties.MultiColor] = 0;
                ballCounts[ball.Properties.MultiColor] += ball.Properties.MultiAmount;
            }
        }

        // Tüm renkleri listeye al ve shuffle et
        var colorList = new List<(ColorTypes color, int amount)>();
        foreach (var kvp in ballCounts)
        {
            int remaining = kvp.Value;
            while (remaining > 0)
            {
                // 10'un katı, 10-30 arası parça
                int chunk = Random.Range(1, 4) * 10; // 10, 20 veya 30
                chunk = Mathf.Min(chunk, remaining);
                // 10'un katına yuvarla
                chunk = Mathf.Max(Mathf.RoundToInt(chunk / 10f) * 10, 10);
                chunk = Mathf.Min(chunk, remaining);
                colorList.Add((kvp.Key, chunk));
                remaining -= chunk;
            }
        }

        // Shuffle
        for (int i = colorList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (colorList[i], colorList[j]) = (colorList[j], colorList[i]);
        }

        // 3 holder'a eşit böl
        int holderCount = bumperHolders.Length;
        var holderChunks = new List<List<(ColorTypes color, int amount)>>();
        for (int i = 0; i < holderCount; i++)
            holderChunks.Add(new List<(ColorTypes, int)>());

        for (int i = 0; i < colorList.Count; i++)
            holderChunks[i % holderCount].Add(colorList[i]);

        // Her holder'a set et
        Undo.RecordObjects(bumperHolders, "Balance Bumpers to Balls");

        for (int h = 0; h < holderCount; h++)
        {
            var holder = bumperHolders[h];
            var chunks = holderChunks[h];

            Undo.RecordObject(holder, "Balance Bumpers to Balls");
            holder.TargetObjects.Clear();

            foreach (var chunk in chunks)
            {
                holder.TargetObjects.Add(new BumperData
                {
                    Color = chunk.color,
                    Amount = chunk.amount,
                    IsHidden = false
                });
            }

            EditorUtility.SetDirty(holder);
            holder.SpawnPrefabs();
        }

        RefreshStats();
        Debug.Log($"✅ {colorList.Count} chunk {holderCount} holder'a dağıtıldı.");
    }

    private void RandomizeBallColors()
    {
        var balls = FindObjectsOfType<BallController>();
        if (balls.Length == 0) return;

        var allColors = System.Enum.GetValues(typeof(ColorTypes));

        foreach (var ball in balls)
        {
            Undo.RecordObject(ball, "Randomize Ball Color");
            ball.Properties.ObjectColor = (ColorTypes)allColors.GetValue(Random.Range(0, allColors.Length));
            ball.SetColor();
            EditorUtility.SetDirty(ball);
        }

        RefreshStats();
    }

    private void RefreshStats()
    {
        _ballStats.Clear();
        _bumperStats.Clear();

        var tunnels = FindObjectsOfType<TunnelSpawnerController>();
        var tunnelBalls = new HashSet<BallController>();

        // Tunnel child'larındaki ball'ları işaretle
        foreach (var tunnel in tunnels)
        {
            var childBalls = tunnel.GetComponentsInChildren<BallController>();
            foreach (var b in childBalls)
                tunnelBalls.Add(b);

            // Tunnel'ın BallDatas'ını say
            foreach (var data in tunnel.BallDatas)
            {
                if (!_ballStats.ContainsKey(data.ObjectColor))
                    _ballStats[data.ObjectColor] = 0;
                _ballStats[data.ObjectColor] += data.BallAmount;

                if (data.BallBlocker == BallController.BallBlockers.MultiBall)
                {
                    if (!_ballStats.ContainsKey(data.MultiColor))
                        _ballStats[data.MultiColor] = 0;
                    _ballStats[data.MultiColor] += data.MultiAmount;
                }
            }
        }

        var balls = FindObjectsOfType<BallController>();
        foreach (var ball in balls)
        {
            if (tunnelBalls.Contains(ball)) continue; // tunnel child'ı, atla

            if (!_ballStats.ContainsKey(ball.ObjectColor))
                _ballStats[ball.ObjectColor] = 0;
            _ballStats[ball.ObjectColor] += ball.Properties.BallAmount;

            if (ball.Properties.BallBlocker == BallController.BallBlockers.MultiBall)
            {
                if (!_ballStats.ContainsKey(ball.Properties.MultiColor))
                    _ballStats[ball.Properties.MultiColor] = 0;
                _ballStats[ball.Properties.MultiColor] += ball.Properties.MultiAmount;
            }
        }

        var bumpers = FindObjectsOfType<BumperController>();
        foreach (var bumper in bumpers)
        {
            if (!_bumperStats.ContainsKey(bumper.ObjectColor))
                _bumperStats[bumper.ObjectColor] = 0;
            _bumperStats[bumper.ObjectColor] += bumper.Count;
        }
    }

    private void DrawStats()
    {
        if (_ballStats.Count == 0 && _bumperStats.Count == 0) return;

        GUILayout.Space(5);

        // Header
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Color", EditorStyles.boldLabel, GUILayout.Width(80));
        GUILayout.Label("Balls", EditorStyles.boldLabel, GUILayout.Width(60));
        GUILayout.Label("Bumpers", EditorStyles.boldLabel, GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();

        // Tüm renkleri birleştir
        var allColors = new HashSet<ColorTypes>();
        foreach (var k in _ballStats.Keys) allColors.Add(k);
        foreach (var k in _bumperStats.Keys) allColors.Add(k);

        foreach (var colorType in allColors)
        {
            int ballCount = _ballStats.ContainsKey(colorType) ? _ballStats[colorType] : 0;
            int bumperCount = _bumperStats.ContainsKey(colorType) ? _bumperStats[colorType] : 0;

            var color = GetEditorColor(colorType);
            var prevColor = GUI.color;
            GUI.color = color;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(colorType.ToString(), GUILayout.Width(80));
            GUILayout.Label(ballCount.ToString(), GUILayout.Width(60));
            GUILayout.Label(bumperCount.ToString(), GUILayout.Width(70));

            if (ballCount == bumperCount)
            {
                GUI.color = Color.green;
                GUILayout.Label("✔", GUILayout.Width(20));
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("✘", GUILayout.Width(20));
            }

            EditorGUILayout.EndHorizontal();

            GUI.color = prevColor;
        }
    }

    private Color GetEditorColor(ColorTypes colorType)
    {
        if (LevelManager.Instance == null || LevelManager.Instance.ObjectColors == null)
            return Color.white;

        var c = LevelManager.Instance.ObjectColors[(int)colorType];
        // Editor'de çok koyu renkleri biraz aydınlat
        return new Color(
            Mathf.Clamp01(c.r + 0.2f),
            Mathf.Clamp01(c.g + 0.2f),
            Mathf.Clamp01(c.b + 0.2f)
        );
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