using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    public List<CellInfo> cells = new List<CellInfo>();
    public List<BumperDatas> BumperData = new List<BumperDatas>();
    public List<BallProperties> BallData = new List<BallProperties>();
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

public class BumperDatas
{
    public List<BumperData> TargetObjects = new List<BumperData>();
}
