using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    private bool _isGameStarted;

    protected override void Awake()
    {
        base.Awake();
        Input.multiTouchEnabled = false;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (_isGameStarted) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        EventManager.OnGameStart?.Invoke();
                        _isGameStarted = true;
                        Elephant.LevelStarted(PersistData.Instance.CurrentLevel,
                            LevelManager.Instance.LoadLevelID.ToString());
                    }
                }
            }
            else
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    EventManager.OnGameStart?.Invoke();
                    _isGameStarted = true;

                    if (!Application.isEditor)
                    {
                        Elephant.LevelStarted(PersistData.Instance.CurrentLevel,
                            LevelManager.Instance.LoadLevelID.ToString());
                    }
                }
            }
        }
    }

    public void RestartLevel()
    {
        var persistData = PersistData.Instance;
        if (!Application.isEditor)
        {
            Elephant.LevelFailed(persistData.CurrentLevel, LevelManager.Instance.LoadLevelID.ToString());
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        persistData.Save();
    }

    public void NextLevel()
    {
        var persistData = PersistData.Instance;

        if (!Application.isEditor)
        {
            Elephant.LevelCompleted(persistData.CurrentLevel, LevelManager.Instance.LoadLevelID.ToString());
        }

        if (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1)
        {
            persistData.CurrentLevel++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            persistData.CurrentLevel++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        persistData.Save();
    }

    
public void PreviousLevel()
    {
        var persistData = PersistData.Instance;
        
        if (persistData.CurrentLevel > 1)
        {
            persistData.CurrentLevel--;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("Already at the first level.");
        }

        persistData.Save();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            //PersistData.Instance.Save();
        }
    }

    private void OnApplicationQuit()
    {
        //PersistData.Instance.Save();
    }
}