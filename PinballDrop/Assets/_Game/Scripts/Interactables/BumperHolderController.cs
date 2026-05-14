using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BumperHolderController : MonoBehaviour
{
    public List<BumperData> TargetObjects = new List<BumperData>();
    public List<Transform> SpawnedObjects = new List<Transform>();
    public float XOffset;
    private BumperController[] _bumpers;


#if UNITY_EDITOR
    private void OnValidate()
    {
        
            if (Application.isPlaying)
                return;

            if (!gameObject.scene.IsValid())
                return;

            EditorApplication.delayCall -= SpawnPrefabs;
            EditorApplication.delayCall += SpawnPrefabs;
    }

#endif


    private void Start()
    {
        _bumpers = GetComponentsInChildren<BumperController>();
        _bumpers[0].IsActiveBumper = true;
    }

    public void SpawnPrefabs()
    {
        List<GameObject> toDestroy = GetComponentsInChildren<Transform>(true)
            .Where(t => t.name == "Spawned")
            .Select(t => t.gameObject)
            .ToList();
        SpawnedObjects = new List<Transform>();
        for (int i = toDestroy.Count - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(toDestroy[i]);
            else
                DestroyImmediate(toDestroy[i]);
        }

        for (int i = 0; i < TargetObjects.Count; i++)
        {
            BumperController obj = null;
            obj = Instantiate(LevelManager.Instance.BumperControllerPrefab, transform);
            SpawnedObjects.Add(obj.transform);

            obj.ObjectColors = TargetObjects[i].Color;
            obj.transform.localPosition = new Vector3(0 - XOffset * i, 0, 0);
            obj.IsHidden = TargetObjects[i].IsHidden;
            obj.Count = TargetObjects[i].Amount;
            obj.name = "Spawned";
            obj.InitTarget();
        }
    }
}

[Serializable]
public struct BumperData
{
    public ColorTypes Color;
    public int Amount;
    public bool IsHidden;
}