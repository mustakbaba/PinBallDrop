using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SincappStudio;
using UnityEngine;

public class CameraSlider : MonoBehaviour
{
    [SerializeField] private float firstY, secondY;
    [SerializeField] private float durationSlide;

    private void Awake()
    {
        transform.SetZ_Pos(firstY,Space.Self);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            transform.DOLocalMoveZ(secondY, durationSlide).SetEase(Ease.InOutQuart);
        }
    }
}
