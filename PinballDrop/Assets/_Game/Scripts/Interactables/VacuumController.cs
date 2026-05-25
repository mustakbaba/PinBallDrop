// VacuumController.cs
using DG.Tweening;
using UnityEngine;

public class VacuumController : MonoBehaviour
{
    [SerializeField] private Transform _exitTransform;

    public void ShootTheBall(SmallBallController ball)
    {
        AreaCapacityManager.Instance.SetAmount(-1);
        var rb = ball.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.transform.DOMove(_exitTransform.position, .1f).OnComplete(() =>
        {
            var firstBumper = BumperAreaManager.Instance.ActiveBumpers[0];
            ball.transform.DOMove(firstBumper.BouncePoint.position, .25f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    firstBumper.Bounce(ball); 
                });
        });
    }
    
}