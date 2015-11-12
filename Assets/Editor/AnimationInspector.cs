using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AnimationManager))]
public class AnimationInspector : Editor {


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AnimationManager aniManager = (AnimationManager)target;

        


    }

}
