using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SincappStudio;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;

public class BallController : MonoBehaviour
{
    public System.Action OnExploded;

    public enum BallBlockers
    {
        None,
        MultiBall,
    }

    public BallBlockers BallBlocker;

    [ShowIf("BallBlocker", BallBlockers.MultiBall)]
    public ColorTypes MultiColor;

    [ShowIf("BallBlocker", BallBlockers.MultiBall)]
    public int MultiAmount = 5;

    [Header("Ayarlar")] private float upwardForce = 12;
    private float maxY = 899f;
    private float smallBallSpeed = 4f;

    private Rigidbody _rb;
    private bool _isClickable;
    private bool _exploded;

    public int BallAmount = 10;
    public ColorTypes ObjectColor;
    private MaterialPropertyBlock _propertyBlock;
    [SerializeField] private TextMeshPro _amountText;
    [SerializeField] private TextMeshPro _multiAmountText;
    [SerializeField] private GameObject _innerBallObject;
    private MeshRenderer _meshRenderer;
    public bool IsHidden;
    public bool IsFromTunnel { get; set; }

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _rb = GetComponent<Rigidbody>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        StartCoroutine(Sincapp.WaitAndAction(0, () =>
        {
            if (Application.isPlaying) return;
            EditorApplication.delayCall += ValidateBall;
        }));
    }
