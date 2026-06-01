using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEditor;

public class TunnelSpawnerController : MonoBehaviour
{
    [SerializeField] private List<BallProperties> _ballDatas = new List<BallProperties>();
    [SerializeField] private Transform _spawnPoint;      // tünel ağzı
    [SerializeField] private MeshRenderer _tunnelRenderer; // tünelin mesh'i
    [SerializeField] private int _nextColorMaterialIndex = 1; // 2. topun rengi bu indexe gidecek

    private int _currentIndex = 0;
    private BallController _activeBall;

    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (!gameObject.scene.IsValid()) return;
        if (_ballDatas == null || _ballDatas.Count == 0) return;
        if (_spawnPoint == null) return;

        EditorApplication.delayCall -= ShowPreview;
        EditorApplication.delayCall += ShowPreview;
    }

    private void ShowPreview()
    {
        if (this == null) return;

        // Önceki preview'ı temizle
        var existing = GetComponentsInChildren<Transform>(true)
            .Where(t => t.name == "TunnelPreview")
            .Select(t => t.gameObject)
            .ToList();
        foreach (var go in existing)
            DestroyImmediate(go);

        if (_ballDatas == null || _ballDatas.Count == 0) return;
        if (_spawnPoint == null) return;

        var data = _ballDatas[0];
        var ball = PrefabUtility.InstantiatePrefab(LevelManager.Instance.BallControllerPrefab, transform) as BallController;
        ball.transform.position = _spawnPoint.position;
        ball.name = "TunnelPreview";
        ball.IsFromTunnel = true;

        ball.Properties = data;
        ball.SetColor();
        ball.transform.localScale = Vector3.one * 1.2f;

        EditorUtility.SetDirty(this);
    }
#endif

    private void Start()
    {
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

    private void SpawnCurrent()
    {
        if (_currentIndex >= _ballDatas.Count) return;

        var data = _ballDatas[_currentIndex];
        var ball = Instantiate(LevelManager.Instance.BallControllerPrefab, _spawnPoint.position, Quaternion.identity);
        ball.transform.rotation = Quaternion.Euler(-90, 0, 0);

        ball.Properties = data;
        ball.SetColor();
        ball.IsFromTunnel = true;
        ball.GetComponent<Rigidbody>().isKinematic = true;
        // Sıfırdan şişerek gel
        ball.transform.localScale = Vector3.zero;
        ball.transform.position = _spawnPoint.parent.position;
        ball.transform.DOMove(_spawnPoint.position, .5f);
        ball.transform.DOScale(Vector3.one * .89f, 0.4f)
            .SetDelay(0.1f);

        _activeBall = ball;
        ball.OnExploded = OnBallExploded;
    }

    private void OnBallExploded()
    {
        _currentIndex++;

        if (_currentIndex >= _ballDatas.Count)
        {
            // Tünel bitti, next color materyali temizle
            ClearTunnelColor();
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
        if (nextIndex >= _ballDatas.Count)
        {
            ClearTunnelColor();
            return;
        }

        var nextData = _ballDatas[nextIndex];
        var color = LevelManager.Instance.ObjectColors[(int)nextData.ObjectColor];

        var mats = _tunnelRenderer.materials;
        var propBlock = new MaterialPropertyBlock();
        _tunnelRenderer.GetPropertyBlock(propBlock, _nextColorMaterialIndex);
        propBlock.SetColor("_BaseColor", color);
        _tunnelRenderer.SetPropertyBlock(propBlock, _nextColorMaterialIndex);
    }

    private void ClearTunnelColor()
    {
        var propBlock = new MaterialPropertyBlock();
        _tunnelRenderer.GetPropertyBlock(propBlock, _nextColorMaterialIndex);
        propBlock.SetColor("_BaseColor", Color.gray);
        _tunnelRenderer.SetPropertyBlock(propBlock, _nextColorMaterialIndex);
    }
}