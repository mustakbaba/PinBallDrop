using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SincappStudio;
using TMPro;
using UnityEditor;

public class BallController : MonoBehaviour
{
    [Header("Ayarlar")]
    private float upwardForce = 12;
    private float maxY = 899f;

    private float smallBallSpeed = 4f;

    private Rigidbody _rb;
    private bool _isClickable;
    private bool _exploded;

    public int BallAmount = 10;
    public ColorTypes ObjectColor;
    private MaterialPropertyBlock _propertyBlock;
    [SerializeField] private TextMeshPro _amountText;
    private MeshRenderer _meshRenderer;
    public bool IsHidden;

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

    public void SetColor()
    {
        var color = LevelManager.Instance.ObjectColors[(int)ObjectColor];

        var renderer = GetComponentInChildren<MeshRenderer>();
        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(_propertyBlock);

        var a = Mathf.InverseLerp(5, 20, BallAmount);
        var scale = Mathf.Lerp(0.5f, 2f, a);
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
    }

    private void FixedUpdate()
    {
        if (_exploded) return;

        // Sürekli yukarı kuvvet
        _rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);

        // Üst sınırı geçemesin
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
        // Gravity Y, kamera Z'den bakıyor — önünde (Z ekseninde) top var mı?
        Ray ray = new Ray(transform.position, Vector3.down);
        bool blocked = Physics.Raycast(ray, 3f, LayerMask.GetMask("Collectable"));
        _isClickable = !blocked;

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

        float radius = transform.localScale.x * 0.5f;
        Vector3 center = transform.position;

        for (int i = 0; i < BallAmount; i++)
        {
            // Fibonacci disk — Z sabit, XY düzleminde eşit dağılım
            float angle = i * 137.5f * Mathf.Deg2Rad;
            float r = radius * Mathf.Sqrt((float)i / BallAmount);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * r,
                Mathf.Sin(angle) * r,
                0f
            );

            var small = Instantiate(LevelManager.Instance.SmallBallPrefab, center + offset, Quaternion.identity);
            var rb = small.GetComponent<Rigidbody>();
            small.SetColor(ObjectColor);

            if (rb != null)
            {
                // XY'de hafif dağıl, Z'de öne fırlat
                Vector3 dir = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(-0.3f, 0.3f),
                    -1f
                ).normalized;

                rb.AddForce(dir * smallBallSpeed, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}