#endif

    private void ValidateBall() => SetColor();

    public void SetColor(bool isPlaying = false)
    {
        if (this == null) return;
        var color = LevelManager.Instance.ObjectColors[(int)ObjectColor];

        var renderer = GetComponentInChildren<MeshRenderer>();
        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_propertyBlock);

        if (BallBlocker == BallBlockers.MultiBall)
        {
            // HalfHalf materyale geç
            _multiAmountText.gameObject.SetActive(true);
            _amountText.transform.localScale = Vector3.one * 0.75f;
            _multiAmountText.transform.localScale = Vector3.one * 0.75f;
            _amountText.transform.localPosition = Vector3.zero + Vector3.up * .55f + Vector3.left * .25f;
            _multiAmountText.transform.localPosition = Vector3.zero + Vector3.up * .55f + Vector3.right * .25f;

            renderer.material = LevelManager.Instance.HalfHalfMaterial;
            _propertyBlock.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)ObjectColor]);
            _propertyBlock.SetColor("_BaseColor2", LevelManager.Instance.ObjectColors[(int)MultiColor]);
            renderer.SetPropertyBlock(_propertyBlock);
        }
        else
        {
            _amountText.transform.localScale = Vector3.one;
            _multiAmountText.gameObject.SetActive(false);
            _amountText.transform.localPosition = Vector3.zero + Vector3.up * .55f;
            renderer.material = LevelManager.Instance.SingleMaterial;
            _propertyBlock.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)ObjectColor]);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        var a = Mathf.InverseLerp(5, 20, BallAmount);
        var scale = Mathf.Lerp(0.5f, 2f, a);
        if (!isPlaying && !IsFromTunnel)
            transform.localScale = Vector3.one * scale;

        _amountText.text = BallAmount.ToString();

        if (Application.isPlaying)
            renderer.materials[0].color = color;

        if (IsHidden)
        {
            _amountText.text = "?";
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.3f) * .5f);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        // Multi ball görsel
        bool isMulti = BallBlocker == BallBlockers.MultiBall;
        if (_innerBallObject != null)
            _innerBallObject.SetActive(isMulti);

        if (isMulti && _innerBallObject != null)
        {
            var innerRenderer = _innerBallObject.GetComponent<MeshRenderer>();
            if (innerRenderer != null)
            {
                if (_propertyBlock == null)
                    _propertyBlock = new MaterialPropertyBlock();
                innerRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)MultiColor]);
                innerRenderer.SetPropertyBlock(_propertyBlock);

                if (Application.isPlaying)
                    innerRenderer.materials[0].color = LevelManager.Instance.ObjectColors[(int)MultiColor];
            }
        }

        if (_multiAmountText != null)
        {
            _multiAmountText.gameObject.SetActive(isMulti);
            if (isMulti)
                _multiAmountText.text = MultiAmount.ToString();
        }
    }

    private void FixedUpdate()
    {
        if (_exploded) return;
        _rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);

        if (transform.position.y >= maxY)
        {
            var vel = _rb.velocity;
            vel.y = Mathf.Min(vel.y, 0f);
            _rb.velocity = vel;
        }
    }

    private void Update()
    {
        if (_exploded) return;
        CheckClickable();
    }

  private void CheckClickable()
{
    float sideOffset = transform.localScale.x * 0.25f;
    float horizontalRayLength = transform.localScale.x * 1f;

    Vector3 centerOrigin = transform.position;
    Vector3 leftOrigin = transform.position + Vector3.left * sideOffset;
    Vector3 rightOrigin = transform.position + Vector3.right * sideOffset;
    Vector3 horizontalOrigin = transform.position + Vector3.down * 0.1f;

    int mask = LayerMask.GetMask("Collectable", "Obstacle");

    bool centerBlocked = Physics.Raycast(centerOrigin, Vector3.down, 3f, mask);
    bool leftBlocked = Physics.Raycast(leftOrigin, Vector3.down, 3f, mask);
    bool rightBlocked = Physics.Raycast(rightOrigin, Vector3.down, 3f, mask);
    bool leftHBlocked = Physics.Raycast(horizontalOrigin, Vector3.left, horizontalRayLength, mask);
    bool rightHBlocked = Physics.Raycast(horizontalOrigin, Vector3.right, horizontalRayLength, mask);

    _isClickable = !centerBlocked || !leftBlocked || !rightBlocked || !leftHBlocked || !rightHBlocked;

#if UNITY_EDITOR
    Debug.DrawLine(centerOrigin, centerOrigin + Vector3.down * 3f, centerBlocked ? Color.red : Color.green);
    Debug.DrawLine(leftOrigin, leftOrigin + Vector3.down * 3f, leftBlocked ? Color.red : Color.green);
    Debug.DrawLine(rightOrigin, rightOrigin + Vector3.down * 3f, rightBlocked ? Color.red : Color.green);
    Debug.DrawLine(horizontalOrigin, horizontalOrigin + Vector3.left * horizontalRayLength, leftHBlocked ? Color.red : Color.cyan);
    Debug.DrawLine(horizontalOrigin, horizontalOrigin + Vector3.right * horizontalRayLength, rightHBlocked ? Color.red : Color.cyan);
#endif

    if (_isClickable)
    {
        _amountText.DOFade(1f, .1f);
        _meshRenderer.material.SetFloat("_OutlineWidth", 1);
        if (IsHidden)
        {
            IsHidden = false;
            _amountText.text = BallAmount.ToString();
            SetColor();
        }
    }
    else
    {
        _meshRenderer.material.SetFloat("_OutlineWidth", 0);
        _amountText.DOFade(.25f, .1f);
    }
}

    private void OnMouseDown()
    {
         if (!_isClickable || _exploded) return;
        Explode();
    }

    private void Explode()
    {
        _exploded = true;
        _rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(SpawnSmallBalls());
    }

    private IEnumerator SpawnSmallBalls()
    {
        _meshRenderer.enabled = false;
        _amountText.gameObject.SetActive(false);
        if (_innerBallObject != null) _innerBallObject.SetActive(false);
        if (_multiAmountText != null) _multiAmountText.gameObject.SetActive(false);

        var capacityManager = AreaCapacityManager.Instance;
        int available = capacityManager.CapacityAmount - capacityManager.CurrentAmount;

        if (available <= 0)
        {
            // Hiç yer yok, geri döndür
            RestoreBall();
            yield break;
        }

        int totalNeeded = BallBlocker == BallBlockers.MultiBall ? BallAmount + MultiAmount : BallAmount;

        if (available >= totalNeeded)
        {
            // Hepsi sığıyor, normal patlat
            yield return StartCoroutine(SpawnBatch(BallAmount, ObjectColor, capacityManager));
            if (BallBlocker == BallBlockers.MultiBall)
                yield return StartCoroutine(SpawnBatch(MultiAmount, MultiColor, capacityManager));
            
            OnExploded?.Invoke();
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }
        else
        {
            // Kısmen sığıyor — orantılı dağıt
            int mainSpawn, multiSpawn;

            if (BallBlocker == BallBlockers.MultiBall)
            {
                // Orantılı böl
                float ratio = (float)BallAmount / totalNeeded;
                mainSpawn = Mathf.FloorToInt(available * ratio);
                multiSpawn = available - mainSpawn;
            }
            else
            {
                mainSpawn = available;
                multiSpawn = 0;
            }

            yield return StartCoroutine(SpawnBatch(mainSpawn, ObjectColor, capacityManager));
            if (BallBlocker == BallBlockers.MultiBall && multiSpawn > 0)
                yield return StartCoroutine(SpawnBatch(multiSpawn, MultiColor, capacityManager));

            // Kalanları hesapla
            BallAmount -= mainSpawn;
            if (BallBlocker == BallBlockers.MultiBall)
                MultiAmount -= multiSpawn;

            // Sıfırlandıysa yok et
            bool mainEmpty = BallAmount <= 0;
            bool multiEmpty = BallBlocker != BallBlockers.MultiBall || MultiAmount <= 0;

            if (mainEmpty && multiEmpty)
            {
                OnExploded?.Invoke();
                yield return new WaitForSeconds(0.1f);
                Destroy(gameObject);
            }
            else
            {
                // Kalan miktarla geri döndür
                if (mainEmpty && BallBlocker == BallBlockers.MultiBall)
                {
                    // Sadece multi kaldı, rengi güncelle
                    ObjectColor = MultiColor;
                    BallAmount = MultiAmount;
                    BallBlocker = BallBlockers.None;
                }

                RestoreBall();
            }
        }
    }

    private void RestoreBall()
    {
        _exploded = false;
        _rb.isKinematic = false;
        GetComponent<Collider>().enabled = true;
        _meshRenderer.enabled = true;
        _amountText.gameObject.SetActive(true);

        var a = Mathf.InverseLerp(5, 20, BallAmount);
        var scale = Mathf.Lerp(0.5f, 2f, a);
        SetColor(true);
        transform.DOScale(Vector3.one * scale, .2f);
    }

    private IEnumerator SpawnBatch(int amount, ColorTypes color, AreaCapacityManager capacityManager)
    {
        float radius = transform.localScale.x * 0.5f;
        Vector3 center = transform.position;

        for (int i = 0; i < amount; i++)
        {
            float angle = i * 137.5f * Mathf.Deg2Rad;
            float r = radius * Mathf.Sqrt((float)i / Mathf.Max(amount, 1));

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * r,
                Mathf.Sin(angle) * r,
                0f
            );

            var small = Instantiate(LevelManager.Instance.SmallBallPrefab, center + offset, Quaternion.identity);
            var rb = small.GetComponent<Rigidbody>();
            small.SetColor(color);

            if (rb != null)
            {
                Vector3 dir = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(-0.3f, 0.3f),
                    -1f
                ).normalized;
                rb.AddForce(dir * smallBallSpeed, ForceMode.Impulse);
            }
        }

        capacityManager.SetAmount(amount);
        yield return null;
    }
}