#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.TutorialEngine
{
    public class LevelLeftMenuItem : MonoBehaviour
    {

        [HideInInspector]
        public string tutorialName;

        [HideInInspector]
        public string tutorialPath;
        private Button btn;
        private Button editBtn;
        private Button validBtn;
        private Text nameText;
        private InputField nameInput;

        void Start()
        {

            if (nameInput == null) nameInput = transform.FindChildRecursiveComp<InputField>("NameField");
            nameInput.gameObject.SetActive(false);

            validBtn = transform.FindChildRecursiveComp<Button>("ValidBtn");
            validBtn.onClick.AddListener(OnClickValidate);
            validBtn.gameObject.SetActive(false);

            editBtn = transform.FindChildRecursiveComp<Button>("EditBtn");
            editBtn.onClick.AddListener(OnClickEdit);

            btn = transform.FindChildRecursiveComp<Button>("NameBtn");
            btn.onClick.AddListener(OnClickItem);
        }

        private void OnClickValidate()
        {
            if (tutorialName != nameInput.text)
                Edit(tutorialName, nameInput.text);

            validBtn.gameObject.SetActive(false);
            nameInput.gameObject.SetActive(false);

        }

        private void OnClickEdit()
        {
            validBtn.gameObject.SetActive(true);
            nameInput.gameObject.SetActive(true);
            nameInput.text = tutorialName;
        }

        public void SetLevelPath(string path)
        {
            tutorialPath = path;
            tutorialName = path.Remove(0, path.LastIndexOf("/") + 1).Replace(".asset", "").Replace(".json", "");

            nameText = transform.FindChildRecursiveComp<Text>("NameText");

            nameText.text = tutorialName;
        }

        private void OnClickItem()
        {

            if (TutorialLoader.instance != null)
                TutorialLoader.instance.Load(tutorialName);
            if (TutorialEditorController.instance != null)
                TutorialEditorController.instance.Load(tutorialName);

            if (LeftMenu.instance != null)
                LeftMenu.instance.CloseMenu();
        }

        public void Edit(string name, string newName)
        {
            if (System.IO.File.Exists(Application.dataPath + TutorialEditorController.SavePath + newName + ".json"))
            {
                Debug.LogError("The File " + Application.dataPath + TutorialEditorController.SavePath + newName + ".json already exists");
                return;
            }


#if UNITY_EDITOR

            // Saving local file
            try
            {

                System.IO.File.Move(Application.dataPath + TutorialEditorController.SavePath + name + ".json",
                                    Application.dataPath + TutorialEditorController.SavePath + newName + ".json");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                nameText.text = tutorialName = newName;
            }
            catch (System.Exception e) { Debug.LogError(e.Message); }

#endif
        }

    }
}
