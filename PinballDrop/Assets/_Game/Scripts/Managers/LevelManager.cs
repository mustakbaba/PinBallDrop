using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
    public BallController BallControllerPrefab;
    public SmallBallController SmallBallPrefab;
    public GameObject FakeBumperPrefab;
    public TunnelSpawnerController TunnelPrefab;
    public BumperController BumperControllerPrefab;
    public BumperHolderController BumperHolderPrefab;
    public LineBoxConnectorController ConnectorPrefab;
    [SerializeField] private bool _timePuzzle;

    public int[] NewFeatureUnlockLevels { get; set; } = { 1, 10, 20, 30 };
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

        if (Application.isEditor)
        {
            // LoadLevel(levelData);
        }
        else
        {
            LoadLevel(levelData);
        }

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

        #region JamData

        var allCells = FindObjectsOfType<CellController>();


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

        levelToSave.BumperData = new List<BumperDatas>();
        var allBumpers = FindObjectsOfType<BumperHolderController>();

        foreach (var bumper in allBumpers)
        {
            var bumperData = new BumperDatas();

            foreach (var target in bumper.TargetObjects)
            {
                var data = new BumperData
                {
                    IsHidden = target.IsHidden,
                    Color = target.Color,
                    Amount = target.Amount
                };

                bumperData.TargetObjects.Add(data);
            }

            levelToSave.BumperData.Add(bumperData);
        }

        var ballProperties = new List<BallProperties>();
        var allBalls = FindObjectsOfType<BallController>();

        foreach (var ball in allBalls)
        {
            if (ball.IsFromTunnel) continue;
            
            var data = new BallProperties
            {
                BallBlocker = ball.Properties.BallBlocker,
                MultiColor = ball.Properties.MultiColor,
                IsIce = ball.Properties.IsIce,
                IceAmount = ball.Properties.IceAmount,
                MultiAmount = ball.Properties.MultiAmount,
                BallAmount = ball.Properties.BallAmount,
                ObjectColor = ball.Properties.ObjectColor,
                IsHidden = ball.Properties.IsHidden,
                Position = ball.transform.position
            };

            ballProperties.Add(data);
        }

        levelToSave.BallData = ballProperties;

        var tunnels = FindObjectsOfType<TunnelSpawnerController>();

        levelToSave.TunnelData = new List<TunnelData>();

        foreach (var tunnel in tunnels)
        {
            var tunnelData = new TunnelData
            {
                BallDatas = tunnel.BallDatas,
                SpawnPoint = tunnel.transform.position,
                SpawnRotation = tunnel.transform.rotation.eulerAngles
            };

            levelToSave.TunnelData.Add(tunnelData);
        }
    }

    public void LoadLevel(LevelData level)
    {
        var allCells = FindObjectsOfType<CellController>();

        InGameUIManager.Instance.SetLevelDifficulty(level.LevelDifficulty);

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

        var allBumpers = FindObjectsOfType<BumperHolderController>();

        for (int i = 0; i < level.BumperData.Count; i++)
        {
            if (i < allBumpers.Length)
            {
                var bumperData = level.BumperData[i];
                var bumper = allBumpers[i];
                bumper.TargetObjects = bumperData.TargetObjects;
                bumper.SpawnPrefabs();
            }
        }

        var allBalls = FindObjectsOfType<BallController>();

        var toDestroy = allBalls.ToList();

        for (int i = toDestroy.Count - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(toDestroy[i].gameObject);
            else
                DestroyImmediate(toDestroy[i].gameObject);
        }

        for (int i = 0; i < level.BallData.Count; i++)
        {
            var ballData = level.BallData[i];
            var ball = Instantiate(BallControllerPrefab, Vector3.zero, Quaternion.Euler(-90, 0, 0));
            ball.Properties = ballData;
            ball.transform.position = ballData.Position;
            ball.SetColor(false);
        }

        var tunnels = FindObjectsOfType<TunnelSpawnerController>();
        
        var toDestroyTunnels = tunnels.ToList();
        
        for (int i = toDestroyTunnels.Count - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(toDestroyTunnels[i].gameObject);
            else
                DestroyImmediate(toDestroyTunnels[i].gameObject);
        }
        
        for (int i = 0; i < level.TunnelData.Count; i++)
        {
            var tunnelData = level.TunnelData[i];
            var tunnelObj = Instantiate(TunnelPrefab, tunnelData.SpawnPoint, Quaternion.Euler(tunnelData.SpawnRotation));
            tunnelObj.BallDatas.AddRange(tunnelData.BallDatas);
            tunnelObj.ShowPreview();
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