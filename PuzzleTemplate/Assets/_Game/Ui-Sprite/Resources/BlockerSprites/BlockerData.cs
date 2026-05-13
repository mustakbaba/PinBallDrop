using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockerData", menuName = "Blocker/BlockerData", order = 1)]
public class BlockerData : ScriptableObject
{
    public Sprite BlockerSprite;
    public string BlockerName;
    public string Description;
}
