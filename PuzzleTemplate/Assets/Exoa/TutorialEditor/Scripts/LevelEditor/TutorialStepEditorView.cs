using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStepEditorView : MonoBehaviour
{

    public Button delBtn;
    public Button addBtn;
    public Button downBtn;
    public Button upBtn;
    public Toggle makeClickableToggle;
    public Toggle replaceNextBtnToggle;
    public InputField targetObjInput;
    public InputField textInput;

    void Start()
    {
        delBtn.onClick.AddListener(OnClickDel);
        downBtn.onClick.AddListener(OnClickDown);
        upBtn.onClick.AddListener(OnClickUp);

        makeClickableToggle.onValueChanged.AddListener(OnMakeClickableChange);
        replaceNextBtnToggle.onValueChanged.AddListener(OnReplaceNextBtnChange);
    }

    private void OnMakeClickableChange(bool arg0)
    {
        //throw new NotImplementedException();
    }

    private void OnReplaceNextBtnChange(bool arg0)
    {
        //throw new NotImplementedException();
    }

    private void OnClickDel()
    {
        DestroyImmediate(gameObject);
        ResizeContainer();
    }



    private void OnClickAdd()
    {
        GameObject prefab = Resources.Load<GameObject>("TutorialStepEditorView");
        Instantiate(prefab, transform.parent);
    }

    private void OnClickDown()
    {
        int index = transform.GetSiblingIndex();
        index++;
        index = Mathf.Clamp(index, 0, transform.parent.childCount);
        transform.SetSiblingIndex(index);
    }

    private void OnClickUp()
    {
        int index = transform.GetSiblingIndex();
        index--;
        index = Mathf.Clamp(index, 0, transform.parent.childCount);
        transform.SetSiblingIndex(index);
    }

    public TutorialSession.TutorialStep GetStep()
    {
        InputField target = transform.FindChildRecursiveComp<InputField>("TargetObjInput");
        InputField t = transform.FindChildRecursiveComp<InputField>("TextInput");

        TutorialSession.TutorialStep s = new TutorialSession.TutorialStep();
        s.target_obj = target.text;
        s.text = t.text;
        s.isClickable = makeClickableToggle.isOn;
        s.isReplacingNextButton = replaceNextBtnToggle.isOn;
        return s;
    }
    public static void AddTutorialStep(TutorialSession.TutorialStep s)
    {
        GameObject prefab = Resources.Load<GameObject>("TutorialStepEditorView");
        GameObject inst = Instantiate(prefab, GameObject.Find("TutorialSteps").transform);
        TutorialStepEditorView view = inst.GetComponent<TutorialStepEditorView>();

        view.targetObjInput.text = s.target_obj;
        view.textInput.text = s.text;
        view.makeClickableToggle.isOn = s.isClickable;
        view.replaceNextBtnToggle.isOn = s.isReplacingNextButton;

        ResizeContainer();
    }

    public static void SetAllTutorialSteps(TutorialSession.TutorialStep[] steps)
    {

        GameObject.Find("TutorialSteps").transform.ClearChildrenImmediate();
        ResizeContainer();

        if (steps != null)
            foreach (TutorialSession.TutorialStep step in steps)
                AddTutorialStep(step);
    }
    public static List<TutorialSession.TutorialStep> GetAllTutorialSteps()
    {
        List<TutorialSession.TutorialStep> steps = new List<TutorialSession.TutorialStep>();
        RectTransform container = GameObject.Find("TutorialSteps").GetComponent<RectTransform>();

        for (int i = 0; i < container.childCount; i++)
        {
            TutorialStepEditorView view = container.GetChild(i).GetComponent<TutorialStepEditorView>();
            steps.Add(view.GetStep());
        }
        return steps;
    }



    public static void ResizeContainer()
    {
        RectTransform container = GameObject.Find("TutorialSteps").GetComponent<RectTransform>();
        container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, container.childCount * 732.47f);
    }
}
