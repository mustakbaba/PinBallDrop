using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TutorialSession
{
    [System.Serializable]
    public struct TutorialStep
    {
        public string target_obj;
        public string text;
        public bool isClickable;
        public bool isReplacingNextButton;
    }
    public TutorialStep[] steps;
}
