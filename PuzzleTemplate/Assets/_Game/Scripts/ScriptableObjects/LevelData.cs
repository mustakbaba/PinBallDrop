using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    public List<CellInfo> cells = new List<CellInfo>();
    public List<LineInfo> lineObjects = new List<LineInfo>();
    
    public LevelDifficulties LevelDifficulty;

    public enum LevelDifficulties
    {
        Easy,
        Medium,
        Hard
    }
}

[System.Serializable]
public class CellInfo
{
    public CellController.CellTypes CellType;
    public ColorTypes ObjColor;
    public BlockerTypes BlockerType;
    public int BlockerLockAmount;
    public Vector2Int GridPos;
}

[System.Serializable]
public class LineInfo
{
    public List<TargetBoxData> TargetBoxes;
    
}