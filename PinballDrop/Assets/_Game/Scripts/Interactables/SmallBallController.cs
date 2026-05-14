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

    public float vacuumForce = 8f;
    int bounceCount = 0;

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

    public void JumpToTargets()
    {
        bounceCount++;
        
        if (BumperAreaManager.Instance.ActiveBumpers.Count > bounceCount)
        {
            BumperAreaManager.Instance.ActiveBumpers[bounceCount-1].GetComponent<BumperController>().Bounce();

            transform.DOJump(BumperAreaManager.Instance.ActiveBumpers[bounceCount].BouncePoint.position, 1, 1, .35f)
                .OnComplete(JumpToTargets).SetEase(Ease.Linear);
        }
        else
        {
            // Tüm hedeflere zıpladıktan sonra topu yok et
            // Destroy(gameObject, 1f);
        }
    }
}