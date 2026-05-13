using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MarbleController : MonoBehaviour
{
    public ColorTypes ObjectColor { get; set; }
    private MaterialPropertyBlock _propertyBlock;

    public void SetColor(ColorTypes objectColor)
    {
        ObjectColor = objectColor;
        var color = LevelManager.Instance.ObjectColors[(int)objectColor];

        var renderer = GetComponentInChildren<MeshRenderer>();
        // MaterialPropertyBlock oluştur ve uygula
        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(_propertyBlock);
        if (Application.isPlaying)
        {
            renderer.materials[0].color = color;
        }
    }

    public void Drop()
    {
        transform.DOMoveZ(
                transform.position.z - 5f,
                0.4f
            )
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // Destroy(gameObject);

                // Diğer marble'lar tekrar kontrol etsin
                MarblePathChecker.Instance.CheckAllMarbles();
            });
        transform.DOMoveY(transform.position.y + .42f, 0.2f).SetEase(Ease.OutQuad)
       .OnComplete(() =>
        {
           
        });
    }
}