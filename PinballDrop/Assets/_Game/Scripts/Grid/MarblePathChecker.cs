using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SincappStudio;

public class MarblePathChecker : MonoSingleton<MarblePathChecker>
{
    private void Start()
    {
        StartCoroutine(Sincapp.WaitAndAction(0.1f, () => { CheckAllMarbles(); }));
    }

    // Tüm marble'ları path'e göre günceller
    public void CheckAllMarbles()
    {
        var grid = GridCreator.Instance;

        for (int x = 0; x < grid.width; x++)
        {
            for (int z = 0; z < grid.height; z++)
            {
                var cell = grid.GetCell(x, z);
                if (cell == null || cell.CellType != CellController.CellTypes.Marble)
                    continue;

                bool canExit = HasPathToBottom(x, z);
                UpdateMarbleVisual(cell, canExit);
            }
        }
    }

    // BFS ile Z=0 satırına yol var mı kontrol et
    public bool HasPathToBottom(int startX, int startZ)
    {
        var grid = GridCreator.Instance;
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();

        queue.Enqueue((startX, startZ));
        visited.Add((startX, startZ));

        int[] dx = { -1, 1, 0, 0 };
        int[] dz = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (cx, cz) = queue.Dequeue();

            // Z=0 satırına ulaştı mı?
            if (cz == 0)
                return true;

            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int nz = cz + dz[i];

                if (visited.Contains((nx, nz))) continue;

                var neighbor = grid.GetCell(nx, nz);
                if (neighbor == null) continue;

                // Block veya başka bir marble → geçilemez
                if (neighbor.CellType == CellController.CellTypes.Block) continue;
                if (neighbor.CellType == CellController.CellTypes.Marble &&
                    !(nx == startX && nz == startZ)) continue;

                visited.Add((nx, nz));
                queue.Enqueue((nx, nz));
            }
        }

        return false;
    }

    private void UpdateMarbleVisual(CellController cell, bool canExit)
    {
        var marbleObj = cell.GetComponentInChildren<MarbleController>();
        if (marbleObj == null) return;

        float targetScale = canExit ? 1.2f : 1.0f;
        marbleObj.transform.DOScale(targetScale, 0.25f).SetEase(Ease.OutBack);
    }
}