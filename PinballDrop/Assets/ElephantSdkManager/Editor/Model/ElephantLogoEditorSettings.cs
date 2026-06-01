using System;
using UnityEngine;

namespace ElephantSdkManager.Editor.Model
{
    /// <summary>
    /// Serializable data model for the Elephant Logo Editor.
    /// Stores whether the Rollic branding is enabled and the custom background color when disabled.
    /// </summary>
    [Serializable]
    public class ElephantLogoEditorSettings
    {
        public bool isRollicLogoEnabled;
        public Color backgroundColor;
    }
}
