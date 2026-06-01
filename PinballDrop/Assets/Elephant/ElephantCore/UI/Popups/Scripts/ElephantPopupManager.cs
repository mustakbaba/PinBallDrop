using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace ElephantSDK
{
    public class ElephantPopupManager : MonoBehaviour
    {
        private static ElephantPopupManager _instance;

        public static ElephantPopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject managerObj = new GameObject("ElephantPopupManager");
                    _instance = managerObj.AddComponent<ElephantPopupManager>();
                    DontDestroyOnLoad(managerObj);
                }

                return _instance;
            }
        }

        private Canvas _popupCanvas;
        private GameObject _currentPopup;

        [Range(0.8f, 0.98f)] [Tooltip("Maximum screen height the popup can occupy (0.9 = 90% of screen)")]
        public float maxScreenHeightRatio = 0.9f;

        public T ShowPopup<T>(string prefabPath) where T : MonoBehaviour
        {
            CloseCurrentPopup();

            Canvas canvas = GetOrCreateCanvas();

            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                ElephantLog.LogError("ElephantPopupManager", $"Failed to load popup prefab: {prefabPath}");
                ElephantLog.LogError("ElephantPopupManager", $"Make sure prefab exists at: Resources/{prefabPath}.prefab");
                ElephantLog.LogError("ElephantPopupManager", "Canvas NOT activated - game flow not blocked");
                return null;
            }

            GameObject popupObj = null;
            T popup = null;

            try
            {
                popupObj = Instantiate(prefab, canvas.transform);
                popupObj.SetActive(true);

                popup = popupObj.GetComponent<T>();
                if (popup == null)
                {
                    ElephantLog.LogError("ElephantPopupManager", $"Prefab missing component {typeof(T).Name}");
                    ElephantLog.LogError("ElephantPopupManager", "Canvas NOT activated - game flow not blocked");
                    Destroy(popupObj);
                    return null;
                }

                _currentPopup = popupObj;
                canvas.gameObject.SetActive(true);

                StartCoroutine(ConstrainPopupAfterLayout(popupObj));

                ElephantLog.Log("ElephantPopupManager", $"✓ Popup shown: {prefabPath}");
                return popup;
            }
            catch (System.Exception e)
            {
                ElephantLog.LogError("ElephantPopupManager", $"Exception creating popup: {e.Message}");
                ElephantLog.LogError("ElephantPopupManager", $"Stack trace: {e.StackTrace}");
                ElephantLog.LogError("ElephantPopupManager", "Canvas NOT activated - game flow not blocked");

                if (popupObj != null)
                {
                    Destroy(popupObj);
                }

                canvas.gameObject.SetActive(false);

                return null;
            }
        }

        private IEnumerator ConstrainPopupAfterLayout(GameObject popupObj)
        {
            yield return new WaitForEndOfFrame();

            if (popupObj == null)
            {
                ElephantLog.Log("ElephantPopupManager", "Popup was destroyed before constraints could be applied");
                yield break;
            }

            Canvas.ForceUpdateCanvases();

            ConstrainPopupToScreenHeight(popupObj);
        }

        private void ConstrainPopupToScreenHeight(GameObject popupObj)
        {
            // Additional safety check
            if (popupObj == null)
            {
                ElephantLog.Log("ElephantPopupManager", "Popup is null in ConstrainPopupToScreenHeight");
                return;
            }

            RectTransform popupRect = popupObj.GetComponent<RectTransform>();
            if (popupRect == null)
            {
                ElephantLog.Log("ElephantPopupManager", "Popup has no RectTransform");
                return;
            }

            RectTransform canvasRect = _popupCanvas.GetComponent<RectTransform>();
            float canvasHeight = canvasRect.rect.height;
            float maxAllowedHeight = canvasHeight * maxScreenHeightRatio;

            float popupHeight = popupRect.rect.height;

            ElephantLog.Log("ElephantPopupManager", $"Popup height: {popupHeight}, Max allowed: {maxAllowedHeight}, Canvas height: {canvasHeight}");

            if (popupHeight > maxAllowedHeight)
            {
                ElephantLog.Log("ElephantPopupManager", "Popup exceeds max height - constraining");

                ContentSizeFitter csf = popupObj.GetComponent<ContentSizeFitter>();
                if (csf != null && csf.verticalFit != ContentSizeFitter.FitMode.Unconstrained)
                {
                    ElephantLog.Log("ElephantPopupManager", "Disabling ContentSizeFitter vertical fit");
                    csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }

                popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxAllowedHeight);
            }
        }

        public void CloseCurrentPopup()
        {
            if (_currentPopup != null)
            {
                ElephantLog.Log("ElephantPopupManager", "Closing popup");
                Destroy(_currentPopup);
                _currentPopup = null;
            }

            if (_popupCanvas != null)
            {
                _popupCanvas.gameObject.SetActive(false);
            }
        }

        private Canvas GetOrCreateCanvas()
        {
            if (_popupCanvas != null) return _popupCanvas;
            
            if (EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                ElephantLog.Log("ElephantPopupManager", "Created EventSystem");
            }

            GameObject canvasObj = new GameObject("ElephantPopupCanvas");
            canvasObj.transform.SetParent(transform);
            DontDestroyOnLoad(canvasObj);

            _popupCanvas = canvasObj.AddComponent<Canvas>();
            _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _popupCanvas.sortingOrder = 2001;
            _popupCanvas.pixelPerfect = true;

            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170, 2532);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            CreateBlockingBackground();

            canvasObj.SetActive(false);

            ElephantLog.Log("ElephantPopupManager", "Created popup canvas");
            return _popupCanvas;
        }

        private void CreateBlockingBackground()
        {
            GameObject bgObj = new GameObject("BlockingBackground");
            bgObj.transform.SetParent(_popupCanvas.transform, false);

            Image img = bgObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);
            img.raycastTarget = true;

            RectTransform rect = bgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            ElephantLog.Log("ElephantPopupManager", "Created blocking background");
        }
    }
}