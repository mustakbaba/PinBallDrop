using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElephantStudioLogoHelper : MonoBehaviour
{
    public Canvas canvas;
    public Image logoImage;

    private void OnEnable()
    {
        Sprite logo = Resources.Load<Sprite>("ElephantResources/StudioLogo");
        if (logo == null)
        {
            canvas.gameObject.SetActive(false);
        }
        else
        {
            logoImage.sprite = logo;
            canvas.gameObject.SetActive(true);
            canvas.transform.localScale = Vector3.one * 0.1f;
            var rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.position = Vector2.zero;
            rectTransform.sizeDelta = Vector2.one;
            canvas.worldCamera = Camera.main;
        }
    }
}