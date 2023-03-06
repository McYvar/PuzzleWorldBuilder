using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DropDownMenu))]
public class DropDownMenuCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DropDownMenu menu = (DropDownMenu)target;
        if (GUILayout.Button("Add button"))
        {
            menu.AddButtonToMenu();
        }

        if (GUILayout.Button("Remove button"))
        {
            menu.RemoveButtonFromMenu();
        }

        if (GUILayout.Button("Clear buttons"))
        {
            menu.ClearButtons();
        }
    }
}
#endif