using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElephantSDK
{
    public abstract class BasePopup : MonoBehaviour
    {
        [Header("Common UI References")]
        [SerializeField] protected Image gameImage;
        [SerializeField] protected Image popupBackgroundImage;
        [SerializeField] protected Image titleBackgroundImage;
        [SerializeField] protected TextMeshProUGUI[] titleTexts;
        [SerializeField] protected Image[] dividers;
        [SerializeField] protected Image buttonAreaImage;
        [SerializeField] protected Image buttonImage;

        protected virtual void ApplyStyle(PopupType popupType)
        {
            PopupStyleApplier.ApplyToBasePopup(
                popupType,
                popupBackgroundImage,
                buttonAreaImage,
                buttonImage,
                titleBackgroundImage,
                titleTexts,
                dividers);
            PopupGameImageLoader.SetupGameImage(popupType, gameImage);
        }
    }
}
