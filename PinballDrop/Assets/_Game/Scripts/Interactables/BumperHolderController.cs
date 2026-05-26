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
            else if (i == 1 || i == 2)
            {
                // 2. ve 3. görünür ama küçük
                _bumpers[i].transform.localScale = Vector3.one * 0.5f;
            }
            else
            {
                // 4. ve sonrası gizli, 3. bumper'ın pozisyonunda bekle
                _bumpers[i].transform.localScale = Vector3.zero;
                _bumpers[i].transform.position = _bumpers[2].transform.position;
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
        {
            // Bu holder'da next yok, tüm bumperlar bitti
            CheckHolderComplete(current);
            return null;
        }

        var next = SpawnedObjects[currentIndex + 1].GetComponent<BumperController>();
        if (next == null || next.IsActiveBumper) return null;

        // Tüm arkadakileri bir öne kaydır — ama sadece görünür olanlar (index <= 2'ye kadar)
        for (int i = currentIndex + 1; i < SpawnedObjects.Count; i++)
        {
            var bumper = SpawnedObjects[i].GetComponent<BumperController>();
            if (bumper == null) continue;

            int newIndex = i - 1; // yeni sıra indexi

            if (newIndex <= 2)
            {
                // Görünür sıraya giriyor — pozisyona git ve büyü
                Vector3 targetPos = SpawnedObjects[i - 1].position;
                bumper.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutBack);

                if (newIndex == 2)
                {
                    // 3. sıraya yerleşiyor — 3. boyutuna büyü
                    bumper.transform.DOScale(0.5f, 0.3f).SetEase(Ease.OutBack);
                }
            }
            // 4. ve sonrası hareket etmesin, pozisyonda kalsın
        }

        next.IsActiveBumper = true;
        next.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        next.IsHidden = false;
        next.SetColor();

        int listIndex = BumperAreaManager.Instance.ActiveBumpers.IndexOf(current);
        if (listIndex >= 0)
            BumperAreaManager.Instance.ActiveBumpers[listIndex] = next;

        return next;
    }
    private void CheckHolderComplete(BumperController lastBumper)
    {
        bool allDone = SpawnedObjects
            .Select(t => t.GetComponent<BumperController>())
            .Where(b => b != null)
            .All(b => b.Count <= 0);

        if (allDone)
        {
            var fake = Instantiate(LevelManager.Instance.FakeBumperPrefab, lastBumper.transform.position, lastBumper.transform.rotation);
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