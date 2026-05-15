// SlotController.cs
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    public const int Capacity = 12;
    public const int Columns = 4;
    public const int Rows = 3;
    public float Spacing = 0.35f;

    public ColorTypes SlotColor { get; private set; }
    public bool IsAvailable => _balls.Count == 0;
    public bool IsFull => _balls.Count >= Capacity;
    public bool HasColor(ColorTypes color) => !IsAvailable && SlotColor == color;

    private List<SmallBallController> _balls = new List<SmallBallController>();

    private void OnMouseDown()
    {
        if (_balls.Count == 0) return;
        ClearSlot();
    }

    // SlotController.cs
    public bool TryAddBall(SmallBallController ball)
    {
        if (IsFull) return false;
        if (!IsAvailable && SlotColor != ball.ObjectColor) return false;
        if (_balls.Contains(ball)) return false;

        if (IsAvailable)
            SlotColor = ball.ObjectColor;

        int index = _balls.Count;
        int col = index % Columns;
        int row = index / Columns;

        float offsetX = (Columns - 1) * Spacing * 0.5f;
        float offsetY = (Rows - 1) * Spacing * 0.5f;

        Vector3 localTarget = new Vector3(
            col * Spacing - offsetX,
            row * Spacing - offsetY,
            0f
        );

        _balls.Add(ball);

        // Önce tüm tweenleri öldür
        ball.transform.DOKill();
    
        // Fiziği tamamen kapat
        var rb = ball.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    
        // Collider'ı kapat — diğer toplarla çakışmasın
        var col2 = ball.GetComponent<Collider>();
        if (col2 != null) col2.enabled = false;

        // Parent'a al, sonra hareket et
        ball.transform.SetParent(transform);
        // ball.enabled = false;

        ball.transform.DOLocalMove(localTarget, 0.3f).SetEase(Ease.OutBack);

        return true;
    }

    private void ClearSlot()
    {
        var capacityManager = AreaCapacityManager.Instance;
        int available = capacityManager.CapacityAmount - capacityManager.CurrentAmount;
    
        if (available <= 0) return;

        int sendCount = Mathf.Min(_balls.Count, available);
    
        
        capacityManager.SetAmount(sendCount);

        var ballsToProcess = _balls.GetRange(0, sendCount);
        _balls.RemoveRange(0, sendCount);

        foreach (var ball in ballsToProcess)
        {
            if (ball == null) continue;
            ball.transform.DOKill();
            ball.transform.SetParent(null);
            ball.GoToPipe();
        }

        if (_balls.Count == 0)
            SlotColor = default;
    }
}