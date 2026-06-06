using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lofelt.NiceVibrations;
using SincappStudio;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using Random = UnityEngine.Random;

public class BallController : MonoBehaviour
{
    public System.Action OnExploded;
    public ColorTypes ObjectColor { get; set; }

    public enum BallBlockers
    {
        None,
        MultiBall,
    }

    public BallProperties Properties;

    [Header("Ayarlar")] private float upwardForce = 12;
    private float maxY = 899f;
    private float smallBallSpeed = 4f;

    private Rigidbody _rb;
    private bool _isClickable;
    private bool _exploded;

    private MaterialPropertyBlock _propertyBlock;

    [FoldoutGroup("References")] [SerializeField]
    private TextMeshPro _amountText;

    [FoldoutGroup("References")] [SerializeField]
    private TextMeshPro _multiAmountText;

    [FoldoutGroup("References")] [SerializeField]
    private GameObject _innerBallObject;

    private MeshRenderer _meshRenderer;
    public bool IsFromTunnel { get; set; }

    private float _clickableTimer = 0f;
    private const float ClickableDelay = 1f;
    private bool _isClickableConfirmed;
    private int _defIceAmount;
    private float _defIceScale;
    [SerializeField] private Transform _iceObjTransform;
    [SerializeField] private TextMeshPro _iceText;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        EventManager.OnBallExplode += OneBallExplode;
    }

    private void OnDisable()
    {
        EventManager.OnBallExplode -= OneBallExplode;
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
    private void Start()
    {
        _defIceAmount = Properties.IceAmount;
        _defIceScale = _iceObjTransform.localScale.x;
    }

    private void ValidateBall() => SetColor();

    public void SetColor(bool isPlaying = false)
    {
        if (this == null) return;
        if (Properties.BallBlocker != BallBlockers.MultiBall)
        {
            Properties.MultiAmount = 0;
        }

        if (Properties.IsIce)
        {
            _iceObjTransform.gameObject.SetActive(true);
            _amountText.gameObject.SetActive(false);
            _multiAmountText.gameObject.SetActive(false);
            _iceText.gameObject.SetActive(true);
            _iceText.SetText($"{Properties.IceAmount}");
        }
        else
        {
            _iceText.gameObject.SetActive(false);
            _amountText.gameObject.SetActive(true);
            _iceObjTransform.gameObject.SetActive(false);
        }

        var color = LevelManager.Instance.ObjectColors[(int)Properties.ObjectColor];
        ObjectColor = Properties.ObjectColor;
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_propertyBlock);
        var clr = LevelManager.Instance.ObjectColors[(int)Properties.ObjectColor];

        if (Properties.BallBlocker == BallBlockers.MultiBall)
        {
            // HalfHalf materyale geç
            _multiAmountText.gameObject.SetActive(true);
            _amountText.transform.localScale = Vector3.one * 0.75f;
            _multiAmountText.transform.localScale = Vector3.one * 0.75f;
            _amountText.transform.localPosition = Vector3.zero + Vector3.up * .55f + Vector3.right * .25f;
            _multiAmountText.transform.localPosition = Vector3.zero + Vector3.up * .55f + Vector3.left * .25f;

            renderer.material = LevelManager.Instance.HalfHalfMaterial;
            _propertyBlock.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)Properties.ObjectColor]);
            _propertyBlock.SetColor("_BaseColor2", LevelManager.Instance.ObjectColors[(int)Properties.MultiColor]);
            renderer.SetPropertyBlock(_propertyBlock);
        }
        else
        {
            _amountText.transform.localScale = Vector3.one;
            _multiAmountText.gameObject.SetActive(false);
            _amountText.transform.localPosition = Vector3.zero + Vector3.up * .55f + Vector3.forward * .1f;
            renderer.material = LevelManager.Instance.SingleMaterial;


            _propertyBlock.SetColor("_BaseColor", clr);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        clr *= 0.4f;
        clr.a = 1f;

        var multiColor = LevelManager.Instance.ObjectColors[(int)Properties.MultiColor];

        multiColor *= 0.4f;
        multiColor.a = 1f;

        _amountText.GetComponent<MeshRenderer>().GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor("_OutlineColor", clr);
        _amountText.GetComponent<MeshRenderer>().SetPropertyBlock(_propertyBlock);

        _multiAmountText.GetComponent<MeshRenderer>().GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor("_OutlineColor", multiColor);
        _multiAmountText.GetComponent<MeshRenderer>().SetPropertyBlock(_propertyBlock);

        var a = Mathf.InverseLerp(5, 30, Properties.BallAmount + Properties.MultiAmount);
        var scale = Mathf.Lerp(0.55f, 1.325f, a);

        if (!isPlaying && !IsFromTunnel)
        {
            transform.localScale = Vector3.one * scale;
            if (Properties.BallAmount + Properties.MultiAmount >= 70)
            {
                transform.localScale = Vector3.one * 2f;
            }
        }

        _amountText.text = Properties.BallAmount.ToString();

        if (Application.isPlaying)
            renderer.materials[0].color = color;

        if (Properties.IsHidden)
        {
            _amountText.text = "?";
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.3f) * .5f);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        // Multi ball görsel
        bool isMulti = Properties.BallBlocker == BallBlockers.MultiBall;
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
                _propertyBlock.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)Properties.MultiColor]);
                innerRenderer.SetPropertyBlock(_propertyBlock);

                if (Application.isPlaying)
                    innerRenderer.materials[0].color = LevelManager.Instance.ObjectColors[(int)Properties.MultiColor];
            }
        }

        if (_multiAmountText != null)
        {
            _multiAmountText.gameObject.SetActive(isMulti);
            if (isMulti)
                _multiAmountText.text = Properties.MultiAmount.ToString();
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

    private void OneBallExplode()
    {
        if (Properties.IsIce)
        {
            Properties.IceAmount--;
            var a = Mathf.InverseLerp(0, _defIceAmount, Properties.IceAmount);
            var mult = Mathf.Lerp(.8f, 1f,a);
            _iceObjTransform.DOScaleX(_defIceScale * mult, 0.05f);
            _iceObjTransform.DOScaleY(_defIceScale * mult, 0.05f);
            
            _iceText.SetText($"{Properties.IceAmount}");
            if (Properties.IceAmount <= 0)
            {
                Properties.IsIce = false;
                _iceText.gameObject.SetActive(false);
                ParticleManager.Instance.PlayIceBreakEffect(transform.position);
                SetColor();
            }
        }
    }

    private void CheckClickable()
    {
        float sideOffset = transform.localScale.x * 0.25f;
        float horizontalRayLength = transform.localScale.x * 1.75f;

        Vector3 centerOrigin = transform.position;
        Vector3 leftOrigin = transform.position + Vector3.left * sideOffset;
        Vector3 rightOrigin = transform.position + Vector3.right * sideOffset;
        Vector3 horizontalOrigin = transform.position + Vector3.down * 0.1f;

        int mask = LayerMask.GetMask("Collectable", "Obstacle");

        bool centerBlocked = Physics.Raycast(centerOrigin, Vector3.down, 3f, mask);
        bool leftBlocked = Physics.Raycast(leftOrigin, Vector3.down, 3f, mask);
        bool rightBlocked = Physics.Raycast(rightOrigin, Vector3.down, 3f, mask);

        // Yatay rayler — çarptığı noktanın ortasından aşağı kontrol
        bool leftHBlocked = false;
        bool rightHBlocked = false;

        RaycastHit leftHit, rightHit;

        if (Physics.Raycast(horizontalOrigin, Vector3.left, out leftHit, horizontalRayLength, mask))
        {
            Vector3 hitObjCenter = leftHit.collider.transform.position;
            Vector3 midPoint = (transform.position + hitObjCenter) * 0.5f;
            midPoint.y = transform.position.y;

            float spread = 0.035f;
            Vector3 midLeft = midPoint + Vector3.left * spread;
            Vector3 midRight = midPoint + Vector3.right * spread;

            bool mid1 = Physics.Raycast(midPoint, Vector3.down, 3f, mask);
            bool mid2 = Physics.Raycast(midLeft, Vector3.down, 3f, mask);
            bool mid3 = Physics.Raycast(midRight, Vector3.down, 3f, mask);

            leftHBlocked = mid1 || mid2 || mid3; // biri bile kapalıysa engelli

#if UNITY_EDITOR
            Debug.DrawLine(midPoint, midPoint + Vector3.down * 3f, mid1 ? Color.red : Color.yellow);
            Debug.DrawLine(midLeft, midLeft + Vector3.down * 3f, mid2 ? Color.red : Color.yellow);
            Debug.DrawLine(midRight, midRight + Vector3.down * 3f, mid3 ? Color.red : Color.yellow);
#endif
        }

        if (Physics.Raycast(horizontalOrigin, Vector3.right, out rightHit, horizontalRayLength, mask))
        {
            Vector3 hitObjCenter = rightHit.collider.transform.position;
            Vector3 midPoint = (transform.position + hitObjCenter) * 0.5f;
            midPoint.y = transform.position.y;

            float spread = 0.035f;
            Vector3 midLeft = midPoint + Vector3.left * spread;
            Vector3 midRight = midPoint + Vector3.right * spread;

            bool mid1 = Physics.Raycast(midPoint, Vector3.down, 3f, mask);
            bool mid2 = Physics.Raycast(midLeft, Vector3.down, 3f, mask);
            bool mid3 = Physics.Raycast(midRight, Vector3.down, 3f, mask);

            rightHBlocked = mid1 || mid2 || mid3; // biri bile kapalıysa engelli

#if UNITY_EDITOR
            Debug.DrawLine(midPoint, midPoint + Vector3.down * 3f, mid1 ? Color.red : Color.yellow);
            Debug.DrawLine(midLeft, midLeft + Vector3.down * 3f, mid2 ? Color.red : Color.yellow);
            Debug.DrawLine(midRight, midRight + Vector3.down * 3f, mid3 ? Color.red : Color.yellow);
#endif
        }

        bool leftHOpen = !leftHBlocked; // 3ü de açıksa true
        bool rightHOpen = !rightHBlocked; // 3ü de açıksa true

        _isClickable = !centerBlocked || !leftBlocked || !rightBlocked || leftHOpen || rightHOpen;

#if UNITY_EDITOR
        Debug.DrawLine(centerOrigin, centerOrigin + Vector3.down * 3f, centerBlocked ? Color.red : Color.green);
        Debug.DrawLine(leftOrigin, leftOrigin + Vector3.down * 3f, leftBlocked ? Color.red : Color.green);
        Debug.DrawLine(rightOrigin, rightOrigin + Vector3.down * 3f, rightBlocked ? Color.red : Color.green);
        Debug.DrawLine(horizontalOrigin, horizontalOrigin + Vector3.left * horizontalRayLength,
            leftHBlocked ? Color.red : Color.cyan);
        Debug.DrawLine(horizontalOrigin, horizontalOrigin + Vector3.right * horizontalRayLength,
            rightHBlocked ? Color.red : Color.cyan);
#endif

        bool rawClickable = !centerBlocked || !leftBlocked || !rightBlocked || leftHOpen || rightHOpen;

        if (!rawClickable)
        {
            // Direkt kapat
            _clickableTimer = 0f;
            _isClickable = false;
            _isClickableConfirmed = false;
        }
        else
        {
            // Timer'ı artır, 1sn dolunca aç
            _clickableTimer += Time.deltaTime;
            if (_clickableTimer >= ClickableDelay)
                _isClickable = true;
        }

        if (_isClickable)
        {
            _amountText.DOFade(1f, .1f);
            _multiAmountText.DOFade(1f, .1f);
            _meshRenderer.material.SetFloat("_OutlineWidth", 1);
            if (Properties.IsHidden)
            {
                Properties.IsHidden = false;
                _amountText.text = Properties.BallAmount.ToString();
                SetColor();
            }
        }
        else
        {
            _meshRenderer.material.SetFloat("_OutlineWidth", 0);
            _amountText.DOFade(.35f, .1f);
            _multiAmountText.DOFade(.35f, .1f);
        }
    }

    private void OnMouseDown()
    {
        if (!_isClickable || _exploded) return;
        if (GameManager.Instance.IsGameLose) return;
        if (Properties.IsIce) return;
        Explode();
    }

    private void Explode()
    {
        _exploded = true;
        _rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(SpawnSmallBalls());
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
    }

    private IEnumerator SpawnSmallBalls()
    {
        // _meshRenderer.enabled = false;


        var capacityManager = AreaCapacityManager.Instance;
        int available = capacityManager.CapacityAmount - capacityManager.CurrentAmount;

        if (available <= 0)
        {
            // Hiç yer yok, geri döndür
            RestoreBall();
            yield break;
        }

        int totalNeeded = Properties.BallBlocker == BallBlockers.MultiBall
            ? Properties.BallAmount + Properties.MultiAmount
            : Properties.BallAmount;

        if (available >= totalNeeded)
        {
            // Hepsi sığıyor, normal patlat

            SoundManager.Instance.BalloonPopSound();

            transform.DOScale(transform.localScale.x + .85f, .15f);

            yield return StartCoroutine(SpawnBatch(Properties.BallAmount, Properties.ObjectColor, capacityManager
            ));
            if (Properties.BallBlocker == BallBlockers.MultiBall)
            {
                yield return StartCoroutine(SpawnBatch(Properties.MultiAmount, Properties.MultiColor, capacityManager,
                    true));
            }
            EventManager.OnBallExplode?.Invoke();
            OnExploded?.Invoke();
            // yield return new WaitForSeconds(0.1f);
            _amountText.gameObject.SetActive(false);
            if (_innerBallObject != null) _innerBallObject.SetActive(false);
            if (_multiAmountText != null) _multiAmountText.gameObject.SetActive(false);
            ParticleManager.Instance.BalloonPopParticle(transform.position, Properties.ObjectColor,Properties.BallAmount);
            Destroy(gameObject);
        }
        else
        {
            // Kısmen sığıyor — orantılı dağıt
            int mainSpawn, multiSpawn;

            if (Properties.BallBlocker == BallBlockers.MultiBall)
            {
                // Orantılı böl
                float ratio = (float)Properties.BallAmount / totalNeeded;
                mainSpawn = Mathf.FloorToInt(available * ratio);
                multiSpawn = available - mainSpawn;
            }
            else
            {
                mainSpawn = available;
                multiSpawn = 0;
            }

            yield return StartCoroutine(SpawnBatch(mainSpawn, Properties.ObjectColor, capacityManager));
            if (Properties.BallBlocker == BallBlockers.MultiBall && multiSpawn > 0)
                yield return StartCoroutine(SpawnBatch(multiSpawn, Properties.MultiColor, capacityManager));

            // Kalanları hesapla
            Properties.BallAmount -= mainSpawn;
            if (Properties.BallBlocker == BallBlockers.MultiBall)
                Properties.MultiAmount -= multiSpawn;

            // Sıfırlandıysa yok et
            bool mainEmpty = Properties.BallAmount <= 0;
            bool multiEmpty = Properties.BallBlocker != BallBlockers.MultiBall || Properties.MultiAmount <= 0;

            if (mainEmpty && multiEmpty)
            {
                OnExploded?.Invoke();
                yield return new WaitForSeconds(0.1f);
                Destroy(gameObject);
            }
            else
            {
                // Kalan miktarla geri döndür
                if (mainEmpty && Properties.BallBlocker == BallBlockers.MultiBall)
                {
                    // Sadece multi kaldı, rengi güncelle
                    Properties.ObjectColor = Properties.MultiColor;
                    Properties.BallAmount = Properties.MultiAmount;
                    Properties.BallBlocker = BallBlockers.None;
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

        var a = Mathf.InverseLerp(5, 30, Properties.BallAmount + Properties.MultiAmount);
        var scale = Mathf.Lerp(0.55f, 1.325f, a);
        SetColor(true);
        transform.DOScale(Vector3.one * scale, .2f);
    }

    private IEnumerator SpawnBatch(int amount, ColorTypes color, AreaCapacityManager capacityManager,
        bool isMulti = false)
    {
        if (!isMulti && !IsFromTunnel)
        {
            yield return new WaitForSeconds(.1f);
        }


        float radius = (transform.localScale.x-.8f) * 0.5f;
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
                rb.AddForce(dir * smallBallSpeed + Vector3.down * 2f, ForceMode.VelocityChange);
                Vector3 tunnelBoost = IsFromTunnel ? transform.right * -1.5f : Vector3.zero;
                Debug.Log(tunnelBoost);
                rb.AddForce((dir * smallBallSpeed) + tunnelBoost, ForceMode.VelocityChange);
            }
        }

        capacityManager.SetAmount(amount);
        yield return null;
    }
}

[Serializable]
public class BallProperties
{
    public BallController.BallBlockers BallBlocker;

    [ShowIf("BallBlocker", BallController.BallBlockers.MultiBall)]
    public ColorTypes MultiColor;

    public ColorTypes ObjectColor;

    [ShowIf("BallBlocker", BallController.BallBlockers.MultiBall)]
    public int MultiAmount = 5;

    public int BallAmount = 10;
    public bool IsHidden;
    public bool IsIce;
    [ShowIf("IsIce")] public int IceAmount;
    public Vector3 Position;
}