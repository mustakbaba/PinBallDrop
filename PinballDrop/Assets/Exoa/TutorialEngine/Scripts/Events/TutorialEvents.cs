using UnityEngine;
using UnityEngine.Events;

namespace Exoa.TutorialEngine
{
    public class TutorialEvents
    {
        public delegate void OnTutorialEventHandler();
        public delegate void OnTutorialFocusHandler(string objectName, Vector3 rectCenterPosition);
        public delegate void OnTutorialProgressHandler(int currentStep, int totalSteps);

        public static OnTutorialEventHandler OnTutorialComplete;
        public static OnTutorialProgressHandler OnTutorialProgress;
        public static OnTutorialFocusHandler OnTutorialFocus;
        public static OnTutorialEventHandler OnTutorialLoaded;
        public static OnTutorialEventHandler OnTutorialReady;
    }
}
