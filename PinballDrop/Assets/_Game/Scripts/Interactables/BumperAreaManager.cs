using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SincappStudio;
using UnityEditor;
using UnityEngine;

public class BumperAreaManager : MonoSingleton<BumperAreaManager>
{
    public List<BumperController> ActiveBumpers = new List<BumperController>();
    public List<BumperHolderData> BumperHolders = new List<BumperHolderData>();

    protected override void Awake()
    {
        base.Awake();
    }
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
    private void Start()
    {
        var lines = GetComponentsInChildren<BumperHolderController>().Where(x => x.SpawnedObjects.Count > 0).ToList();
        StartCoroutine(Sincapp.WaitAndAction(0.01f,() =>
        {
            ActiveBumpers = GetComponentsInChildren<BumperController>().Where(x => x.IsActiveBumper).ToList();
        }));
            
    }
    public void SpawnPrefabs()
    {
       
        var allBumperHolders = GetComponentsInChildren<BumperHolderController>(true);
        
        for (int i = 0; i < BumperHolders.Count; i++)
        {
            var bumperHolder = allBumperHolders[i].transform;
            bumperHolder.gameObject.SetActive(true);
          //  bumperHolder.transform.SetZ_Pos(0,Space.Self);

            //bumperHolder.transform.position = new Vector3(BumperHolders[i].XPos, BumperHolders[i].YPos, bumperHolder.transform.position.z);
        }
    }
    public void CheckWin()
    {
        bool allDone = ActiveBumpers.All(b => b.Count <= 0);
        if (allDone)
            EventManager.OnGameWin?.Invoke();
    }
    
    
}

[Serializable]
public struct BumperHolderData
{
    public float XPos;
    public float YPos;
}