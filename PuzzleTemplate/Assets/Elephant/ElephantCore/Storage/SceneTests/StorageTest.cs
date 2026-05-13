using System.Collections;
using UnityEngine;

namespace ElephantSDK
{
    public class StorageTest : MonoBehaviour
    {
        private PlayerSaveData _model;

        void Start()
        {
            _model = new PlayerSaveData();
            Elephant.LoadStorage(_model);
            
            StartCoroutine(ChangeAndSyncModelsCoroutine());
        }
        IEnumerator ChangeAndSyncModelsCoroutine()
        {
            yield return new WaitForSeconds(15);
        
            _model.Level += 1;

            yield return new WaitForSeconds(15);
        
            _model.Name += "x";
        }
    }
}
