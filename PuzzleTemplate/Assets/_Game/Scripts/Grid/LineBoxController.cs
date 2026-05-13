using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lofelt.NiceVibrations;
using SincappStudio;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LineBoxController : MonoBehaviour
{
    [Header("Properties")] public bool IsHidden;
    public bool IsConnectedBlock;
    public ColorTypes ObjectColors;

    [Space(5)] [Header("References")] [SerializeField]
    private MeshRenderer _boxMesh;

    [SerializeField] private TextMeshPro _amountText;
    public int Count { get; set; }
    public int Id { get; set; }
    private MaterialPropertyBlock _propBlock;
    private LineBoxHolderManager _lineBoxHolderManager;
    private List<LineBoxController> _connectedBlocks = new List<LineBoxController>();
    public LineBoxConnectorController Connector { get; set; }
    private bool _isOut = false;

    private void Start()
    {
        _lineBoxHolderManager = GetComponentInParent<LineBoxHolderManager>();
        SetTextOpacity(_lineBoxHolderManager.GetIndex(this) == 0);
        if (IsConnectedBlock)
        {
            var allSpawneds = FindObjectsOfType<LineBoxHolderManager>().Select(x => x.SpawnedObjects).ToList();

            Debug.Log("Finding connected blocks for Turret ID: " + Id, gameObject);
            foreach (var block in allSpawneds)
            {
                foreach (Transform blockPart in block)
                {
                    var turretController = blockPart.GetComponent<LineBoxController>();

                    if (turretController.IsConnectedBlock && turretController != this && turretController.Id == Id)
                    {
                        _connectedBlocks.Add(turretController);

                        if (turretController.Connector != null) return;

                        Connector = Instantiate(LevelManager.Instance.ConnectorPrefab);
                        Connector.ConnectBlocks(transform, blockPart);
                        turretController.Connector = Connector;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        StartCoroutine(Sincapp.WaitAndAction(0, () =>
        {
            if (Application.isPlaying) return;
            EditorApplication.delayCall += InitTarget;
        }));
    }
#endif

    private void OnMouseDown()
    {
        if (InGameUIManager.Instance.IsBlockerPopupOpen) return;
        if (_isOut) return;
        if (_lineBoxHolderManager.GetIndex(this) == 0)
        {
            if (IsConnectedBlock)
            {
                if (_connectedBlocks[0].GetComponentInParent<LineBoxHolderManager>().GetIndex(_connectedBlocks[0]) == 0)
                {
                    Connector.gameObject.SetActive(false);
                    ExitObject();
                    _connectedBlocks[0].ExitObject();
                }
            }
            else
            {
                ExitObject();
            }
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


    public void ExitObject()
    {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
        _isOut = true;


        StartCoroutine(Sincapp.WaitAndAction(0.1f, () =>
        {
            transform.DOScale(0, .2f);
            StartCoroutine(FadeAlpha(1, 0, .15f));
        }));
    }


    public void Reveal()
    {
        IsHidden = false;
        _amountText.text = Count.ToString();
        _boxMesh.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_BaseColor", LevelManager.Instance.ObjectColors[(int)ObjectColors] * .5f);
        _boxMesh.SetPropertyBlock(_propBlock);
    }

    public void SetTextOpacity(bool isVisible)
    {
        var color = _amountText.color;
        color.a = isVisible ? 1 : .55f;
        _amountText.color = color;
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            foreach (MeshRenderer componentsInChild in _boxMesh.GetComponentsInChildren<MeshRenderer>())
            {
                componentsInChild.GetPropertyBlock(_propBlock);
                Color c = _propBlock.GetColor("_BaseColor");

                c.a = Mathf.Lerp(from, to, t);

                _propBlock.SetColor("_BaseColor", c);
                componentsInChild.SetPropertyBlock(_propBlock);
            }

            yield return null;
        }

        _lineBoxHolderManager.RemoveTargetBox(this);
    }
}