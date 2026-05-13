using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractableDataContainer : ScriptableObject
{
    public List<InteractableData> interactableDataList = new List<InteractableData>();
}

[System.Serializable]
public class InteractableData
{
    public List<InteractableValues> InteractableValueList = new List<InteractableValues>();
    public InteractableValues.TransformValuesData TransformValues;
    public GameObject InteractableReference;
}

[System.Serializable]
public class InteractableValues
{
    public PropertyValuesData PropertyValues;

    [System.Serializable]
    public class PropertyValuesData
    {
        public string Name;
        [ShowIf("Value", "")]
        public string Value;
        [ShowIf("ValueSO", "null")]
        public ScriptableObject ValueSO;
    }

    [System.Serializable]
        public class TransformValuesData
        {
            public Quaternion Rotation;
            public Vector3 Position;
        }
    }