// SmallBallController.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class SmallBallController : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _grounded;
    public bool _goingToVacuum;
    private VacuumController _vacuum;
    public ColorTypes ObjectColor;

    public float vacuumForce = 8f;
    public int bounceCount = 0;
    private bool _wentToVacuum;
    private ParticleSystem _trailParticle;
    private bool _isCloseVacuum;
    private float randForce;

    private void Awake()
    {
        _trailParticle = GetComponentInChildren<ParticleSystem>(true);
        _trailParticle.gameObject.SetActive(false);
        _rb = GetComponent<Rigidbody>();
        _vacuum = FindObjectOfType<VacuumController>();
    }

    private void Update()
    {
        if (_grounded || _goingToVacuum) return;

        // Y hızı durdu ve altında bir şey var
        bool isStopped = Mathf.Abs(_rb.velocity.y) < 0.5f;
        bool hasSupport = Physics.Raycast(transform.position, Vector3.down, 0.8f,
            LayerMask.GetMask("Ground", "SmallBall"));

        if (isStopped)
        {
            _grounded = true;
            StartCoroutine(GoToVacuum());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_wentToVacuum) return; // zaten gitti, tekrar tetiklenmesin
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            var vacuum = other.GetComponent<VacuumController>();
            if (vacuum != null)
            {
                _wentToVacuum = true;
                vacuum.ShootTheBall(this);
                _trailParticle.gameObject.SetActive(true);
                var allParticleChildren = _trailParticle.GetComponentsInChildren<ParticleSystem>();
                for (var i = 0; i < allParticleChildren.Length; i++)
                {
                    var ps = allParticleChildren[i];
                    var clt = ps.colorOverLifetime;

                    if (i == 0)
                    {
                        // clt.color = LevelManager.Instance.ObjectColors[(int)ObjectColor];


                        Gradient gradient = new Gradient();

                        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

                        alphaKeys[0] = new GradientAlphaKey();
                        alphaKeys[0].alpha = 1f; // başlangıç alpha
                        alphaKeys[0].time = 0f; // 0. index

                        alphaKeys[1] = new GradientAlphaKey();
                        alphaKeys[1].alpha = 0f; // bitiş alpha
                        alphaKeys[1].time = 1f; // 1. index

                        GradientColorKey[] colorKeys = new GradientColorKey[2];

                        colorKeys[0] = new GradientColorKey(Color.white, 0f);
                        colorKeys[1] = new GradientColorKey(LevelManager.Instance.ObjectColors[(int)ObjectColor], 1f);

                        gradient.SetKeys(colorKeys, alphaKeys);

                        clt.color = gradient;
                    }

                    if (i == 1)
                    {
                        // clt.color = LevelManager.Instance.ObjectColors[(int)ObjectColor];


                        Gradient gradient = new Gradient();

                        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

                        alphaKeys[0] = new GradientAlphaKey();
                        alphaKeys[0].alpha = 1f; // başlangıç alpha
                        alphaKeys[0].time = 0f; // 0. index

                        alphaKeys[1] = new GradientAlphaKey();
                        alphaKeys[1].alpha = 0f; // bitiş alpha
                        alphaKeys[1].time = 1f; // 1. index

                        GradientColorKey[] colorKeys = new GradientColorKey[2];

                        colorKeys[0] = new GradientColorKey(LevelManager.Instance.ObjectColors[(int)ObjectColor], 0f);
                        colorKeys[1] = new GradientColorKey(LevelManager.Instance.ObjectColors[(int)ObjectColor], 1f);

                        gradient.SetKeys(colorKeys, alphaKeys);

                        clt.color = gradient;
                    }
                    else
                    {
                    }
                }
            }
        }
    }

    private IEnumerator RandForceShake()
    {
        while (true)
        {
            if (_goingToVacuum && _isCloseVacuum)
            {
                randForce = Random.Range(1f, 5f);
                yield return new WaitForSeconds(.25f);
            }

            yield return null;
        }
    }

    private IEnumerator GoToVacuum()
    {
        _goingToVacuum = true;
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(RandForceShake());
        while (true)
        {
            if (_vacuum == null) yield break;
            if (_wentToVacuum) yield break;

            Vector3 dir = _vacuum.transform.position - transform.position;
            dir.z = 0f;

            float distance = dir.magnitude;


            if (distance < 1f) // eşiği büyüt
            {
                _isCloseVacuum = true;
                // _vacuum.ShootTheBall(this);
                // yield break;
            }

            // Yaklaştıkça force artır

            float forceMult = Mathf.Clamp(1f / distance, 0.5f, 3f);
            _rb.AddForce(dir.normalized * vacuumForce * forceMult * randForce, ForceMode.Acceleration);

            // Hızı sınırla, geçip gitmesin
            var vel = _rb.velocity;
            vel.z = 0f;
            if (vel.magnitude > 6f)
                _rb.velocity = vel.normalized * 6f;

            yield return new WaitForFixedUpdate();
        }
    }

    // SmallBallController.cs — JumpToTargets
    // SmallBallController.cs
    public void JumpToTargets()
    {
        if (BumperAreaManager.Instance.ActiveBumpers.Count <= bounceCount)
        {
            // Slot'a smooth git
            var slot = SlotHolderManager.Instance.GetAvailableSlot(ObjectColor);
            if (slot == null)
            {
                SlotHolderManager.Instance.TryPlaceBall(this);
                return;
            }

            transform.DOJump(slot.transform.position + Vector3.up * .3f, 1, 1, .35f)
                .SetEase(Ease.Linear)
                .OnComplete(() => { SlotHolderManager.Instance.TryPlaceBall(this); });
            return;
        }

        var targetBumper = BumperAreaManager.Instance.ActiveBumpers[bounceCount];
        transform.DOJump(targetBumper.BouncePoint.position, 1, 1, .35f)
            .OnComplete(() => { targetBumper.Bounce(this); }).SetEase(Ease.Linear);
    }

    public void SetColor(ColorTypes objectColor)
    {
        ObjectColor = objectColor;
        var mat = GetComponent<MeshRenderer>().material;
        mat.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)objectColor]);
    }

    // SmallBallController.cs — ResetBall ekle
    public void ResetBall()
    {
        _grounded = false;
        _goingToVacuum = false;
        bounceCount = 0;
        _wentToVacuum = false;
        StopAllCoroutines();
        _isCloseVacuum = false;
        randForce = 0;
        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void GoToPipe()
    {
        PipeHolderManager pipeHolderManager = PipeHolderManager.Instance;
        var points = pipeHolderManager.PipePathTransforms;

        MoveToPoint(points.ToList(), 0);
    }

    private void MoveToPoint(List<Transform> points, int index)
    {
        if (index >= points.Count)
        {
            ResetBall();
            var col = GetComponent<Collider>();
            _rb.isKinematic = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.AddForce(Vector3.left * 5f, ForceMode.VelocityChange);
            if (col != null) col.enabled = true;
            return;
        }

        float randX = 0;
        float randY = 0;
        if (index == points.Count - 1)
        {
            randX = Random.Range(-.5f, 0.5f);
            randY = Random.Range(-0.5f, 0.5f);
        }

        transform.DOMove(points[index].position + new Vector3(randX, randY, 0), 9f)
            .SetEase(Ease.Linear)
            .SetSpeedBased()
            .OnComplete(() => MoveToPoint(points, index + 1));
    }
}