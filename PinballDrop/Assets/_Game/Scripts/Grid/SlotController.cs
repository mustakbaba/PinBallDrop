// SlotController.cs

using System;
using System.Collections.Generic;
using DG.Tweening;
using Lofelt.NiceVibrations;
using TMPro;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    public ColorTypes SlotColor { get; private set; }
    public bool IsAvailable => _balls.Count == 0;
    public bool IsFull => _balls.Count >= Columns * Depths * Rows;
    public bool HasColor(ColorTypes color) => !IsAvailable && SlotColor == color;

    private List<SmallBallController> _balls = new List<SmallBallController>();
    [SerializeField] private TextMeshPro _capacityText;
    private MeshRenderer _meshRenderer;
    private Color _defColor;
    private bool _isClearing;
    private int _clearTweenId;
    private bool _isNotEmpty;

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _defColor = _meshRenderer.material.color;
        UpdateText();
    }

    private void OnEnable()
    {
        EventManager.OnGameLose += GameLose;
    }

    private void OnDisable()
    {
        EventManager.OnGameLose -= GameLose;
    }

    private void UpdateText()
    {
        _capacityText.text = $"{_balls.Count}/{Capacity}";
    }


    private void OnMouseDown()
    {
        if (_balls.Count == 0 || _isClearing) return;
        if (GameManager.Instance.IsGameLose) return;
        ClearSlot();
    }

    // SlotController.cs
    public const int Capacity = 20; // 3x2x3
    public const int Columns = 5; // X
    public const int Depths = 2; // Z
    public const int Rows = 2; // Y
    public float Spacing = 0.5f;

    public bool TryAddBall(SmallBallController ball)
    {
        if (IsFull) return false;
        if (!IsAvailable && SlotColor != ball.ObjectColor) return false;
        if (_balls.Contains(ball)) return false;

        if (IsAvailable)
            SlotColor = ball.ObjectColor;


        int index = _balls.Count;
        int col = index % Columns;
        int depth = (index / Columns) % Depths;
        int row = index / (Columns * Depths);

// Son 4 top için Y daha yüksek
        float extraY = index >= Capacity - 5 ? 0.07f : 0f;

        Vector3 startOffset = new Vector3(-0.4f, 0.57f + extraY, -0.17f);

        Vector3 localTarget = startOffset + new Vector3(
            col * Spacing * 1.5f,
            row * Spacing,
            depth * (Spacing * 1.5f)
        );

        _balls.Add(ball);

        transform.DOKill();
        transform.DOScale(1.1f, .1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            transform.DOScale(1f, .1f).OnComplete(() =>
            {
                // Top gelmeyi kestikten sonra loop başlat
                if (_balls.Count > 0)
                {
                    transform.DOScale(1.1f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                    // transform.DOLocalMoveZ(.1f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                }
            });
        });

        ball.transform.DOKill();

        var rb = ball.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        var col2 = ball.GetComponent<Collider>();
        if (col2 != null) col2.enabled = false;

        ball.transform.SetParent(transform);
        ball.transform.localPosition = new Vector3(0f, 0f, 0f);
        ball.transform.DOLocalMove(localTarget, 0.3f).SetEase(Ease.OutBack);
        _meshRenderer.material.color = LevelManager.Instance.ObjectColors[(int)SlotColor];
        UpdateText();
        return true;
    }

    private void ClearSlot()
    {
        var capacityManager = AreaCapacityManager.Instance;
        int available = capacityManager.CapacityAmount - capacityManager.CurrentAmount;
        _clearTweenId = GetInstanceID();
        if (available <= 0) return;

        _isClearing = true;
        SoundManager.Instance.SlotClickSound();
        int sendCount = Mathf.Min(_balls.Count, available);
        capacityManager.SetAmount(sendCount);

        var ballsToProcess = _balls.GetRange(0, sendCount);

        for (int i = 0; i < ballsToProcess.Count; i++)
        {
            var ball = ballsToProcess[i];
            if (ball == null) continue;

            float delay = i * 0.05f;
            ball.transform.DOKill();
            ball.transform.SetParent(null);
            ball.transform.localScale = Vector3.one * .27f;

            DOVirtual.DelayedCall(delay, () =>
            {
                if (ball == null) return;
                ball.GoToPipe();
                _balls.Remove(ball); // tek tek çıkar
                UpdateText();
            }).SetId(_clearTweenId);
            ;
        }
        transform.DOKill();
        transform.DOScale(1f, .1f);
        transform.DOLocalMoveZ(0, .1f);
        // Son top gönderildikten sonra unlock
        float totalDelay = (ballsToProcess.Count - 1) * 0.075f + 0.1f;
        DOVirtual.DelayedCall(totalDelay, () =>
        {
            _isClearing = false;

            if (_balls.Count == 0)
            {
                SlotColor = default;
                _meshRenderer.material.color = _defColor;
                _isNotEmpty = false;
            }

            UpdateText();
        }).SetId(_clearTweenId);
        ;

        HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
    }

    private void GameLose()
    {
        DOTween.Kill(_clearTweenId); // delayed call'ları öldür

        foreach (var ball in _balls)
        {
            if (ball == null) continue;
            ball.transform.DOKill();
        }

        _isClearing = false;
        UpdateText();
    }
}