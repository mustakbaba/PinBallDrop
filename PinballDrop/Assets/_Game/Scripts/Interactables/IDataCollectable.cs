using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataCollectable
{
  InteractableData GetInteractableData();
  GameObject GetGameObjectReference();
}
