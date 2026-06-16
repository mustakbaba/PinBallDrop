using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ElephantSDK;
using Lofelt.NiceVibrations;
using SincappStudio;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class InGameUIManager : MonoSingleton<InGameUIManager>
{
    [SerializeField] private TextMeshProUGUI _levelDifficultyText;
    [SerializeField] private Image _levelDifficultyImage;
    [SerializeField] private GameObject _hardLevelSplash;
    [SerializeField] private GameObject _tapToStart;
    [SerializeField] private TextMeshProUGUI _playerMoney;
    [SerializeField] private Transform _collectableSprite;
    [SerializeField] private Transform _collectableTargetTransform;
    [SerializeField] private Transform targetImage;
    [SerializeField] private Material _greyMat;
    [SerializeField] private Slider _vibrationSlider;
    [SerializeField] private Slider _soundsSlider;
    [SerializeField] private GameObject _moneyImage;
    [SerializeField] private GameObject _settingsImage;
    [SerializeField] private GameObject _rewardedCanvas;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private GameObject _timeUiHolder;
    [SerializeField] private GameObject _blockerPopup;
    [SerializeField] private Image _blockerImage;
    public bool IsBlockerPopupOpen { get; set; }

    [SerializeField] private TextMeshProUGUI _blockerNameText;
    [SerializeField] private TextMeshProUGUI _blockerDescText;
    [SerializeField] private Transform _slotTuto;
    [SerializeField] private Transform _ballsTuto;

    private Animator _animator;
    [SerializeField] private CanvasGroup _noSpaceWarningCanvasGroup;
    private bool _canSpawnNoSpaceWarning = true;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();
        _levelText.SetText($"LEVEL {PersistData.Instance.CurrentLevel}");

        if (LevelManager.Instance.IsTimePuzzleActive)
        {
            _timeUiHolder.SetActive(true);
        }

        var isHapticOn = PlayerPrefsX.GetBool("HapticMode", true);
        var isSoundsOn = PlayerPrefsX.GetBool("SoundsActivate", true);


        _vibrationSlider.value = isHapticOn ? 1 : 0;
        HapticController.hapticsEnabled = isHapticOn;

        _soundsSlider.value = isSoundsOn ? 1 : 0;
        AudioListener.pause = !isSoundsOn;
        ;


        if (PersistData.Instance.RecentlyBlockerReached)
        {
            OpenBlockerPopup();
            PersistData.Instance.RecentlyBlockerReached = false;
        }
    }

    private void OnEnable()
    {
        EventManager.OnGameWin += DisableInGameUI;
        EventManager.OnGameLose += DisableInGameUI;
    }

    private void OnDisable()
    {
        EventManager.OnGameWin -= DisableInGameUI;
        EventManager.OnGameLose -= DisableInGameUI;
    }

    private void Start()
    {
        StartCoroutine(Sincapp.WaitAndAction(.5f, () =>
        {
            if (!PersistData.Instance.IsBallsTutoShown)
            {
                OpenBallTuto();
            }
        }));
    }
    
    public void OpenBallTuto()
    {
        _ballsTuto.gameObject.SetActive(true);
    }
    public void CloseBallTuto()
    {
        _ballsTuto.gameObject.SetActive(false);
    }

    public void OpenSlotTuto()
    {
        _slotTuto.gameObject.SetActive(true);
    }
    public void CloseSlotTuto()
    {
        _slotTuto.gameObject.SetActive(false);
    }

    public void OpenBlockerPopup()
    {
        _blockerPopup.SetActive(true);
        IsBlockerPopupOpen = true;
        _blockerImage.sprite =
            Resources.Load<Sprite>($"BlockerSprites/Blocker_{PersistData.Instance.CurrentBlockerIndex}");

        var blockerData =
            Resources.Load<BlockerData>($"BlockerSprites/Blocker_{PersistData.Instance.CurrentBlockerIndex}");
        _blockerImage.sprite = blockerData.BlockerSprite;
        _blockerNameText.SetText(blockerData.BlockerName);
        _blockerDescText.SetText(blockerData.Description);
    }

    public void DisablePopup()
    {
        IsBlockerPopupOpen = false;
        _blockerPopup.SetActive(false);
    }

    private void LateUpdate()
    {
        _timeText.SetText(
            $"{Mathf.FloorToInt(LevelManager.Instance.DurationSeconds / 60)}:{Mathf.FloorToInt(LevelManager.Instance.DurationSeconds % 60):00}");

        Vector2 localPoint;
        RectTransform canvasRect = targetImage.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition,
                targetImage.GetComponentInParent<Canvas>().worldCamera, out localPoint))
        {
            targetImage.localPosition = localPoint;
        }

        if (LevelManager.Instance.DurationSeconds < 15)
        {
            _timeText.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 2, 1));
        }
    }

    private void UpdateMoneyText()
    {
        _playerMoney.SetText((PersistData.Instance.Money).ToString("f0"));
        FadeMoney();
    }

    public void NoSpaceWarning()
    {
        if (!_canSpawnNoSpaceWarning)
        {
            return;
        }

        _canSpawnNoSpaceWarning = false;
        var spawnedWarning = Instantiate(_noSpaceWarningCanvasGroup, Vector3.zero, Quaternion.identity, transform);
        spawnedWarning.transform.localPosition = Vector3.zero;
        spawnedWarning.transform.DOMoveY(spawnedWarning.transform.position.y + 250, 1.45f)
            .OnComplete(() =>
            {
                Destroy(spawnedWarning.gameObject);
                _canSpawnNoSpaceWarning = true;
            });
        spawnedWarning.DOFade(0, 1f).SetDelay(.35f);
    }

    public void SetLevelDifficulty(LevelData.LevelDifficulties levelDifficulties)
    {
        switch (levelDifficulties)
        {
            case LevelData.LevelDifficulties.Easy:
                _levelDifficultyText.SetText("Easy");
                _levelDifficultyImage.color = new Color(0.3f, 0.8f, 0.16f);
                break;
            case LevelData.LevelDifficulties.Medium:
                _levelDifficultyText.SetText("Medium");
                _levelDifficultyImage.color = new Color(0.91f, 0.74f, 0.14f);
                break;
            case LevelData.LevelDifficulties.Hard:
                _levelDifficultyText.SetText("Hard");
                _levelDifficultyImage.color = new Color(0.81f, 0.1f, 0.13f);
                break;
        }
    }

    public void HardLevelSplash()
    {
        StartCoroutine(Sincapp.WaitAndAction(0.01f,() =>
        {
            _hardLevelSplash.SetActive(true);

            _hardLevelSplash.transform.DOScale(Vector3.one * 4, 1f).OnComplete(() =>
            {
                _hardLevelSplash.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    _hardLevelSplash.SetActive(false);
                });
            });
        }));
      
    }

    public void GameUIStatus(bool isOpen)
    {
        if (isOpen)
        {
            _levelText.transform.parent.gameObject.SetActive(true);
            _settingsImage.SetActive(true);
            _moneyImage.SetActive(true);
            _rewardedCanvas.SetActive(true);
        }
        else
        {
            _levelText.transform.parent.gameObject.SetActive(false);
            _settingsImage.SetActive(false);
            _moneyImage.SetActive(false);
            _rewardedCanvas.SetActive(false);
        }
    }

    private void FadeMoney()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("MoneyFade")) return;

        _animator.Play("MoneyFade");
    }

    public void RestartLevel()
    {
        GameManager.Instance.RestartLevel();
    }

    private void DisableInGameUI()
    {
        gameObject.SetActive(false);
    }

    public void SetVibration()
    {
        _vibrationSlider.value = _vibrationSlider.value == 1 ? 0 : 1;
        bool isHapticOn = _vibrationSlider.value == 1;
        HapticController.hapticsEnabled = isHapticOn;
        PlayerPrefsX.SetBool("HapticMode", isHapticOn);
    }

    public void SetSounds()
    {
        _soundsSlider.value = _soundsSlider.value == 1 ? 0 : 1;
        bool isSoundsOn = _soundsSlider.value == 1;
        AudioListener.pause = !isSoundsOn;
        PlayerPrefsX.SetBool("SoundsActivate", isSoundsOn);
    }

    public void DirectPrivacy()
    {
        Elephant.ShowSettingsView();
    }

    private void MoneySendUi(Vector3 spawnPos, float money)
    {
        var mainCam = Camera.main;
        var a = Mathf.InverseLerp(100, 1000, money);
        var b = Mathf.Lerp(1, 15, a);

        for (int i = 0; i < (int)1; i++)
        {
            Transform moneySpawned = Instantiate(_collectableSprite, mainCam.WorldToScreenPoint(spawnPos),
                Quaternion.identity,
                transform);
            float randY = Random.Range(220, 270);
            moneySpawned.DOMove(mainCam.WorldToScreenPoint(spawnPos) + Vector3.up * 100 + new Vector3(0, randY),
                    .7f)
                .OnComplete(() =>
                {
                    moneySpawned.DOMove(_collectableTargetTransform.position, .7f).OnComplete(() =>
                    {
                        FadeMoney();
                        Destroy(moneySpawned.gameObject);
                    });
                });
        }
    }
}