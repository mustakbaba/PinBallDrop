using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


public class LineBoxHolderManager : MonoBehaviour
{
    public List<TargetBoxData> TargetObjects = new List<TargetBoxData>();
    public List<Transform> SpawnedObjects = new List<Transform>();
    public float ZOffset;


#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying)
            {
                SpawnPrefabs();
            }
        };
    }
#endif

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
            LineBoxController obj = null;
            obj = Instantiate(LevelManager.Instance.LineBoxPrefab, transform);
            SpawnedObjects.Add(obj.transform);

            obj.ObjectColors = TargetObjects[i].Color;
            obj.transform.localPosition = new Vector3(0, 0, 0 - ZOffset * i);
            obj.IsHidden = TargetObjects[i].IsHidden;
            obj.Count = TargetObjects[i].Amount;
            obj.IsConnectedBlock = TargetObjects[i].IsConnected;
            obj.Id = TargetObjects[i].ConnectedIndex;
            obj.name = "Spawned";
            obj.InitTarget();
        }
    }

    public void RemoveTargetBox(LineBoxController targetBox)
    {
        if (SpawnedObjects.Contains(targetBox.transform))
        {
            SpawnedObjects.Remove(targetBox.transform);
            Destroy(targetBox.gameObject);
            RePosition();
        }
    }

    public int GetIndex(LineBoxController box)
    {
        return SpawnedObjects.IndexOf(box.transform);
    }

    private void RePosition()
    {
        for (var i = 0; i < SpawnedObjects.Count; i++)
        {
            var obj = SpawnedObjects[i];
            obj.transform.DOLocalMove(new Vector3(0, 0, 0 - ZOffset * i), 0.15f).SetEase(Ease.Linear);

            if (i == 0)
            {
                if (obj.GetComponent<LineBoxController>().IsHidden)
                {
                    obj.GetComponent<LineBoxController>().Reveal();
                }

                obj.GetComponent<LineBoxController>().SetTextOpacity(true);
            }
        }
    }
}

[Serializable]
public struct TargetBoxData
{
    public ColorTypes Color;
    public int Amount;
    public bool IsHidden;
    public bool IsConnected;
    [ShowIf("IsConnected")]  public int ConnectedIndex;
    
}