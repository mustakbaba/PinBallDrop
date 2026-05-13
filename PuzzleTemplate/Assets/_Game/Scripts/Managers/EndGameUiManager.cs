using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SincappStudio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EndGameUiManager :  MonoSingleton<EndGameUiManager>,IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button _winButton, _loseButton;
    private Animator _animator;
    [SerializeField] private Transform _emojiHolder;
    [SerializeField] private Transform _failEmojiHolder;
    [SerializeField] private GameObject particleHolder;
    [SerializeField] private Slider _blockerSlider;
    [SerializeField] private Image _blockerImage;
    [SerializeField] private TextMeshProUGUI _fillText;
    private bool _isFailed;
    private Sprite[] _blockerSprites;
    [SerializeField] private CanvasGroup _failPanelCanvasGroup;
    Tween fadeTween;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _blockerSprites = Resources.LoadAll<Sprite>("BlockerSprites");
        _blockerSlider.value = PersistData.Instance.CurrentBlockerFillAmount;
        SetFillText(PersistData.Instance.CurrentBlockerFillAmount);

        if (PersistData.Instance.CurrentBlockerIndex >= _blockerSprites.Length)
        {
            _blockerImage.transform.parent.gameObject.SetActive(false);
            _blockerSlider.gameObject.SetActive(false);
        }
        else
        {
            _blockerImage.sprite = _blockerSprites[PersistData.Instance.CurrentBlockerIndex];
        }
    }

    private void OnEnable()
    {
        EventManager.OnGameWin += ShowWinScreen;
        EventManager.OnGameLose += ShowLoseScreen;
        _winButton.onClick.AddListener(OnWinButtonClicked);
        _loseButton.onClick.AddListener(OnLoseButtonClicked);
    }

    private void OnDisable()
    {
        EventManager.OnGameWin -= ShowWinScreen;
        EventManager.OnGameLose -= ShowLoseScreen;
        _winButton.onClick.RemoveAllListeners();
        _loseButton.onClick.RemoveAllListeners();
    }

    private void ShowWinScreen()
    {
        if (_isFailed) return;

        _isFailed = true;
        Time.timeScale = 1;

        StartCoroutine(Sincapp.WaitAndAction(.5f, () =>
        {
            _animator.SetTrigger("Win");
            particleHolder.gameObject.SetActive(true);
            var randIndex = Random.Range(0, _emojiHolder.childCount - 1);
            _emojiHolder.GetChild(randIndex).gameObject.SetActive(true);

            LoadNextBlocker();
        }));
    }


    public void LoadNextBlocker()
    {
        if (PersistData.Instance.CurrentBlockerIndex < _blockerSprites.Length)
        {
            PersistData.Instance.CurrentBlockerFillAmount += LevelManager.Instance.FillAddAmountEachLevel;

            var targetValue = PersistData.Instance.CurrentBlockerFillAmount >= 1
                ? 1
                : PersistData.Instance.CurrentBlockerFillAmount;

            if (PersistData.Instance.CurrentBlockerFillAmount >= 1)
            {
                PersistData.Instance.CurrentBlockerFillAmount = 0;
                PersistData.Instance.CurrentBlockerIndex++;
                PersistData.Instance.RecentlyBlockerReached = true;
            }

            StartCoroutine(Sincapp.WaitAndAction(0.25f,
                () =>
                {
                    _blockerSlider.DOValue(targetValue, .25f).OnUpdate(() => { SetFillText(_blockerSlider.value); });
                }));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPointerOverButton(eventData))
            return;

        FadeTo(0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        FadeTo(1f);
    }

    void FadeTo(float targetAlpha)
    {
        fadeTween?.Kill();

        fadeTween = _failPanelCanvasGroup
            .DOFade(targetAlpha, .3f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); // timescale bağımsız
    }

    bool IsPointerOverButton(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<Button>() != null)
                return true;
        }

        return false;
    }

    private void ShowLoseScreen()
    {
        if (_isFailed) return;

        Time.timeScale = 1;

        _isFailed = true;

        _animator.SetTrigger("Lose");
        _failPanelCanvasGroup.DOFade(1, .5f);
        particleHolder.gameObject.SetActive(true);
        var randIndex = Random.Range(0, _failEmojiHolder.childCount - 1);
        _failEmojiHolder.GetChild(randIndex).gameObject.SetActive(true);
    }


    private void OnWinButtonClicked()
    {
        GameManager.Instance.NextLevel();
    }

    private void OnLoseButtonClicked()
    {
        GameManager.Instance.RestartLevel();
    }

    private void SetFillText(float fillAmount)
    {
        _fillText.SetText($"{fillAmount * 100:0}%");
    }
}