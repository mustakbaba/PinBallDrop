using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElephantSDK
{
    public static class PopupStyleApplier
    {
        private static readonly HashSet<int> TitleOutlineLinearNormalizedIds = new();

        public static bool TryParseHexColor(string hex, out Color color, bool convertForLinearProject = true)
        {
            color = default;
            if (ParseHexColor(hex, convertForLinearProject) is Color parsed)
            {
                color = parsed;
                return true;
            }

            return false;
        }

        public static void ApplyToBasePopup(
            PopupType popupType,
            Image popupBackgroundImage,
            Image buttonContainerImage,
            Image buttonImage,
            Image titleBackgroundImage,
            TMP_Text[] titleTexts,
            Image[] dividers)
        {
            var style = PopupUIStyleConfig.GetStyle(popupType);
            if (style == null)
            {
                ApplyTitleOutlineLinearCorrectionOnly(titleTexts);
                return;
            }

            ApplyColor(popupBackgroundImage, style.background);
            ApplyColor(titleBackgroundImage, style.titleBackground);
            ApplyColor(buttonContainerImage, style.buttonContainer);
            ApplyColor(buttonImage, style.buttonPrimary);
            ApplyTitleStyle(titleTexts, style.title, style.titleOutline, style.titleOutlineLinearCorrected);
            ApplyColor(dividers, style.divider);
        }

        public static void ApplyToInGameSettings(Image panelBackground, TMP_Text[] titleTexts)
        {
            var style = PopupUIStyleConfig.GetStyle(PopupType.InGameSettings);
            if (style == null)
            {
                ApplyTitleOutlineLinearCorrectionOnly(titleTexts);
                return;
            }

            ApplyColor(panelBackground, style.layout?.panel);
            ApplyTitleStyle(titleTexts, style.title, style.titleOutline, style.titleOutlineLinearCorrected);
        }

        public static void ApplyBackground(PopupType popupType, Image popupBackgroundImage)
        {
            var style = PopupUIStyleConfig.GetStyle(popupType);
            if (style == null)
            {
                return;
            }

            ApplyColor(popupBackgroundImage, style.background);
        }

        private static void ApplyColor(Image image, string colorHex)
        {
            if (image != null && ParseHexColor(colorHex) is Color color)
            {
                image.color = color;
            }
        }

        private static void ApplyColor(Image[] images, string colorHex)
        {
            if (images == null || ParseHexColor(colorHex) is not Color color)
            {
                return;
            }

            foreach (var image in images)
            {
                if (image != null)
                {
                    image.color = color;
                }
            }
        }

        private static void ApplyTitleStyle(TMP_Text[] titleTexts, string colorHex, string outlineHex, bool? titleOutlineLinearCorrected)
        {
            if (titleTexts == null)
            {
                return;
            }

            Color? titleColor = ParseHexColor(colorHex);
            Color? outlineColor = ParseHexColor(outlineHex, ShouldConvertOutlineForLinearProject(titleOutlineLinearCorrected));
            bool hasOutlineOverride = outlineColor is Color;
            bool shouldNormalizeDefaultOutline = !hasOutlineOverride && string.IsNullOrWhiteSpace(outlineHex) && QualitySettings.activeColorSpace == ColorSpace.Linear;

            foreach (var txt in titleTexts)
            {
                if (txt == null)
                {
                    continue;
                }

                if (titleColor is Color tc)
                {
                    txt.color = tc;
                }

                if (outlineColor is Color oc)
                {
                    txt.outlineColor = oc;
                    txt.ForceMeshUpdate();
                }
                else if (shouldNormalizeDefaultOutline)
                {
                    int id = txt.GetInstanceID();
                    if (TitleOutlineLinearNormalizedIds.Add(id))
                    {
                        Color current = txt.outlineColor;
                        Color linear = current.linear;
                        linear.a = current.a;
                        txt.outlineColor = linear;
                        txt.ForceMeshUpdate();
                    }
                }
            }
        }

        private static void ApplyTitleOutlineLinearCorrectionOnly(TMP_Text[] titleTexts)
        {
            if (titleTexts == null || QualitySettings.activeColorSpace != ColorSpace.Linear)
            {
                return;
            }

            foreach (var txt in titleTexts)
            {
                if (txt == null)
                {
                    continue;
                }

                int id = txt.GetInstanceID();
                if (TitleOutlineLinearNormalizedIds.Add(id))
                {
                    Color current = txt.outlineColor;
                    Color linear = current.linear;
                    linear.a = current.a;
                    txt.outlineColor = linear;
                    txt.ForceMeshUpdate();
                }
            }
        }

        private static bool ShouldConvertOutlineForLinearProject(bool? titleOutlineLinearCorrected)
        {
            return titleOutlineLinearCorrected ?? true;
        }

        private static Color? ParseHexColor(string hex, bool convertForLinearProject = false)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return null;
            }

            hex = hex.Trim();
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length != 6 && hex.Length != 8)
            {
                return null;
            }

            try
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                int a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) : 255;
                Color parsedColor = new Color(r / 255f, g / 255f, b / 255f, a / 255f);

                if (convertForLinearProject && QualitySettings.activeColorSpace == ColorSpace.Linear)
                {
                    Color linearColor = parsedColor.linear;
                    linearColor.a = parsedColor.a;
                    return linearColor;
                }

                return parsedColor;
            }
            catch (Exception e)
            {
                ElephantLog.LogError("PopupStyleApplier", $"Invalid color hex '{hex}': {e.Message}");
                return null;
            }
        }
    }
}
