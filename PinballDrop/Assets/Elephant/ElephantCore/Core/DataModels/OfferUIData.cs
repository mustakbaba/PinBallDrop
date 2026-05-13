using ElephantSDK;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class OfferUIData
{
    public ContainerData container;
    public ComponentData[] components;
}

[System.Serializable]
public class ContainerData
{
    public float width;
    public float height;
}

[System.Serializable]
public class ComponentData
{
    public string type;
    public string url;
    public float x;
    public float y;
    public float width;
    public float height;
    public string characters;
    public string action;
    public int font_size;
    public string animation_url;
    public string font_post_script_name;
    [JsonConverter(typeof(ColorConverter))] 
    public Color stroke;
    public float stroke_weight;
    [JsonConverter(typeof(ColorConverter))] 
    public Color fill;
    public float rotation;
    public string name;
    public string text_align_horizontal;
    public string text_align_vertical;
    public float real_w;
    public float real_h;
}