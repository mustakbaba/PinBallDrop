using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineBoxConnectorController : MonoBehaviour
{
    [Header("Endpoints")]
    public Transform a;
    public Transform b;

    [Header("Cylinder settings")]
    [Tooltip("Unity default Cylinder uses Y as length axis.")]
    public float radius = 0.1f;

    void LateUpdate()
    {
        if (!a || !b) return;

        Vector3 p1 = a.position;
        Vector3 p2 = b.position;

        Vector3 dir = p2 - p1;
        float dist = dir.magnitude;

        if (dist < 0.0001f)
        {
            // Çok üst üste gelirse saçmalamasın
            transform.position = p1;
            return;
        }

        // 1) Pozisyon: orta nokta (pivot ortada)
        transform.position = (p1 + p2) * 0.5f;

        // 2) Rotasyon: Y ekseni dir yönüne baksın
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // 3) Scale: Y = dist/2 (çünkü cylinder'ın yüksekliği 2 birim kabul edilir: -1..+1)
        // X ve Z: radius (çap değil, scale olduğu için "kalınlık" gibi düşün)
        transform.localScale = new Vector3(radius, dist * 0.5f, radius);
    }
    
    public void ConnectBlocks(Transform pointA, Transform pointB)
    {
        a = pointA;
        b = pointB;
    }
}