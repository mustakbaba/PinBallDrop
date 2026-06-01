using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SocialButtonData
{
    public string name;
    public string url;
    public bool active;
    public int reward;
}

[System.Serializable]
public class SocialButtonList
{
    public List<SocialButtonData> buttons;
}
