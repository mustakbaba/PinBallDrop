// VacuumController.cs
using DG.Tweening;
using UnityEngine;

public class VacuumController : MonoBehaviour
{
    [SerializeField] private Transform _exitTransform;

    public void ShootTheBall(SmallBallController ball)
    {
        var rb = ball.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.transform.DOMove(_exitTransform.position, .1f).OnComplete(() =>
        {
            var target = BumperAreaManager.Instance.ActiveBumpers[0].BouncePoint;
            ball.transform.DOMove(target.position, .25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                BumperAreaManager.Instance.ActiveBumpers[0].Bounce();
                ball.JumpToTargets();
            });;
        });
    }
}