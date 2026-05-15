using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BumperHolderController : MonoBehaviour
{
    public List<BumperData> TargetObjects = new List<BumperData>();
    public List<Transform> SpawnedObjects = new List<Transform>();
    private float XOffset = .325f;
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

        for (int i = 0; i < _bumpers.Length; i++)
        {
            if (i == 0)
            {
                _bumpers[i].IsActiveBumper = true;
                _bumpers[i].transform.localScale = Vector3.one;
            }
            else
            {
                _bumpers[i].transform.localScale = Vector3.one * 0.5f;
            }
        }
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

            obj.ObjectColor = TargetObjects[i].Color;
            if (transform.position.x < 0)
            {
                XOffset = -.325f;
            }

            obj.transform.localPosition = new Vector3(0 + XOffset * i, 0, 0);
            obj.IsHidden = TargetObjects[i].IsHidden;
            obj.Count = TargetObjects[i].Amount;
            obj.name = "Spawned";
            obj.InitTarget();
        }
    }

    // BumperHolderController.cs — GetNextBumper metodu ekle
    public BumperController GetNextBumper(BumperController current)
    {
        int currentIndex = -1;
        for (int i = 0; i < SpawnedObjects.Count; i++)
        {
            if (SpawnedObjects[i] == current.transform)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex < 0 || currentIndex + 1 >= SpawnedObjects.Count)
            return null;

        var next = SpawnedObjects[currentIndex + 1].GetComponent<BumperController>();
        if (next == null || next.IsActiveBumper) return null;

        // Tüm arkadakileri bir öne kaydır
        for (int i = currentIndex + 1; i < SpawnedObjects.Count; i++)
        {
            var bumper = SpawnedObjects[i].GetComponent<BumperController>();
            if (bumper == null) continue;

            Vector3 targetPos = SpawnedObjects[i - 1].position;
            bumper.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutBack);
        }

        next.IsActiveBumper = true;
        next.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

        int listIndex = BumperAreaManager.Instance.ActiveBumpers.IndexOf(current);
        if (listIndex >= 0)
            BumperAreaManager.Instance.ActiveBumpers[listIndex] = next;

        return next;
    }
}

[Serializable]
public struct BumperData
{
    public ColorTypes Color;
    public int Amount;
    public bool IsHidden;
}