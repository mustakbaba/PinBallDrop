// SmallBallController.cs

using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SmallBallController : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _grounded;
    private bool _goingToVacuum;
    private VacuumController _vacuum;
    public ColorTypes ObjectColor;

    public float vacuumForce = 8f;
    public int bounceCount = 0;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _vacuum = FindObjectOfType<VacuumController>();
    }

    private void Update()
    {
        if (_grounded || _goingToVacuum) return;

        // Y hızı durdu ve altında bir şey var
        bool isStopped = Mathf.Abs(_rb.velocity.y) < 0.5f;
        bool hasSupport = Physics.Raycast(transform.position, Vector3.down, 0.8f,
            LayerMask.GetMask("Obstacle", "SmallBall"));

        if (isStopped && hasSupport)
        {
            _grounded = true;
            StartCoroutine(GoToVacuum());
        }
    }

    private IEnumerator GoToVacuum()
    {
        _goingToVacuum = true;
        yield return new WaitForSeconds(0.3f);

        while (true)
        {
            if (_vacuum == null) yield break;

            // Sadece XZ düzleminde vacuum'a doğru git, Y'ye dokunma
            Vector3 dir = _vacuum.transform.position - transform.position;
            dir.y = 0f;

            float distance = dir.magnitude;

            if (distance < 0.3f)
            {
                _vacuum.ShootTheBall(this);
                yield break;
            }

            _rb.AddForce(dir.normalized * vacuumForce, ForceMode.Force);

            yield return new WaitForFixedUpdate();
        }
    }

    // SmallBallController.cs — JumpToTargets
    // SmallBallController.cs
    public void JumpToTargets()
    {

        if (BumperAreaManager.Instance.ActiveBumpers.Count <= bounceCount)
        {
            SlotHolderManager.Instance.TryPlaceBall(this);
            return;
        }

        var targetBumper = BumperAreaManager.Instance.ActiveBumpers[bounceCount];
        transform.DOJump(targetBumper.BouncePoint.position, 1, 1, .35f)
            .OnComplete(() =>
            {
                targetBumper.Bounce(this);
            }).SetEase(Ease.Linear);
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
       AreaCapacityManager.Instance.SetAmount(1);

        StopAllCoroutines();

        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void GoToPipe()
    {
        PipeHolderManager pipeHolderManager = PipeHolderManager.Instance;

        transform.DOMoveY(pipeHolderManager.BottomPipe.position.y, 9f)
            .SetEase(Ease.Linear)
            .SetSpeedBased()
            .OnComplete(() =>
            {
                transform.DOMoveX(pipeHolderManager.RightPipe.position.x, 9f)
                    .SetEase(Ease.Linear)
                    .SetSpeedBased()
                    .OnComplete(() =>
                    {
                        transform.DOMoveY(pipeHolderManager.RightPipeEnd.position.y, 9f)
                            .SetEase(Ease.Linear)
                            .SetSpeedBased()
                            .OnComplete(() =>
                            {
                                transform.DOMoveX(transform.position.x - 1.5f, 9).
                                    SetEase(Ease.Linear).SetSpeedBased()
                                    .OnComplete(() =>
                                    {
                                        // State'i tamamen sıfırla
                                        ResetBall();
                                        var col = GetComponent<Collider>();

                                        // Fiziği geri aç
                                        _rb.isKinematic = false;
                                        _rb.velocity = Vector3.zero;
                                        _rb.angularVelocity = Vector3.zero;
                                        if (col != null) col.enabled = true;
                                    });
                            });
                    });
            });
    }
}