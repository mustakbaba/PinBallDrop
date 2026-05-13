using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElephantSDK
{
    public class OfferUIManager : MonoBehaviour
    {
        private Canvas _dynamicCanvas;
        private GameObject _uiContainer;
        private static OfferAssetManager _offerAssetManager;
        private string _iapNames;
        private List<PurchaseOption> _currentPurchaseOptions = new List<PurchaseOption>();
        private List<Button> _currentPurchaseButtons = new List<Button>();

        private GameObject _loadingPanel;

        private void OnEnable()
        {
            ElephantCore.onOfferUIFetched += DisplayOfferUI;
            Elephant.OnDismissOfferUI += DismissOfferUI;
            Elephant.OnOfferClosed += EnablePurchaseButtons;
        }

        private void OnDisable()
        {
            ElephantCore.onOfferUIFetched -= DisplayOfferUI;
            Elephant.OnDismissOfferUI -= DismissOfferUI;
            Elephant.OnOfferClosed -= EnablePurchaseButtons;
        }

        private void DisplayOfferUI()
        {
            if (_dynamicCanvas != null)
            {
                ElephantLog.Log("OFFERUI", "There is already a shown offer!");
                return;
            }
            try
            {
                _iapNames = "";
                _currentPurchaseOptions.Clear();
                _currentPurchaseButtons.Clear();
                _offerAssetManager = OfferAssetManager.GetInstance();

                CreateCanvas();
                CreateBlockingPanel();
                CreateComponents();


                Elephant.TriggerOfferShown(_currentPurchaseOptions);
                _offerAssetManager.iapNames = _iapNames;
                Elephant.OfferShownEvent(_offerAssetManager.currentOffer.Segment, _iapNames, _offerAssetManager.currentOffer.OfferName, _offerAssetManager.offerMetaData.triggerPoint);
            }
            catch (Exception e)
            {
                var triggerPoint = _offerAssetManager.offerMetaData.triggerPoint;
                ElephantLog.Log("OFFERUI", "UI Couldn't be shown in trigger point: " +  triggerPoint + " Reason: " + e);
                var param = Params.New().Set("error", e.ToString());
                param.Set("trigger_point", triggerPoint);
                Elephant.Event("offerui_not_shown", -1, param);
                if (_dynamicCanvas != null)
                    Destroy(_dynamicCanvas.gameObject);
            }
        }

        private void EnablePurchaseButtons()
        {
            foreach (var purchaseButton in _currentPurchaseButtons)
            {
                purchaseButton.interactable = true;
            }
        }

        private void DismissOfferUI()
        {
            if (_dynamicCanvas != null)
            {
                Elephant.OfferClosedEvent(_offerAssetManager.currentOffer.Segment, _iapNames, _offerAssetManager.currentOffer.OfferName, _offerAssetManager.offerMetaData.triggerPoint);

                Destroy(_dynamicCanvas.gameObject);
            }
            else
            {
                ElephantLog.LogError("OFFERUI", "No canvas to destroy.");
            }
        }

        private void CreateBlockingPanel()
        {
            GameObject blockingPanel = new GameObject("BlockingPanel");
            blockingPanel.transform.SetParent(_dynamicCanvas.transform, false);
            blockingPanel.transform.SetAsFirstSibling(); // Ensure it's the first child, i.e., behind everything else

            Image img = blockingPanel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // Transparent color

            RectTransform rectTransform = blockingPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;        // Stretched to full canvas
            rectTransform.anchorMax = Vector2.one;         // Stretched to full canvas
            rectTransform.sizeDelta = Vector2.zero;        // No additional size
            rectTransform.anchoredPosition = Vector2.zero; // No offset
        }

        private void IsFullWide()
        {
            var leftmost = float.MaxValue;
            var rightmost = float.MinValue;

            foreach (var component in _offerAssetManager.offerUIData.components)
            {
                var left = component.x;
                var right = component.x + component.width;

                if (left < leftmost) leftmost = left;
                if (right > rightmost) rightmost = right;
            }
            var isFullWide = Mathf.Approximately(leftmost, 0) && Mathf.Approximately(rightmost, _offerAssetManager.offerUIData.container.width);

            if (isFullWide)
            {
                _dynamicCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                var scaleFactorX = Screen.width / _uiContainer.GetComponent<RectTransform>().sizeDelta.x;
                var scaleFactorY = scaleFactorX;

                if (scaleFactorX * _uiContainer.GetComponent<RectTransform>().sizeDelta.y > Screen.height)
                {
                    scaleFactorY = Screen.height / _uiContainer.GetComponent<RectTransform>().sizeDelta.y;
                }

                _uiContainer.GetComponent<RectTransform>().localScale = new Vector3(scaleFactorX, scaleFactorY, 1);
            }
        }

        private void CreateCanvas()
        {
            _dynamicCanvas = new GameObject("DynamicCanvas").AddComponent<Canvas>();
            _dynamicCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _dynamicCanvas.sortingOrder = 1000;
            _dynamicCanvas.gameObject.AddComponent<GraphicRaycaster>();
            _dynamicCanvas.pixelPerfect = true;


            CanvasScaler scaler = _dynamicCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(_offerAssetManager.offerUIData.container.width, _offerAssetManager.offerUIData.container.height);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;

            _uiContainer = new GameObject("UIContainer");
            _uiContainer.transform.SetParent(_dynamicCanvas.transform, false);
            RectTransform containerRect = _uiContainer.AddComponent<RectTransform>();
            SetContainerRectTransform(containerRect, _offerAssetManager.offerUIData.container);
        }

        private void CreateComponents()
        {
            foreach (var component in _offerAssetManager.offerUIData.components)
            {
                switch (component.type)
                {
                    case "image":
                        CreateImage(component);
                        break;
                    case "button":
                        CreateButton(component);
                        break;
                    case "text":
                        CreateText(component);
                        break;
                    case "animation":
                        break;
                }
            }
        }

        private void CreateImage(ComponentData component)
        {
            var imgObject = new GameObject("DynamicImage");
            imgObject.transform.SetParent(_uiContainer.transform, false);
            var rawImage = imgObject.AddComponent<RawImage>();
            SetImage(component.url, rawImage, imgObject, component);

            if (component.name.Contains("background"))
            {
                IsFullWide();
            }
        }

        private void SetImage(string url, RawImage rawImage, GameObject imgObject, ComponentData component)
        {
            var filename = Utils.GetFileNameFromUrl(url);
            var fileData = File.ReadAllBytes(Path.Combine(Utils.GetSubdirectoryPath(), filename));
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            rawImage.texture = texture;
            var referenceResolution = new Vector2(_offerAssetManager.offerUIData.container.width, _offerAssetManager.offerUIData.container.height);

            imgObject.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
            imgObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                (component.x + component.width / 2) - referenceResolution.x / 2,
                (referenceResolution.y / 2) - (component.y + component.height / 2));
        }


        private void CreateButton(ComponentData component)
        {
            GameObject btnObject = new GameObject("DynamicButton");
            btnObject.transform.SetParent(_uiContainer.transform, false);
            Button button = btnObject.AddComponent<Button>();
            Image img = btnObject.AddComponent<Image>();
            button.targetGraphic = img;
            SetButtonImage(component.url, img);
            SetRectTransform(btnObject.GetComponent<RectTransform>(), component);

            switch (component.action)
            {
                case "purchase":
                    button.onClick.AddListener(delegate
                    {
                        PurchaseAction(component.name);
                    });
                    _currentPurchaseButtons.Add(button);
                    break;
                case "close":
                    button.onClick.AddListener(CloseAction);
                    break;
            }
        }

        private void SetButtonImage(string url, Image img)
        {
            var filename = Utils.GetFileNameFromUrl(url);
            var fileData = File.ReadAllBytes(Path.Combine(Utils.GetSubdirectoryPath(), filename));
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            }

        private void CreateText(ComponentData component)
        {
            var textObject = new GameObject("DynamicText");
            textObject.transform.SetParent(_uiContainer.transform, false);

            var uiText = textObject.AddComponent<Text>();

            var customFont = Resources.Load<Font>(component.font_post_script_name);

            if (customFont == null)
            {
                customFont = Resources.Load<Font>("BALOO2 ELEPHANT");
            }

            uiText.font = customFont;
            uiText.fontSize = component.font_size;

            if (_offerAssetManager.templateFieldsCache.TryGetValue((_offerAssetManager.currentOffer.OfferName + "_" + component.name), out var value))
            {
                if (component.name.Contains("@price"))
                {
                    if (_iapNames == "")
                    {
                        _iapNames += value;
                    }
                    else
                    {
                        _iapNames = _iapNames + ", " + value;
                    }

                    var purchaseOption = _offerAssetManager.purchaseOptions.FirstOrDefault(po => po.name == value);

                    if (purchaseOption != null)
                    {
                        _currentPurchaseOptions.Add(purchaseOption);
                        switch (purchaseOption.typeEnum)
                        {
                            case PurchaseType.soft_currency:
                                if(purchaseOption.CurrencyCost.HasValue)
                                    component.characters = purchaseOption.CurrencyCost.ToString();
                                else
                                    throw new OperationCanceledException(purchaseOption.name + " has no cost.");
                                break;
                            case PurchaseType.hard_currency:
                                if(purchaseOption.CurrencyCost.HasValue)
                                    component.characters = purchaseOption.CurrencyCost.ToString();
                                else
                                    throw new OperationCanceledException(purchaseOption.name + " has no cost.");
                                break;
                            case PurchaseType.rewarded:
                                if(purchaseOption.CurrencyCost.HasValue)
                                    component.characters = purchaseOption.RewardedAdCount.ToString();
                                else
                                    throw new OperationCanceledException(purchaseOption.name + " has no RewardedAdCount.");
                                break;
                            case PurchaseType.iap:
                                if (_offerAssetManager.localPricingCache.TryGetValue(purchaseOption.StoreProductId, out var priceDetails))
                                    component.characters = FormatPriceString(priceDetails, customFont);
                                else
#if UNITY_EDITOR
                                    component.characters = FormatPriceString(component.characters, customFont);
#else
                                    throw new OperationCanceledException(purchaseOption.name + " has no local pricing.");
#endif
                                break;
                            default:
                                throw new OperationCanceledException("Purchase option type is undefined: " + purchaseOption.typeEnum);
                        }
                    }
                }
                else if (value != "")
                    component.characters = value;
            }
            #if !UNITY_EDITOR
            else
            {
                throw new OperationCanceledException("Dynamic field is missing: " + component.name);
            }
#endif

            uiText.text = component.characters.Replace("\\n", "\n");



            uiText.color = new Color(component.fill.r, component.fill.g, component.fill.b, component.fill.a);
            uiText.alignment = TextAlignmentMapper(component);
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            uiText.raycastTarget = false;

            var outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(component.stroke.r, component.stroke.g, component.stroke.b, component.stroke.a);
            outline.effectDistance = new Vector2(component.stroke_weight, component.stroke_weight);


            var boundingBoxPosition = new Vector2(component.x, component.y);
            var boundingBoxSize = new Vector2(component.width, component.height);
            var originalSize = new Vector2(component.real_w, component.real_h);
            var realCoordinates = GetOriginalTopLeft(boundingBoxPosition, boundingBoxSize, originalSize);

            component.width = component.real_w;
            component.height = component.real_h;
            component.x = realCoordinates.x;
            component.y = realCoordinates.y;

            var textRect = textObject.GetComponent<RectTransform>();
            SetRectTransform(textRect, component);

            AdjustFontSizeToFit(uiText, textRect);

            if (!(Math.Abs(component.rotation) > 0.001f))
                return;
            var rotationInDegrees = -component.rotation * Mathf.Rad2Deg;
            textObject.transform.rotation = Quaternion.Euler(0, 0, rotationInDegrees);
        }
        
        private void AdjustFontSizeToFit(Text uiText, RectTransform textRect)
        {
            Canvas.ForceUpdateCanvases(); // Update the canvas immediately so we get up-to-date measurements

            while (uiText.preferredWidth > textRect.rect.width || uiText.preferredHeight > textRect.rect.height)
            {
                uiText.fontSize--;               // Decrease the font size by 1
                Canvas.ForceUpdateCanvases();    // Update canvas again to re-measure with the new font size
                if (uiText.fontSize <= 0) break; // Prevent the font size from going below 0
            }
        }

        private string FormatPriceString(string priceDetails, Font font)
        {
#if UNITY_EDITOR
            return !HasAllCharacters(font, priceDetails) ? "no_font" : priceDetails;
#else

            var parts = priceDetails.Split(' ');

            if (parts.Length != 3)
            {
                throw new OperationCanceledException($"Unexpected priceDetails format: {priceDetails}");
            }

            var formattedPrice = parts[0];
            var numericPrice = parts[1];
            var currencyCode = parts[2];

            return !HasAllCharacters(font, formattedPrice) ? $"{numericPrice} {currencyCode}" : formattedPrice;
#endif
        }

        private bool HasAllCharacters(Font font, string str)
        {
            foreach (char c in str)
            {
                if (!font.HasCharacter(c))
                {
                    return false;
                }
            }
            return true;
        }


        private TextAnchor TextAlignmentMapper(ComponentData component)
        {
            switch (component.text_align_horizontal)
            {
                case "LEFT":
                    switch (component.text_align_vertical)
                    {
                        case "TOP": return TextAnchor.UpperLeft;
                        case "CENTER": return TextAnchor.MiddleLeft;
                        case "BOTTOM": return TextAnchor.LowerLeft;
                        default: return TextAnchor.UpperLeft;
                    }

                case "CENTER":
                    switch (component.text_align_vertical)
                    {
                        case "TOP": return TextAnchor.UpperCenter;
                        case "CENTER": return TextAnchor.MiddleCenter;
                        case "BOTTOM": return TextAnchor.LowerCenter;
                        default: return TextAnchor.UpperCenter;
                    }

                case "RIGHT":
                    switch (component.text_align_vertical)
                    {
                        case "TOP": return TextAnchor.UpperRight;
                        case "CENTER": return TextAnchor.MiddleRight;
                        case "BOTTOM": return TextAnchor.LowerRight;
                        default: return TextAnchor.UpperRight;
                    }

                case "JUSTIFY":
                    // We don't support it
                    switch (component.text_align_vertical)
                    {
                        case "TOP": return TextAnchor.UpperLeft;
                        case "CENTER": return TextAnchor.MiddleLeft;
                        case "BOTTOM": return TextAnchor.LowerLeft;
                        default: return TextAnchor.UpperLeft;
                    }

                default:
                    return TextAnchor.UpperLeft;
            }
        }

        private void SetRectTransform(RectTransform rectTransform, ComponentData component)
        {
            Vector2 referenceResolution = new Vector2(_offerAssetManager.offerUIData.container.width, _offerAssetManager.offerUIData.container.height);

            rectTransform.sizeDelta = new Vector2(component.width, component.height);
            rectTransform.anchoredPosition = new Vector2(
                (component.x + component.width / 2) - referenceResolution.x / 2,
                (referenceResolution.y / 2) - (component.y + component.height / 2)
            );
        }

        private void SetContainerRectTransform(RectTransform rectTransform, ContainerData container)
        {
            Vector2 referenceResolution = new Vector2(container.width, container.height);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = referenceResolution;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        private void PurchaseAction(string componentName)
        {
            foreach (var button in _currentPurchaseButtons)
            {
                button.interactable = false;
            }
            if (!_offerAssetManager.templateFieldsCache.TryGetValue((_offerAssetManager.currentOffer.OfferName + "_" + componentName), out var value))
                return;
            var purchaseOption = _offerAssetManager.purchaseOptions.FirstOrDefault(po => po.name == value);
            if (purchaseOption != null)
            {
                Elephant.TriggerOfferPurchaseRequested(purchaseOption);
            }
        }

        private void CloseAction()
        {
            Elephant.TriggerOfferDismissed(_currentPurchaseOptions);
            DismissOfferUI();
        }

        private static Vector2 GetOriginalTopLeft(Vector2 boundingBoxPosition, Vector2 boundingBoxSize, Vector2 originalSize)
        {
            Vector2 boundingBoxCenter = boundingBoxPosition + boundingBoxSize * 0.5f;
            Vector2 originalTopLeft = boundingBoxCenter - originalSize * 0.5f;

            return originalTopLeft;
        }
    }
}