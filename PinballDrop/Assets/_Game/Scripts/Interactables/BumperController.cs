using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SincappStudio;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BumperController : MonoBehaviour
{
    public bool IsActiveBumper;
    [Header("Properties")] public bool IsHidden;
    public bool IsConnectedBlock;
    public ColorTypes ObjectColors;

    [Space(5)] [Header("References")] [SerializeField]
    private MeshRenderer _boxMesh;

    [SerializeField] private TextMeshPro _amountText;
    public int Count { get; set; }
    private MaterialPropertyBlock _propBlock;
    private LineBoxHolderManager _lineBoxHolderManager;
    private bool _isOut = false;
    public Transform BouncePoint;

    private void Start()
    {
        if (transform.position.x<0)
        {
            BouncePoint.SetX_Pos(.15f,Space.Self);
        }
    }

    public void InitTarget()
    {
        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        if (LevelManager.Instance == null || LevelManager.Instance.ObjectColors == null)
        {
            return;
        }

        var clr = LevelManager.Instance.ObjectColors[(int)ObjectColors] * .76f;
        clr.a = 1f;

        var clrCape = clr;
        clrCape.a = 1;


        _boxMesh.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_BaseColor", clr);
        _boxMesh.SetPropertyBlock(_propBlock);

        _propBlock.SetColor("_BaseColor", clrCape);


        _amountText.text = Count.ToString();

        if (IsHidden)
        {
            _amountText.text = "?";

            _boxMesh.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.3f) * .5f);
            _boxMesh.SetPropertyBlock(_propBlock);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
#endif
    }

    public void Bounce()
    {
        transform.DOScale(1.2f, .1f).OnComplete(() =>
        {
            transform.DOScale(1f, .1f);
        });
    }
}
