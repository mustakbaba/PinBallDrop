using Exoa.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.TutorialEngine
{
    public class TutorialController : MonoBehaviour
    {
        private enum State { Inactive, Loading, Playing, FadIngOut };
        private static State tutorialState;
        private static bool isSkippable;

        private TutorialSession session;
        private List<TutorialSession.TutorialStep> steps;
        public TutorialPopup popup;

        public Button hiddenBtn;
        public RectTransform hiddenBtnRt;
        public RectTransform mask;
        private int currentStep = -1;
        public float maskScale = 1.2f;
        private Color initBGColor;
        public Image bg;

        public static TutorialController instance;
        private bool retried;

        private float size1Duration = 0.2f;
        public Color tutoBGColor;
        private GameObject currentTarget;
        private Vector2 currentEndPositionValue;
        private float currentEndSizeValue;
        private float currentEndPositionChangeTime;
        private Color currentBgColor;

        private GameObject lastFocusedObject;
        private Transform lastFocusedObjectParent;
        private int lastFocusedObjectSibling;
        public bool debug;

        [Header("ANIMATION")]
        public Springs popupMoveSettings;
        private Vector2Spring popupMoveSpring;
        public Springs maskMoveSettings;
        private Vector2Spring maskMoveSpring;
        public Springs maskSize1Settings;
        public Springs maskSize2Settings;
        private Vector2Spring maskSizeSpring;
        public Springs bgColorSettings;
        private Vector4Spring bgColorSpring;

        public static bool IsSkippable { get { return isSkippable; } set { isSkippable = value; } }
        public static bool IsTutorialActive { get { return tutorialState == State.Playing; } }

        private void OnDestroy()
        {
            TutorialEvents.OnTutorialLoaded -= OnTutorialLoeaded;
        }
        void Awake()
        {
            if (instance != null) throw new Exception("TutorialController alraedy creaeted");

            instance = this;
            popup.gameObject.SetActive(false);
            popup.closeBtn.onClick.AddListener(OnClosePopup);
        }

        private void OnClosePopup()
        {
            HideTutorial();
        }

        void Start()
        {
            initBGColor = bg.color;
            bg.material.color = initBGColor;

            if (TutorialLoader.instance.tutorialLoaded)
                OnTutorialLoeaded();

            TutorialEvents.OnTutorialLoaded += OnTutorialLoeaded;

        }

        private void OnTutorialLoeaded()
        {
            steps = new List<TutorialSession.TutorialStep>();
            if (TutorialLoader.instance != null && TutorialLoader.instance.currentTutorial.tutorial_steps != null)
                steps.AddRange(TutorialLoader.instance.currentTutorial.tutorial_steps);

            tutorialState = State.Playing;
            currentStep = -1;

            if (steps.Count > 0)
            {

                popup.OnClickNext.RemoveAllListeners();
                popup.OnClickNext.AddListener(Next);
                popup.closeBtn.gameObject.SetActive(IsSkippable);
                popup.gameObject.SetActive(true);
                popup.createBackground = false;
                popup.Init();
                popup.Center();
                popup.Open();

                bg.gameObject.SetActive(true);

                mask.gameObject.SetActive(true);
                mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
                mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200);
                mask.anchoredPosition = new Vector2(0, 1000);
                hiddenBtn.gameObject.SetActive(false);

                Next();
            }
            else
            {
                HideTutorial();
            }
        }

        private void Update()
        {
            if (tutorialState == State.Inactive)
            {
                return;
            }
            if (tutorialState == State.FadIngOut)
            {
                Vector2 targetOutSize = new Vector2(7000, 7000);
                mask.sizeDelta = maskSize1Settings.UpdateSpring(ref maskSizeSpring, targetOutSize);
                popup.PopupRt.anchoredPosition = popupMoveSettings.UpdateSpring(ref popupMoveSpring, targetOutSize);
                if (Vector2.Distance(targetOutSize, mask.sizeDelta) < 100)
                {
                    tutorialState = State.Inactive;
                    bg.gameObject.SetActive(false);
                    mask.gameObject.SetActive(false);
                    popup.Hide();
                    TutorialEvents.OnTutorialComplete?.Invoke();

                    return;
                }
            }
            if (tutorialState == State.Playing)
            {
                if (currentTarget != null)
                {
                    Rect targetRect2D = GetObjectCanvasRect(currentTarget);
                    float targetSize = Mathf.Max(targetRect2D.width, targetRect2D.height);

                    if (currentEndSizeValue != targetSize)
                    {
                        currentEndSizeValue = targetSize;
                    }

                    if (currentEndPositionValue != targetRect2D.position)
                    {
                        currentEndPositionValue = targetRect2D.position;
                    }

                    mask.anchoredPosition = maskMoveSettings.UpdateSpring(ref maskMoveSpring, currentEndPositionValue);
                    Vector2 targetMaskSize = Vector2.one * currentEndSizeValue * maskScale;

                    if (currentEndPositionChangeTime > Time.time - size1Duration)
                    {
                        targetMaskSize = new Vector2(.3f, 2f) * currentEndSizeValue * maskScale;
                        mask.sizeDelta = maskSize1Settings.UpdateSpring(ref maskSizeSpring, targetMaskSize);
                    }
                    else
                    {
                        mask.sizeDelta = maskSize2Settings.UpdateSpring(ref maskSizeSpring, targetMaskSize);
                    }
                    Vector2 popupPositon = popup.CalculatePopupPosition(targetRect2D);
                    popup.PopupRt.anchoredPosition = popupMoveSettings.UpdateSpring(ref popupMoveSpring, popupPositon);
                    hiddenBtnRt.anchoredPosition = currentEndPositionValue;
                    hiddenBtnRt.sizeDelta = targetMaskSize;
                }
                else
                {
                    popup.PopupRt.anchoredPosition = popupMoveSettings.UpdateSpring(ref popupMoveSpring, Vector2.zero);
                }
                Vector4 newColorV = bgColorSettings.UpdateSpring(ref bgColorSpring, currentBgColor.ToVector4());
                bg.color = newColorV.ToColor();
            }
        }

        void Next()
        {
            if (debug) print("Next Tutorial Step");
            currentStep++;
            if (steps == null || currentStep >= steps.Count)
            {
                HideTutorial();

                return;
            }
            TutorialSession.TutorialStep s = steps[currentStep];
            popup.SetStep(s);
            currentTarget = null;


            if (s.target_obj != "")
                currentTarget = GameObject.Find(s.target_obj);
            //if (lastFocusedObject != null)
            //{
            //lastFocusedObject.transform.SetParent(lastFocusedObjectParent);
            //lastFocusedObject.transform.SetSiblingIndex(lastFocusedObjectSibling);
            //}
            bool differentObject = currentTarget != lastFocusedObject;

            if (currentTarget != null)
            {
                lastFocusedObject = currentTarget;
                lastFocusedObjectParent = currentTarget.transform.parent;
                lastFocusedObjectSibling = currentTarget.transform.GetSiblingIndex();

                Rect rect2D = GetObjectCanvasRect(currentTarget);

                float size = Mathf.Max(rect2D.width, rect2D.height);
                Vector3 dir = new Vector3(rect2D.position.x, rect2D.position.y, 0) - mask.anchoredPosition3D;

                Debug.DrawRay(mask.position, dir, Color.yellow, 10);
                mask.rotation = Quaternion.LookRotation(mask.forward, dir);

                currentEndPositionValue = rect2D.position;
                currentEndPositionChangeTime = differentObject ? Time.time : Time.time - size1Duration;

                currentEndSizeValue = size;
                RectTransform rt = currentTarget.GetComponent<RectTransform>();
                Button btn = currentTarget.GetComponent<Button>();
                hiddenBtn.onClick.RemoveAllListeners();

                if (s.isClickable && rt != null && btn != null)
                {
                    hiddenBtn.gameObject.SetActive(true);
                    hiddenBtn.onClick.AddListener(btn.onClick.Invoke);
                }
                else
                {
                    hiddenBtn.gameObject.SetActive(false);
                }

                if (s.isReplacingNextButton && rt != null && btn != null)
                {
                    popup.nextBtn.gameObject.SetActive(false);


                    hiddenBtn.onClick.AddListener(popup.nextBtn.onClick.Invoke);
                }
                else
                {
                    popup.nextBtn.gameObject.SetActive(true);
                }
                TutorialEvents.OnTutorialFocus?.Invoke(s.target_obj, rect2D.center);
                TutorialEvents.OnTutorialProgress?.Invoke(currentStep, steps.Count);
            }
            else if (!retried && !string.IsNullOrEmpty(s.target_obj))
            {
                retried = true;
                if (debug) Debug.Log("RETRYING CANNOT FIND " + s.target_obj);
                // Retry in .2f seconds
                currentStep--;
                Invoke("Next", .2f);
            }
            else
            {
                if (!string.IsNullOrEmpty(s.target_obj) && debug)
                    Debug.Log("CANNOT FIND " + s.target_obj);

                currentEndPositionValue = Vector2.zero;
                currentEndPositionChangeTime = Time.time;
                popup.nextBtn.gameObject.SetActive(true);
                hiddenBtn.gameObject.SetActive(false);
                mask.sizeDelta = (Vector2.zero);
                mask.anchoredPosition = (Vector2.zero);
                TutorialEvents.OnTutorialProgress?.Invoke(currentStep, steps.Count);
            }
            currentBgColor = currentStep == 0 ? initBGColor : tutoBGColor;

        }

        private void HideTutorial()
        {

            popup.OnClickNext.RemoveAllListeners();

            tutorialState = State.FadIngOut;

        }


        /**
        * Get Object's bounds in the Canvas space
        **/
        private Rect GetObjectCanvasRect(GameObject obj)
        {
            RectTransform objRect = obj.GetComponent<RectTransform>();
            Renderer objRenderer = obj.GetComponent<Renderer>();
            Collider objCollider = obj.GetComponentInChildren<Collider>();

            Rect newRect = new Rect();
            if (objRect != null)
                newRect = objRect.GetRectFromOtherParent(mask.parent as RectTransform);

            else if (objRenderer != null)
            {
                newRect = objRenderer.GetScreenRect(Camera.main);
                newRect = (mask.parent as RectTransform).ScreenRectToRectTransform(newRect);
            }
            else if (objCollider != null)
            {
                newRect = objCollider.GetScreenRect(Camera.main);
                newRect = (mask.parent as RectTransform).ScreenRectToRectTransform(newRect);

            }
            return newRect;
        }
    }
}
