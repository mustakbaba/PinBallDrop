using System;
using System.Collections;
using System.Collections.Generic;
using SincappStudio;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoSingleton<LevelManager>
{
    #region Prefabs

    public GameObject MarbleObjPrefab;

    #endregion

    public int LoadLevelID { get; set; }
    public Color[] ObjectColors;
    public bool IsTimePuzzleActive => _timePuzzle;
    public float DurationSeconds => _durationSeconds;
    public LineBoxController LineBoxPrefab;
    public SmallBallController SmallBallPrefab;

    public BumperController BumperControllerPrefab;
    public BumperHolderController BumperHolderPrefab;
    public LineBoxConnectorController ConnectorPrefab;
    [SerializeField] private bool _timePuzzle;

    public int[] NewFeatureUnlockLevels { get; set; } = { 1, 5, 10, 20, 30 };
    public float FillAddAmountEachLevel { get; set; }
    public Material HalfHalfMaterial;
    public Material SingleMaterial;

    [ShowIf("_timePuzzle")] [SerializeField]
    private float _durationSeconds = 300f;

    private bool _isGameFinished;

    protected override void Awake()
    {
        var maxLevelCount = Resources.LoadAll<LevelData>("Levels").Length;
        var currentLevel = PersistData.Instance.CurrentLevel;
        Time.timeScale = 1f;

        int loopStartLevel = RemoteController.Instance.LoopStartLevel;

        if (currentLevel < loopStartLevel)
        {
            LoadLevelID = currentLevel;
        }
        else
        {
            int loopLength = maxLevelCount - loopStartLevel + 1;
            int loopIndex = (currentLevel - loopStartLevel) % loopLength;
            LoadLevelID = loopStartLevel + loopIndex;
        }

        if (LoadLevelID > maxLevelCount)
        {
            LoadLevelID = maxLevelCount;
        }

        var levelData = Resources.Load<LevelData>($"Levels/Level_{LoadLevelID:D3}");

        if (levelData.LevelDifficulty == LevelData.LevelDifficulties.Hard)
        {
            InGameUIManager.Instance.HardLevelSplash();
        }
        // LoadLevel(levelData);

        var currentBlockerLevel =
            NewFeatureUnlockLevels[PersistData.Instance.CurrentBlockerIndex];
        var nextBlockerLevel = LevelManager.Instance.NewFeatureUnlockLevels[
            Mathf.Min(NewFeatureUnlockLevels.Length - 1,
                PersistData.Instance.CurrentBlockerIndex + 1)];
        FillAddAmountEachLevel = 100f / (nextBlockerLevel - currentBlockerLevel) / 100f;
        Time.timeScale = 1;
    }

    private void OnEnable()
    {
        EventManager.OnGameStart += GameStarted;
        EventManager.OnGameWin += StopTimePuzzle;
        EventManager.OnGameLose += StopTimePuzzle;
    }

    private void OnDisable()
    {
        EventManager.OnGameStart -= GameStarted;
        EventManager.OnGameWin -= StopTimePuzzle;
        EventManager.OnGameLose -= StopTimePuzzle;
    }

    private void GameStarted()
    {
        if (_timePuzzle)
        {
            StartCoroutine(StartTimePuzzle());
        }
    }


    public void Save(LevelData levelToSave)
    {
        levelToSave.cells = new List<CellInfo>();
        levelToSave.lineObjects = new List<LineInfo>();

        #region JamData

        var allCells = FindObjectsOfType<CellController>();
        var allLineQueue = FindObjectsOfType<LineBoxHolderManager>();

        foreach (var lineHolder in allLineQueue)
        {
            var info = new LineInfo();
            info.TargetBoxes = lineHolder.TargetObjects;
            levelToSave.lineObjects.Add(info);
        }


        foreach (var cell in allCells)
        {
            var info = new CellInfo();
            info.CellType = cell.CellType;
            info.ObjColor = cell.objectColor;
            info.BlockerType = cell.BlockerType;
            info.BlockerLockAmount = cell.BlockerLockAmount;
            info.GridPos = new Vector2Int(cell.Xpos, cell.Zpos);
            levelToSave.cells.Add(info);
        }

        #endregion
    }


    public void LoadLevel(LevelData level)
    {
        var allCells = FindObjectsOfType<CellController>();
        var allLineQueue = FindObjectsOfType<LineBoxHolderManager>();

        InGameUIManager.Instance.SetLevelDifficulty(level.LevelDifficulty);

        for (var i = 0; i < level.lineObjects.Count; i++)
        {
            var info = level.lineObjects[i];
            allLineQueue[i].TargetObjects = new List<TargetBoxData>();
            allLineQueue[i].TargetObjects = info.TargetBoxes;
            allLineQueue[i].SpawnPrefabs();
        }

        foreach (var info in level.cells)
        {
            foreach (var cell in allCells)
            {
                if (new Vector2Int(cell.Xpos, cell.Zpos) == info.GridPos)
                {
                    cell.CellType = info.CellType;
                    cell.objectColor = info.ObjColor;
                    cell.BlockerType = info.BlockerType;
                    cell.BlockerLockAmount = info.BlockerLockAmount;
                    cell.SpawnPrefabs();
                }
            }
        }
    }


    private void StopTimePuzzle()
    {
        if (_timePuzzle)
        {
            _isGameFinished = true;
        }
    }

    private IEnumerator StartTimePuzzle()
    {
        while (_durationSeconds > 0 && !_isGameFinished)
        {
            _durationSeconds -= Time.deltaTime;

            if (_durationSeconds <= 0)
            {
                EventManager.OnGameLose?.Invoke();
                _durationSeconds = 0;
                yield break;
            }

            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            EventManager.OnGameWin?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            EventManager.OnGameLose?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 3;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
}