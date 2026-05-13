using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDataCollectorEditor : EditorWindow
{
    private Vector2 _scrollPosition;

    [MenuItem("Sincapp/Prefab to ScriptableObject")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataCollectorEditor>("Prefab to ScriptableObject");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select interactables and press the button to create Scriptable Objects for their data.");

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DisplayInteractableSelection();
        SaveDatas();
        EditorGUILayout.EndScrollView();
    }

    private void DisplayInteractableSelection()
    {
        EditorGUILayout.BeginVertical();
        
        bool anyObjectSelected = Selection.gameObjects.Length > 0;

        GUI.enabled = anyObjectSelected;

        foreach (GameObject obj in Selection.gameObjects)
        {
            IDataCollectable[] interactableList = obj.GetComponentsInChildren<IDataCollectable>();

            foreach (IDataCollectable interactableObject in interactableList)
            {
                if (interactableObject != null)
                {
                    EditorGUILayout.ObjectField(interactableObject.GetType().ToString(), obj, typeof(GameObject), true);
                }
            }
        }
    }

    private void SaveDatas()
    {
        if (GUILayout.Button("Save Interactable Variables as ScriptableObjects"))
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                InteractableDataContainer dataContainer = ScriptableObject.CreateInstance<InteractableDataContainer>();

                GameObject obj = Selection.gameObjects[i];
                string prefabName = obj.name;
                string path = "Assets/_Game/Prefabs/Levels/Resources/LevelDatas/" + prefabName + "_Data.asset";

                IDataCollectable[] interactableList = obj.GetComponentsInChildren<IDataCollectable>();

                foreach (IDataCollectable interactableObject in interactableList)
                {
                    if (interactableObject != null)
                    {
                        string prefabPath =
                            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(interactableObject
                                .GetGameObjectReference());
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                        if (prefab != null)
                        {
                            InteractableData data = interactableObject.GetInteractableData();
                            data.InteractableReference = prefab;
                            dataContainer.interactableDataList.Add(data);
                        }
                        else
                        {
                            Debug.LogError("Object is not a Prefab! " + interactableObject.ToString());
                        }
                    }
                }

                AssetDatabase.CreateAsset(dataContainer, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Scriptable Objects for interactables created for selected prefabs.");
        }

        EditorGUILayout.EndVertical();
    }
}