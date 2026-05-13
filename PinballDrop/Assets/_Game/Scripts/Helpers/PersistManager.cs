using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class PersistManager<T> : ScriptableObject where T : PersistManager<T>
{
    private static string persistFileName = "Persist Sincapp";
    private static string _persistFileLocation;
    static T _instance;
    public static T Instance => GetInstance();

    private static T GetInstance()
    {
        _persistFileLocation = Application.persistentDataPath + Path.DirectorySeparatorChar + persistFileName;

        if (!_instance)
        {
            var type = typeof(T);
            var attribute = type.GetCustomAttribute<LoadAssetFrom>();
            var so = attribute == null ? CreateInstance<T>() : Resources.Load<T>(type.Name);
            if (so == null)
            {
                throw new Exception(
                    "An unknown error occured while creating the instance for ScriptableSingle of type " +
                    type.FullName + "!");
            }

            _instance = so;
            (so).OnInitialize();
        }

        return _instance;
    }

    protected virtual void OnInitialize()
    {
        var location = _persistFileLocation;

        if (File.Exists(location))
        {
            var json = File.ReadAllText(_persistFileLocation,Encoding.UTF8);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(this);
        File.WriteAllText(_persistFileLocation,json,Encoding.UTF8);
    }

    public void Clear()
    {
        if (File.Exists(_persistFileLocation))
        {
            File.Delete(_persistFileLocation);
        }
    }
    
    public void ClearInGame()
    {
        PersistData newPersist = CreateInstance<PersistData>();
        newPersist.CurrentLevel = PersistData.Instance.CurrentLevel;
        var json = JsonUtility.ToJson(newPersist);
        JsonUtility.FromJsonOverwrite(json, this);
        newPersist.Save();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class LoadAssetFrom : Attribute
{
}