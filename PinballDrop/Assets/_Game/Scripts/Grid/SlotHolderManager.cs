// SlotHolderManager.cs
using System.Collections.Generic;
using UnityEngine;

public class SlotHolderManager : MonoSingleton<SlotHolderManager>
{
    public List<SlotController> Slots = new List<SlotController>();

    private void Start()
    {
        Slots = new List<SlotController>(GetComponentsInChildren<SlotController>());
    }

    // SmallBallController buraya çağırır — uygun slot bulur ve gönderir
    // SlotHolderManager.cs
    public bool TryPlaceBall(SmallBallController ball)
    {
        // Önce aynı renkli dolu olmayan slot
        foreach (var slot in Slots)
        {
            if (slot.HasColor(ball.ObjectColor) && !slot.IsFull)
                return slot.TryAddBall(ball);
        }

        // Sonra ilk boş slot
        foreach (var slot in Slots)
        {
            if (slot.IsAvailable)
                return slot.TryAddBall(ball);
        }

        GameFail();
        return false;
    }

    private void GameFail()
    {
        Debug.Log("FAIL — tüm slotlar dolu!");
        // LevelManager.Instance.Fail(); vs.
    }
}