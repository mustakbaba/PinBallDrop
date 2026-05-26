// SlotController.cs

using System;
using System.Collections.Generic;
using DG.Tweening;
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

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _defColor = _meshRenderer.material.color;
    }

    private void UpdateText()
    {
        _capacityText.text = $"{_balls.Count}/{Capacity}";
    }
    


    private void OnMouseDown()
    {
        if (_balls.Count == 0 || _isClearing) return;
        ClearSlot();
    }

    // SlotController.cs
    public const int Capacity = 18; // 3x2x3
    public const int Columns = 3;   // X
    public const int Depths = 2;    // Z
    public const int Rows = 3;      // Y
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

    Vector3 startOffset = new Vector3(-0.3f, 0.35f, -0.17f);

        Vector3 localTarget = startOffset + new Vector3(
            col * Spacing*1.5f,
            row * Spacing,
            depth * (Spacing*1.5f)
        );

        _balls.Add(ball);

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

        if (available <= 0) return;

        _isClearing = true;

        int sendCount = Mathf.Min(_balls.Count, available);
        capacityManager.SetAmount(sendCount);

        var ballsToProcess = _balls.GetRange(0, sendCount);
        _balls.RemoveRange(0, sendCount);

        for (int i = 0; i < ballsToProcess.Count; i++)
        {
            var ball = ballsToProcess[i];
            if (ball == null) continue;

            float delay = i * 0.05f;
            ball.transform.DOKill();
            ball.transform.SetParent(null);

            // Delay ile sırayla gönder
            DOVirtual.DelayedCall(delay, () =>
            {
                if (ball == null) return;
                ball.GoToPipe();
            });
        }

        // Son top gönderildikten sonra unlock
        float totalDelay = (ballsToProcess.Count - 1) * 0.05f + 0.1f;
        DOVirtual.DelayedCall(totalDelay, () =>
        {
            _isClearing = false;

            if (_balls.Count == 0)
            {
                SlotColor = default;
                _meshRenderer.material.color = _defColor;
            }

            UpdateText();
        });
    }
}