using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using UnityEngine;
using UnityEngine.UI;

public class ElephantStudioLogoHelper : MonoBehaviour
{
    private const string LogTag = "ElephantStudioLogoHelper";

    public Canvas canvas;
    public Image logoImage;

    [SerializeField] private List<GameObject> rollicLogo = new List<GameObject>();

    private const string LogoEditorSettingsResourcePath = "ElephantResources/LogoEditorSettings";
    private const float AnchoredYRollicLogo = -60f;
    private const float AnchoredYCustomLogo = 0f;

    [Serializable]
    private class LogoEditorSettingsModel
    {
        public bool isRollicLogoEnabled = ElephantBrandingDefaults.RollicLogoEnabledByDefault;
        public Color backgroundColor = ElephantBrandingDefaults.RollicBackgroundColor;
    }


    private void OnEnable()
    {
        if (canvas == null || logoImage == null)
        {
            ElephantLog.LogError(LogTag, "Canvas or logoImage reference is missing.");
            return;
        }

        Sprite logo = Resources.Load<Sprite>("ElephantResources/StudioLogo");
        if (logo == null)
        {
            ElephantLog.Log(LogTag, "StudioLogo not found at Resources/ElephantResources/StudioLogo — hiding canvas.");
            canvas.gameObject.SetActive(false);
        }
        else
        {
            logoImage.sprite = logo;
            canvas.gameObject.SetActive(true);
            canvas.transform.localScale = Vector3.one * 0.1f;
            var rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.position = Vector2.zero;
            rectTransform.sizeDelta = Vector2.one;
            canvas.worldCamera = Camera.main;
            ElephantLog.Log(LogTag, "StudioLogo loaded. Applying branding from LogoEditorSettings.");
        }

        ApplyBrandingSettings();
    }

    private void ApplyBrandingSettings()
    {
        var settings = LoadLogoEditorSettings();
        var enabled = settings.isRollicLogoEnabled;

        ApplyMainCameraBackground(settings, enabled);
        ApplyLogoImageYOffset(enabled);

        if (rollicLogo == null || rollicLogo.Count == 0)
        {
            ElephantLog.Log(LogTag, "rollicLogo list is empty.");
            return;
        }

        foreach (var go in rollicLogo)
        {
            if (go != null)
            {
                go.SetActive(enabled);
            }
        }
    }

    private void ApplyLogoImageYOffset(bool rollicLogoEnabled)
    {
        if (logoImage == null)
        {
            ElephantLog.Log(LogTag, "logoImage is null.");
            return;
        }

        var rect = logoImage.rectTransform;
        var pos = rect.anchoredPosition;
        pos.y = rollicLogoEnabled ? AnchoredYRollicLogo : AnchoredYCustomLogo;
        rect.anchoredPosition = pos;
    }

    private void ApplyMainCameraBackground(LogoEditorSettingsModel settings, bool rollicLogoEnabled)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            ElephantLog.Log(LogTag, "Camera.main is null.");
            return;
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = rollicLogoEnabled ? ElephantBrandingDefaults.RollicBackgroundColor : settings.backgroundColor;
    }

    private static LogoEditorSettingsModel LoadLogoEditorSettings()
    {
        try
        {
            var textAsset = Resources.Load<TextAsset>(LogoEditorSettingsResourcePath);
            if (textAsset == null || string.IsNullOrWhiteSpace(textAsset.text))
            {
                ElephantLog.Log(LogTag, "LogoEditorSettings TextAsset missing or empty. Using defaults.");
                return new LogoEditorSettingsModel();
            }

            var settings = JsonUtility.FromJson<LogoEditorSettingsModel>(textAsset.text);
            return settings ?? new LogoEditorSettingsModel();
        }
        catch (Exception e)
        {
            ElephantLog.LogError(LogTag, $"Failed to parse LogoEditorSettings: {e.Message}");
            return new LogoEditorSettingsModel();
        }
    }
}