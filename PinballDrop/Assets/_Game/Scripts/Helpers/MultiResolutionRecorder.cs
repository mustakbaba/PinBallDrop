#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using System.Collections.Generic;

public class MultiResolutionScreenshotWindow : EditorWindow
{
    private Queue<(int width, int height, string name)> resolutionsQueue = new Queue<(int, int, string)>();
    private bool isProcessing = false;

    [MenuItem("Tools/Multi Resolution Screenshot")]
    public static void ShowWindow()
    {
        GetWindow<MultiResolutionScreenshotWindow>("Multi Screenshot");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("📸 Take Screenshots (All Resolutions)", GUILayout.Height(40)))
        {
            StartScreenshotProcess();
        }
    }

    private void StartScreenshotProcess()
    {
        if (isProcessing) return;

        // Çözünürlük listesi
        resolutionsQueue.Clear();
        resolutionsQueue.Enqueue((2048, 2732, "Record_12.9inch"));
        resolutionsQueue.Enqueue((1242, 2688, "Record_6.7inch"));

        isProcessing = true;
        ProcessNextResolution();
    }

    private void ProcessNextResolution()
    {
        if (resolutionsQueue.Count == 0)
        {
            isProcessing = false;
            Debug.Log("✅ All screenshots taken.");
            return;
        }

        var (width, height, name) = resolutionsQueue.Dequeue();

        // GameView boyutunu ayarla
        SetGameViewSize(width, height);

        // 2 frame bekle
        int framesWaited = 0;
        EditorApplication.update += WaitFrames;

        void WaitFrames()
        {
            framesWaited++;
            if (framesWaited >= 2)
            {
                EditorApplication.update -= WaitFrames;
                TakeScreenshot(width, height, name);
                // Sonraki çözünürlüğe geç
                EditorApplication.delayCall += ProcessNextResolution;
            }
        }
    }

    private void TakeScreenshot(int width, int height, string name)
    {
        string folderPath = "Assets/Recordings";
        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);

        var settings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        settings.name = name;
        settings.Enabled = true;
        settings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        settings.CaptureAlpha = false;
        string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        settings.OutputFile = $"{folderPath}/{name}_{timeStamp}";
        settings.RecordMode = RecordMode.SingleFrame;
        settings.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth = width,
            OutputHeight = height
        };

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        controllerSettings.AddRecorderSettings(settings);

        var controller = new RecorderController(controllerSettings);
        controller.PrepareRecording();
        controller.StartRecording();

        EditorApplication.delayCall += () =>
        {
            controller.StopRecording();
            Debug.Log($"📸 Saved: {settings.OutputFile}.png");
            AssetDatabase.Refresh();
        };
    }

    // Game View boyutunu değiştiren fonksiyon
    private void SetGameViewSize(int width, int height)
    {
        var group = GameViewSizeGroupType.Standalone;
        var gameViewSizesInstance = GetGameViewSizesInstance();
        var gameViewSizesType = gameViewSizesInstance.GetType();

        // Mevcut boyut var mı?
        int foundIndex = -1;
        var getGroup = gameViewSizesType.GetMethod("GetGroup");
        var groupObj = getGroup.Invoke(gameViewSizesInstance, new object[] { (int)group });
        var groupType = groupObj.GetType();
        var getDisplayTexts = groupType.GetMethod("GetDisplayTexts");
        var texts = (string[])getDisplayTexts.Invoke(groupObj, null);

        string sizeName = $"{width}x{height}";
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].Contains(sizeName))
            {
                foundIndex = i;
                break;
            }
        }

        // Yoksa ekle
        if (foundIndex == -1)
        {
            var addCustomSize = groupType.GetMethod("AddCustomSize");
            var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var gameViewSizeCtor = gameViewSizeType.GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(string) });
            var fixedResolution = 1; // 0: Aspect Ratio, 1: Fixed Resolution
            var customSize = gameViewSizeCtor.Invoke(new object[] { fixedResolution, width, height, sizeName });
            addCustomSize.Invoke(groupObj, new object[] { customSize });
            foundIndex = texts.Length; // Yeni eklenen index
        }

        // Seç
        var gameViewWindowType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var gameViewWindow = EditorWindow.GetWindow(gameViewWindowType);
        var selectedSizeIndexProp = gameViewWindowType.GetProperty("selectedSizeIndex",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        selectedSizeIndexProp.SetValue(gameViewWindow, foundIndex);
        gameViewWindow.Repaint();
    }

    private object GetGameViewSizesInstance()
    {
        var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        return instanceProp.GetValue(null);
    }
}
#endif
