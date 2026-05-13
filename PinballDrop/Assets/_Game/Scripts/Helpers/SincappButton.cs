using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEngine.UI;

public class SincappButton : Button
{
    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.AddListener(OnCustomClick);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onClick.RemoveListener(OnCustomClick);
    }
    
    private void OnCustomClick()
    {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
    }
}