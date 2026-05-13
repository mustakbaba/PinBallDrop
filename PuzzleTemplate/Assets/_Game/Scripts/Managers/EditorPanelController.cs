using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditorPanelController : MonoBehaviour
{
    [SerializeField] private Button _nextLevelButton,
        _previousLevelButton,
        _openPanelButton;

    [SerializeField] private TMP_InputField levelInput;

    private void Awake()
    {
        _openPanelButton.gameObject.SetActive(RemoteController.Instance.IsDebugModeEnabled);
    }

    private void OnEnable()
    {
        _nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        _previousLevelButton.onClick.AddListener(OnPreviousLevelButtonClicked);
    }

    private void OnDisable()
    {
        _nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        _previousLevelButton.onClick.AddListener(OnPreviousLevelButtonClicked);
    }

    private void OnNextLevelButtonClicked()
    {
        GameManager.Instance.NextLevel();
    }

    private void OnPreviousLevelButtonClicked()
    {
        GameManager.Instance.PreviousLevel();
    }

    public void LoadLevel()
    {
        // Input boşsa hiçbir şey yapma
        if (string.IsNullOrEmpty(levelInput.text))
            return;

        // Sayıya çevrilebiliyorsa
        if (int.TryParse(levelInput.text, out int levelIndex))
        {
            // İstersen 1 yazınca Level1 yüklensin diye -1 yapabilirsin
            // levelIndex--;

            if (levelIndex >= 0)
            {
                PersistData.Instance.CurrentLevel = levelIndex;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}