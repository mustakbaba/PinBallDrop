using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lofelt.NiceVibrations;
using SincappStudio;
using UnityEditor;
using UnityEngine;


public enum BlockerTypes
{
    None,
    ChainKey,
    Ice
}

public enum ColorTypes
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange,
    Pink,
    Cyan,
    Brown,
    White,
    DarkBlue,
    
}

[SelectionBase]
public class CellController : MonoBehaviour
{
    public enum CellTypes
    {
        Empty,
        Marble,
        Block,
    }

    public ColorTypes objectColor;
    public CellTypes CellType;
    public BlockerTypes BlockerType;
    public int BlockerLockAmount;
    public int Xpos;
    public int Zpos;
    public bool HasBlock;
    public void OnMouseDown()
    {
        if (CellType != CellTypes.Marble) return;

        // Path kontrolü
        if (!MarblePathChecker.Instance.HasPathToBottom(Xpos, Zpos))
        {
            Debug.Log($"[{Xpos},{Zpos}] marble çıkamaz, blok veya marble var.");
            return;
        }

        // Çıkabilir → marble'ı uçur
        var marbleObj = GetComponentInChildren<MarbleController>();
        if (marbleObj == null) return;
        marbleObj.Drop();
        CellType = CellController.CellTypes.Empty;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += SpawnPrefabs;
    }
#endif

    public void SpawnPrefabs()
    {
        var levelManager = LevelManager.Instance;
        GameObject spawned = null;

        List<GameObject> toDestroy = GetComponentsInChildren<Transform>(true)
            .Where(t => t.name == "Spawned")
            .Select(t => t.gameObject)
            .ToList();

        for (int i = toDestroy.Count - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(toDestroy[i]);
            }
            else
            {
                DestroyImmediate(toDestroy[i]);
            }
        }

        if (CellType == CellTypes.Block)
        {
        }
        else if (CellType == CellTypes.Marble)
        {
            spawned = Instantiate(
                levelManager.MarbleObjPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

            spawned.name = "Spawned";
            spawned.transform.localPosition = Vector3.zero;

            var marble = spawned.GetComponent<MarbleController>();
            marble.SetColor(objectColor);
        }

#if UNITY_EDITOR

        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }
}