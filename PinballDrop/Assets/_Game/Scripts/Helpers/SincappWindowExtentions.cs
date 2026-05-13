using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR

public class SincappWindowExtentions : MonoBehaviour
{
    [MenuItem("Sincapp/Clear Persist Data")]
    public static void ClearPersistData()
    {
        var persistFileName = "Persist Sincapp";
        var persistFileLocation = Application.persistentDataPath + Path.DirectorySeparatorChar + persistFileName;

        if (File.Exists(persistFileLocation))
        {
            File.Delete(persistFileLocation);
        }
    }
}

#endif