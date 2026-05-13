using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : CellController
{
    public List<CellController> GetPathToExit(ColorTypes colorType)
    {
        Queue<(CellController cell, List<CellController> path)> queue = new();
        HashSet<CellController> visited = new();

        var startPath = new List<CellController> { this };
        queue.Enqueue((this, startPath));
        visited.Add(this);
        while (queue.Count > 0)
        {
            var (current, path) = queue.Dequeue();
            // Hedef çıkış bulunduysa (renk uyumluysa)
            if (current.HasBlock)
            {
                if (current.objectColor == colorType)
                {
                    return path;
                }
            }

            foreach (var neighbor in GetValidNeighbors(current, colorType))
            {
                if (!visited.Contains(neighbor))
                {
                    var newPath = new List<CellController>(path) { neighbor };
                    queue.Enqueue((neighbor, newPath));
                    visited.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<CellController> GetValidNeighbors(CellController cell, ColorTypes exitObjectColor)
    {
        List<CellController> neighbors = new List<CellController>();

        int[,] directions = new int[,]
        {
            { 0, 1 }, // yukarı
            { -1, 0 }, // sol
            { 1, 0 }, // sağ
            { 0, -1 } // aşağı (isteğe bağlı)
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int nx = cell.Xpos + directions[i, 0];
            int nz = cell.Zpos + directions[i, 1];

            var neighbor = GridCreator.Instance.GetCell(nx, nz);
            if (neighbor == null || neighbor == this) continue;
            bool isPassable = !neighbor.HasBlock;


            if (isPassable)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }
}