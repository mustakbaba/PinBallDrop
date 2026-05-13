using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridCreator))]
public class GridCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridCreator gridCreator = (GridCreator)target;

        if (GUILayout.Button("Generate Grid"))
        {
            gridCreator.GenerateGrid();
        }
    }
}