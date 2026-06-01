using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElephantSDK
{
    [DisallowMultipleComponent]
    internal sealed class TMPLinkClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Camera uiCamera;
        public Action<string> OnLinkClicked;
        
        public void Initialize(TMP_Text targetText, Camera camera, Action<string> onLinkClicked)
        {
            text = targetText;
            uiCamera = camera;
            OnLinkClicked = onLinkClicked;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (text == null)
            {
                return;
            }

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, uiCamera);
            if (linkIndex == -1)
            {
                return;
            }

            var linkInfo = text.textInfo.linkInfo[linkIndex];
            var url = linkInfo.GetLinkID();
            if (!string.IsNullOrEmpty(url))
            {
                OnLinkClicked?.Invoke(url);
            }
        }
    }
}
