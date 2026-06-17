using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BallSpawner : MonoBehaviour
{
    [Header("Spawn Alanı")]
    [SerializeField] private Transform _planeArea; // Plane'in transform'u (boyutunu buradan alıcağız)
    [SerializeField] private float _zPosition = 0f; // Sabit Z pozisyonu

    [Header("Random Ball Amount Ayarları")]
    [InfoBox("Her olasılık 0-1 arası olmalı, toplamları otomatik normalize edilir.")]
    [SerializeField] private List<BallAmountWeight> _amountWeights = new List<BallAmountWeight>
    {
        new BallAmountWeight { Amount = 10, Weight = 0.1f },
        new BallAmountWeight { Amount = 20, Weight = 0.8f },
        new BallAmountWeight { Amount = 30, Weight = 0.1f },
    };

    [Header("Boyut Ayarları (BallController.SetColor ile aynı mantık)")]
    [SerializeField] private float _minScale = 0.55f;
    [SerializeField] private float _maxScale = 1.325f;
    [SerializeField] private int _minAmountForScale = 5;
    [SerializeField] private int _maxAmountForScale = 30;
    [SerializeField] private float _bigBallThresholdAmount = 70f; // 70+ ise sabit 2f scale
    [SerializeField] private float _bigBallScale = 2f;

    [Header("Spawn Sıkılığı")]
    [Tooltip("Toplar arası ekstra boşluk çarpanı. 1 = tam dip dibe, 1.05 = %5 boşluk")]
    [SerializeField] private float _spacingMultiplier = 1.0f;

    [Header("Renkler")]
    [SerializeField] private List<ColorTypes> _availableColors = new List<ColorTypes>();

    private float _planeWidth;
    private float _planeHeight;
    private Vector3 _planeCenter;

    [Button("Spawn Balls", ButtonSizes.Large)]
    public void SpawnBalls()
    {
        ClearExisting();

        if (_planeArea == null)
        {
            Debug.LogError("Plane area atanmadı!");
            return;
        }

        CalculatePlaneBounds();

        // Plane'in en üst satırından başlayıp aşağı doğru sıra sıra dolduracağız
        float cursorY = _planeCenter.y + _planeHeight * 0.5f;
        float minY = _planeCenter.y - _planeHeight * 0.5f;

        int safetyCounter = 0;
        int maxIterations = 5000;

        while (cursorY > minY && safetyCounter < maxIterations)
        {
            safetyCounter++;

            // Bu satırı soldan sağa dolduralım, satırın en büyük topuna göre satır yüksekliğini belirleyeceğiz
            float cursorX = _planeCenter.x - _planeWidth * 0.5f;
            float maxX = _planeCenter.x + _planeWidth * 0.5f;

            float rowTallestRadius = 0f;
            bool placedAnyInRow = false;

            int rowSafety = 0;
            while (cursorX < maxX && rowSafety < maxIterations)
            {
                rowSafety++;

                int amount = RollRandomAmount();
                float scale = GetScaleForAmount(amount);
                float radius = GetBallRadius(scale);

                // Bu top satıra sığıyor mu? (X ekseninde)
                if (cursorX + radius * 2f * _spacingMultiplier > maxX && placedAnyInRow)
                {
                    // Sığmıyor, satırı bitir
                    break;
                }

                Vector3 spawnPos = new Vector3(
                    cursorX + radius * _spacingMultiplier,
                    cursorY - radius * _spacingMultiplier,
                    _zPosition
                );

                SpawnSingleBall(spawnPos, scale, amount);

                cursorX += radius * 2f * _spacingMultiplier;
                rowTallestRadius = Mathf.Max(rowTallestRadius, radius);
                placedAnyInRow = true;
            }

            if (!placedAnyInRow)
            {
                // Satıra hiç top sığmadıysa (plane çok dar), sonsuz döngüyü engelle
                break;
            }

            cursorY -= rowTallestRadius * 2f * _spacingMultiplier;
        }

        Debug.Log($"✅ Spawn tamamlandı, toplam {transform.childCount} top spawn edildi.");
    }

    [Button("Clear Existing", ButtonSizes.Medium)]
    public void ClearExisting()
    {
        var children = new List<Transform>();
        foreach (Transform child in transform)
            children.Add(child);

        foreach (var child in children)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    private void CalculatePlaneBounds()
    {
        // Standart Unity Plane mesh'i 10x10 birimdir (localScale 1 iken)
        // Eğer farklı bir mesh kullanıyorsan, Renderer.bounds ile gerçek boyutu alalım
        var renderer = _planeArea.GetComponent<Renderer>();
        if (renderer != null)
        {
            var bounds = renderer.bounds;
            _planeWidth = bounds.size.x;
            _planeHeight = bounds.size.y; // Eğer plane XZ düzleminde duruyorsa burayı size.z yapman gerekebilir
            _planeCenter = bounds.center;
        }
        else
        {
            // Fallback: localScale * 10 (standart plane)
            _planeWidth = _planeArea.localScale.x * 10f;
            _planeHeight = _planeArea.localScale.y * 10f;
            _planeCenter = _planeArea.position;
        }
    }

    private int RollRandomAmount()
    {
        float totalWeight = 0f;
        foreach (var w in _amountWeights)
            totalWeight += w.Weight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var w in _amountWeights)
        {
            cumulative += w.Weight;
            if (rand <= cumulative)
                return w.Amount;
        }

        return _amountWeights[_amountWeights.Count - 1].Amount;
    }

    private float GetScaleForAmount(int amount)
    {
        if (amount >= _bigBallThresholdAmount)
            return _bigBallScale;

        float a = Mathf.InverseLerp(_minAmountForScale, _maxAmountForScale, amount);
        return Mathf.Lerp(_minScale, _maxScale, a);
    }

    private float GetBallRadius(float scale)
    {
        // BallController prefab'ının taban boyutu 1 birim çap kabul ediyoruz (localScale = çap)
        // Eğer prefab'ın gerçek mesh boyutu farklıysa burayı ayarlaman gerekir
        return scale * 0.5f;
    }

    private void SpawnSingleBall(Vector3 position, float scale, int amount)
    {
        var prefab = LevelManager.Instance.BallControllerPrefab;
        var ball = Instantiate(prefab, position, Quaternion.Euler(-90, 0, 0), transform);

        ColorTypes color = _availableColors.Count > 0
            ? _availableColors[Random.Range(0, _availableColors.Count)]
            : ColorTypes.Red;

        ball.Properties = new BallProperties
        {
            ObjectColor = color,
            BallAmount = amount,
            BallBlocker = BallController.BallBlockers.None,
        };

        ball.transform.localScale = Vector3.one * scale;
        ball.SetColor(true); // isPlaying=true → scale'i tekrar override etmesin
        ball.transform.localScale = Vector3.one * scale; // garanti olsun
    }
}

[System.Serializable]
public class BallAmountWeight
{
    public int Amount;
    [Range(0f, 1f)] public float Weight;
}