using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    private void Awake()
    {
        string path = $"LevelDatas/Level_{PersistData.Instance.CurrentLevel}_Data";
        var interactableDataContainer = Resources.Load<InteractableDataContainer>(path);
        var interactableDataList = interactableDataContainer.interactableDataList;
        
        foreach (InteractableData interactableData in interactableDataList)
        {
            var instantiatedObject = Instantiate(interactableData.InteractableReference);

            IDataCollectable iDataCollectable = instantiatedObject.GetComponent<IDataCollectable>();
            
            if (iDataCollectable != null)
            {
                LevelDataHandler.SetData(iDataCollectable,interactableData,instantiatedObject.transform);
            }
        }
    }
}
