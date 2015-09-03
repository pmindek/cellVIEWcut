using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CutObject))]
public class CutObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CutObject cutObject = (CutObject)target;
        if (GUILayout.Button("Show List"))
        {
            var cutObjectEditorWindows = Resources.FindObjectsOfTypeAll<CutObjectWindow>();

            bool foundWindow = false;

            foreach(var window in cutObjectEditorWindows)
            {
                if(window.cutObject == cutObject)
                {
                    window.Show();
                    foundWindow = true;
                }
            }

            if(!foundWindow)
            {
                var newWindow = CreateInstance<CutObjectWindow>(); //GetWindow(typeof(CutObjectWindow)) as CutObjectWindow;
                newWindow.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 80);
                newWindow.title = cutObject.name;
                newWindow.cutObject = cutObject;
                newWindow.Show();
            }
        }
    }
}