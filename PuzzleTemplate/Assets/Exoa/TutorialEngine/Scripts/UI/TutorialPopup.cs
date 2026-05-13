using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialPopup : Popup
{

    public AnimatedButton nextBtn;
    public Button closeBtn;
    public Text contentText;
    public UnityEvent OnClickNext;
    public Transform bg;
    private RectTransform popupRt;

    public RectTransform PopupRt { get => popupRt; set => popupRt = value; }

    public void Init()
    {
        popupRt = GetComponent<RectTransform>();

        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(OnClickNextHandler);
    }

    private void OnClickNextHandler()
    {
        OnClickNext.Invoke();
    }

    public void SetStep(TutorialSession.TutorialStep s)
    {
        contentText.text = s.text;
        UpdateHGroup();
    }

    public Vector2 CalculatePopupPosition(Rect newRect)
    {
        Vector2 maskPosition = newRect.position;
        float maskDistanceFromCenter = maskPosition.magnitude;

        float maskDiagonal = Mathf.Max(newRect.width, newRect.height);


        float popupDiagonal = Mathf.Sqrt(Mathf.Pow(popupRt.rect.width, 2) + Mathf.Pow(popupRt.rect.height, 2));
        Vector2 popupPostion = (maskPosition.magnitude - (maskDiagonal * .5f) - (popupDiagonal * .5f)) * maskPosition.normalized;

        RectTransform parent = popupRt.parent as RectTransform;
        float maxX = parent.rect.width * .5f - popupRt.rect.width * .5f;
        float maxY = parent.rect.height * .5f - popupRt.rect.height * .5f;

        popupPostion.x = Mathf.Clamp(popupPostion.x, -maxX, maxX);
        popupPostion.y = Mathf.Clamp(popupPostion.y, -maxY, maxY);

        return popupPostion;
    }

    private void UpdateHGroup()
    {
        ContentSizeFitter csf = bg.GetComponent<ContentSizeFitter>();
        csf.enabled = false;
        csf.SetLayoutVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)bg);
        csf.enabled = true;
    }
}
