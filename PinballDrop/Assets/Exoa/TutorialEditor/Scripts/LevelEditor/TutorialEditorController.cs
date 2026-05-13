using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

namespace Exoa.TutorialEngine
{
    public class TutorialEditorController : TutorialLoader
    {

        public Button saveButton;
        public InputField levelInputField;

        public static string SavePath = "/Exoa/TutorialEngine/Resources/Tutorials/";

        override public void Awake()
        {
            base.Awake();
            saveButton.onClick.AddListener(() => Save(levelInputField.text));
            levelInputField.text = loadedTutorialName;

        }

        private void OnGUI()
        {
            if (Input.GetKeyDown(KeyCode.E) && Event.current.control && Event.current.type == EventType.KeyDown)
                SceneManager.LoadScene("Game");
            if (Input.GetKeyDown(KeyCode.S) && Event.current.control && Event.current.type == EventType.KeyDown)
                saveButton.onClick.Invoke();
        }


        override protected void ProcessTutorial(bool sendLoadedEvent)
        {
            print("Level Painter ProcessLevel");
            levelInputField.text = loadedTutorialName;

            TutorialStepEditorView.SetAllTutorialSteps(currentTutorial.tutorial_steps);

            base.ProcessTutorial(sendLoadedEvent);
        }

        public virtual void Save(string name)
        {
            GameObject savingFeedback = GameObject.Find("SavingFeedback");
            RectTransform rt = savingFeedback.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);

            print("Saving " + name + " " + levelInputField.text);


            Tutorial tutorial = new Tutorial();
            tutorial.tutorial_steps = TutorialStepEditorView.GetAllTutorialSteps().ToArray();

            string json = JsonUtility.ToJson(tutorial);


#if UNITY_EDITOR

            // Saving local file
            try
            {

                System.IO.File.WriteAllText(Application.dataPath + SavePath + name + ".json", json);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception e) { Debug.LogError(e.Message); }

#endif


            LeftMenu.instance.BuildMenu();

            rt.anchoredPosition = new Vector2(-30, rt.anchoredPosition.y);


        }


        private void SetTextfieldValue(string v, string text)
        {
            if (text == null) text = "";
            GameObject go = GameObject.Find(v);
            if (go == null) return;
            InputField d = go.GetComponent<InputField>();
            if (d == null) return;
            d.text = text;
        }
        private string GetTextfieldValue(string v)
        {
            GameObject go = GameObject.Find(v);
            if (go == null) return null;
            InputField d = go.GetComponent<InputField>();
            if (d == null) return null;
            return d.text;
        }
    }
}
