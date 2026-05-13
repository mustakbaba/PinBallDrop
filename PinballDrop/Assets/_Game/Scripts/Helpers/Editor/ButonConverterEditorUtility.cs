using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButonConverterEditorUtility : MonoBehaviour
{
    [MenuItem("CONTEXT/Button/Convert to SincappButton")]
    public static void ConvertToSinCappButton(MenuCommand command)
    {
        Button button = (Button)command.context;

        GameObject buttonObject = button.gameObject;

        UnityEvent onClickEvents = button.onClick;

        DestroyImmediate(button);

        SincappButton newButton = buttonObject.AddComponent<SincappButton>();

        newButton.onClick = (Button.ButtonClickedEvent)onClickEvents;
    }
}
