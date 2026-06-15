using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine.Serialization;

public class TunnelSpawnerController : MonoBehaviour
{
    public List<BallProperties> BallDatas = new List<BallProperties>();
    [SerializeField] private Transform _spawnPoint; // tünel ağzı
    [SerializeField] private MeshRenderer _tunnelRenderer; // tünelin mesh'i
    [SerializeField] private int _nextColorMaterialIndex = 1; // 2. topun rengi bu indexe gidecek
    [SerializeField] private TextMeshPro _tunnelBallAmountText;
    private int _currentBallAmount;

    private int _currentIndex = 0;
    private BallController _activeBall;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (!gameObject.scene.IsValid()) return;
        if (BallDatas == null || BallDatas.Count == 0) return;
        if (_spawnPoint == null) return;

        EditorApplication.delayCall -= ShowPreview;
        EditorApplication.delayCall += ShowPreview;
    }

    public void ShowPreview()
    {
        if (this == null) return;

        // Önceki preview'ı temizle
        var existing = GetComponentsInChildren<Transform>(true)
            .Where(t => t.name == "TunnelPreview")
            .Select(t => t.gameObject)
            .ToList();
        foreach (var go in existing)
            DestroyImmediate(go);

        if (BallDatas == null || BallDatas.Count == 0) return;
        if (_spawnPoint == null) return;

        var data = BallDatas[0];
        var ball =
            PrefabUtility.InstantiatePrefab(LevelManager.Instance.BallControllerPrefab, transform) as BallController;
        ball.transform.position = _spawnPoint.position;
        ball.name = "TunnelPreview";
        ball.IsFromTunnel = true;
        _currentBallAmount = BallDatas.Count;
        ball.Properties = data;
        ball.SetColor();
        ball.transform.localScale = Vector3.one * 1.2f;

        EditorUtility.SetDirty(this);
    }
#endif

    private void Start()
    {
        _tunnelBallAmountText.transform.LookAt(Camera.main.transform);
        _tunnelBallAmountText.transform.Rotate(0, 180, 0); // ters bakmasın
        _currentBallAmount = BallDatas.Count;
        SetTunnelText();
        // Editor preview'ını temizle
        var existing = GetComponentsInChildren<Transform>(true)
            .Where(t => t.name == "TunnelPreview")
            .Select(t => t.gameObject)
            .ToList();
        foreach (var go in existing)
            Destroy(go);

        SpawnCurrent();
        UpdateTunnelColor();
    }

    private void SetTunnelText()
    {
        _tunnelBallAmountText.text = (_currentBallAmount - 1).ToString();
    }

    private void SpawnCurrent()
    {
        if (_currentIndex >= BallDatas.Count) return;

        var data = BallDatas[_currentIndex];
        var ball = Instantiate(LevelManager.Instance.BallControllerPrefab, _spawnPoint.position, Quaternion.identity);
        ball.transform.rotation = Quaternion.Euler(-90, 0, 0);
        _currentBallAmount = BallDatas.Count - _currentIndex;

        SetTunnelText();
        ball.Properties = data;
        ball.SetColor();
        ball.IsFromTunnel = true;
        ball.GetComponent<Rigidbody>().isKinematic = true;
        // Sıfırdan şişerek gel
        ball.transform.localScale = Vector3.zero;
        ball.transform.position = _spawnPoint.parent.position;
        ball.transform.DOMove(_spawnPoint.position, .5f);

        transform.DOShakeRotation(0.4f, new Vector3(5f, 0, 0), 22, 90, false);
        transform.DOScale(.8f, .2f).SetLoops(2, LoopType.Yoyo);
        ball.transform.DOScale(Vector3.one * .89f, 0.4f)
            .SetDelay(0.1f);

        _activeBall = ball;
        ball.OnExploded = OnBallExploded;
    }

    private void OnBallExploded()
    {
        _currentIndex++;

        if (_currentIndex >= BallDatas.Count - 1)
        {
            _tunnelBallAmountText.gameObject.SetActive(false);
        }

        if (_currentIndex >= BallDatas.Count)
        {
            // Tünel bitti, next color materyali temizle
            ClearTunnelColor();
            _tunnelBallAmountText.gameObject.SetActive(false);
            return;
        }

        // Yeni topu spawn et
        SpawnCurrent();

        // Tünel rengini güncelle
        UpdateTunnelColor();
    }

    private void UpdateTunnelColor()
    {
        int nextIndex = _currentIndex + 1;
        if (nextIndex >= BallDatas.Count)
        {
            ClearTunnelColor();
            return;
        }

        var nextData = BallDatas[nextIndex];
        var color = LevelManager.Instance.ObjectColors[(int)nextData.ObjectColor];

        var mats = _tunnelRenderer.materials;
        var propBlock = new MaterialPropertyBlock();
        var propBlock2 = new MaterialPropertyBlock();
        _tunnelRenderer.GetPropertyBlock(propBlock, _nextColorMaterialIndex);
        propBlock.SetColor("_BaseColor", color);
        _tunnelRenderer.SetPropertyBlock(propBlock, _nextColorMaterialIndex);
        if (nextData.BallBlocker == BallController.BallBlockers.MultiBall)
        {
            propBlock2.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)nextData.MultiColor]);
            _tunnelRenderer.SetPropertyBlock(propBlock2, 2);
        }
        else
        {
            _tunnelRenderer.SetPropertyBlock(propBlock, 2);
        }
    }

    private void ClearTunnelColor()
    {
        var propBlock = new MaterialPropertyBlock();
        _tunnelRenderer.GetPropertyBlock(propBlock, _nextColorMaterialIndex);
        propBlock.SetColor("_BaseColor", Color.gray);
        _tunnelRenderer.SetPropertyBlock(propBlock, _nextColorMaterialIndex);
        _tunnelRenderer.SetPropertyBlock(propBlock, 2);
    }
}