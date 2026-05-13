using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]

public class GridCreator : MonoSingleton<GridCreator>
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1.2f;

    public CellController cellPrefab;
    private CellController[,] _grid;

    public void GenerateGrid()
    {
#if UNITY_EDITOR

        var gridHolder = GameObject.Find("GridHolder");
        
        if (gridHolder != null)
        {
            DestroyImmediate(gridHolder);
        }
      
        gridHolder = new GameObject("GridHolder");
        gridHolder.transform.SetParent(transform);
        gridHolder.transform.localPosition = Vector3.zero;
            
        _grid = new CellController[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float offsetX = (width - 1) * cellSize * 0.5f; // toplam genişliğin yarısı
                Vector3 pos = new Vector3(x * cellSize - offsetX, 0, z * cellSize);
                
                CellController cell = PrefabUtility.InstantiatePrefab(cellPrefab) as CellController;
                 
                cell.transform.SetParent(gridHolder.transform);
                cell.transform.localPosition = pos;
                    
                cell.name = $"Cell_{x}_{z}";
                    
                if (cell != null)
                {
                    cell.Xpos = x;
                    cell.Zpos = z;
                }
                    
                _grid[x, z] = cell;
            }
        }
#endif
    }
    public CellController GetCell(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height)
            return null;

        if (_grid != null)
            return _grid[x, z];

        // _grid yoksa sahneden bul
        var gridHolder = transform.Find("GridHolder");
        if (gridHolder == null) return null;

        var cell = gridHolder.Find($"Cell_{x}_{z}");
        return cell != null ? cell.GetComponent<CellController>() : null;
    }
}