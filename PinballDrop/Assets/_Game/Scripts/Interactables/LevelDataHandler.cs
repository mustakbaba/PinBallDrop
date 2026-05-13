using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

public class LevelDataHandler
{
    public static void AddProperty(InteractableData data, params (string propertyName,string propertyValue)[] properties)
    {
        foreach ((string propertyName, string propertyValue) property in properties)
        {
            InteractableValues.PropertyValuesData propertyData = new InteractableValues.PropertyValuesData
            {
                Name = property.propertyName,
                Value = property.propertyValue
            };
        
            data.InteractableValueList.Add(new InteractableValues
            {
                PropertyValues = propertyData
            });   
        }
    }
    
    public static void AddProperty(InteractableData data, params (string propertyName,ScriptableObject propertyValue)[] properties)
    {
        foreach ((string propertyName, ScriptableObject propertyValue) property in properties)
        {
            InteractableValues.PropertyValuesData propertyData = new InteractableValues.PropertyValuesData
            {
                Name = property.propertyName,
                ValueSO = property.propertyValue
            };
        
            data.InteractableValueList.Add(new InteractableValues
            {
                PropertyValues = propertyData
            });   
        }
    }

    public static void AddTransformValues(InteractableData data, Vector3 position, Quaternion rotation)
    {
        InteractableValues.TransformValuesData transformData = new InteractableValues.TransformValuesData
        {
            Rotation = rotation,
            Position = position
        };
        
        data.TransformValues = transformData;
    }

    public static void SetData(IDataCollectable iDataCollectable,InteractableData data,Transform transform)
    {
        foreach (var value in data.InteractableValueList)
        {
            if (value.PropertyValues != null)
            {
                var field = iDataCollectable.GetType().GetField(value.PropertyValues.Name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (field != null)
                {
                    if (field.FieldType.IsEnum)
                    {
                        object enumValue = Enum.Parse(field.FieldType, value.PropertyValues.Value);
                        field.SetValue(iDataCollectable, enumValue);
                    }
                    else if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                    {
                        field.SetValue(iDataCollectable, value.PropertyValues.ValueSO);
                    }
                    else
                    {
                        object deserializedValue = Convert.ChangeType(value.PropertyValues.Value, field.FieldType,NumberFormatInfo.InvariantInfo);
                        field.SetValue(iDataCollectable, deserializedValue);
                    }
                }
            }

            if (data.TransformValues != null)
            {
                transform.position = data.TransformValues.Position;
                transform.rotation = data.TransformValues.Rotation;
            }
        }
    }
}
