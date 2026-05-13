using UnityEngine;
using UnityEngine.UI;

public class TutorialStepAddBtn : MonoBehaviour
{

    private Button btn;
    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        TutorialStepEditorView.AddTutorialStep(new TutorialSession.TutorialStep());
    }

}
