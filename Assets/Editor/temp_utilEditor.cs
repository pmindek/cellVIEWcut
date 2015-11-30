
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(tmp_util))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        tmp_util myScript = (tmp_util)target;
        if(GUILayout.Button("Export Protein Settings"))
        {
            //TODO: call here the method in the util class for saving
            myScript.ExportProteinSettings();
        }
        else if (GUILayout.Button("Import Protein Settings"))
        {
            //TODO: call here the method in the util class for loading
            myScript.ImportProteinSettings();
        }
    }
}