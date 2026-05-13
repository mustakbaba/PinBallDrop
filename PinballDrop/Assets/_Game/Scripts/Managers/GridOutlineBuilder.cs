using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridOutlineBuilder : MonoBehaviour
{
    [SerializeField] private Transform gridParent; // Grid'in parent objesi
    public GameObject wallPrefab;
    public float wallThickness = 0.2f;
    public float wallHeight = 1f;
    public float gridSize = 1f;

    private void Start()
    {
        bool[,] grid = new bool[7, 7];

        for (int i = 0; i < 49 ; i++)
        {
            int x = i % 7;
            int y = i / 7;
            
            grid[x, y] = gridParent.GetChild(i).GetComponent<CellController>().CellType != CellController.CellTypes.Block;
        }
        
        var outlineEdges = GetOutlineEdges(grid,gridSize);

        foreach (var edge in outlineEdges)
        {
            Debug.DrawLine(edge.start, edge.end, Color.red, 5f); // Scene view'da görünür
        }
        
        BuildWallMesh(outlineEdges);
    }
  
        
    public struct Edge
    {
        public Vector2 start;
        public Vector2 end;

        public Edge(Vector2 s, Vector2 e)
        {
            start = s;
            end = e;
        }
    }


    List<Edge> GetOutlineEdges(bool[,] grid, float cellSize = 1f)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        List<Edge> edges = new();
        float spacing = .05f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y]) continue;

                Vector2 bottomLeft = new Vector2(x * cellSize, y * cellSize);
                Vector2 bottomRight = bottomLeft + Vector2.right * cellSize;
                Vector2 topLeft = bottomLeft + Vector2.up * cellSize;
                Vector2 topRight = bottomRight + Vector2.up * cellSize;

                // Sol kenar
                if (x == 0 || !grid[x - 1, y])
                    edges.Add(new Edge(bottomLeft, topLeft));

                // Sağ kenar
                if (x == width - 1 || !grid[x + 1, y])
                    edges.Add(new Edge(bottomRight, topRight));

                // Alt kenar
                if (y == 0 || !grid[x, y - 1])
                    edges.Add(new Edge(bottomLeft, bottomRight));

                // Üst kenar
                if (y == height - 1 || !grid[x, y + 1])
                    edges.Add(new Edge(topLeft, topRight));
            }
        }

        return edges;
    }
    
    public void BuildWallMesh(List<Edge> edges)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> triangles = new();

        int vertIndex = 0;

        foreach (var edge in edges)
        {
            Vector2 start2D = edge.start;
            Vector2 end2D = edge.end;

            Vector3 start = new Vector3(start2D.x, 0, start2D.y);
            Vector3 end = new Vector3(end2D.x, 0, end2D.y);
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * wallThickness / 2f;

            // 4 köşe (alt)
            Vector3 v0 = start - perpendicular;
            Vector3 v1 = start + perpendicular;
            Vector3 v2 = end + perpendicular;
            Vector3 v3 = end - perpendicular;

            // 4 köşe (üst)
            Vector3 v4 = v0 + Vector3.up * wallHeight;
            Vector3 v5 = v1 + Vector3.up * wallHeight;
            Vector3 v6 = v2 + Vector3.up * wallHeight;
            Vector3 v7 = v3 + Vector3.up * wallHeight;


            // Üst
            AddQuad(vertices, triangles, v4, v5, v6, v7, ref vertIndex);

            // Sağ
            AddQuad(vertices, triangles, v3, v2, v6, v7, ref vertIndex);

            // Sol
            AddQuad(vertices, triangles, v1, v0, v4, v5, ref vertIndex);

            // Ön
            AddQuad(vertices, triangles, v0, v3, v7, v4, ref vertIndex);

            // Arka
            AddQuad(vertices, triangles, v2, v1, v5, v6, ref vertIndex);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<MeshRenderer>();

        filter.mesh = mesh;
    }

    void AddQuad(List<Vector3> verts, List<int> tris, Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl, ref int index)
    {
        verts.Add(bl); // 0
        verts.Add(br); // 1
        verts.Add(tr); // 2
        verts.Add(tl); // 3

        tris.Add(index);
        tris.Add(index + 1);
        tris.Add(index + 2);

        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 3);

        index += 4;
    }
}
