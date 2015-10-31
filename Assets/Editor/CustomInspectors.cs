﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CutObject))]
public class CutObjectCustomEditor : Editor
{
    public Vector2 _scrollPos;

    [SerializeField]
    public bool showFilters = false;

    [SerializeField]
    public CutParameters currentParameters;

    //private bool[] proteinFoldout = new bool[100];

    private float lastRange0;
    private float lastRange1;

    private float lastValue1;
    private float lastValue2;

    private bool once = false;
    
    
    TreeViewItem m_lastSelectedItem = null;

    Vector2 m_mousePos = Vector2.zero;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CutObject cutObject = (CutObject)target;


        TreeViewControl item = cutObject.GetComponent<TreeViewControl>();
        if (null == item)
        {
            Debug.LogError("TreeViewControl is null");
            return;
        }

        bool needsRepainted = false;

        if (null != Event.current &&
            m_mousePos != Event.current.mousePosition)
        {
            needsRepainted = true;
        }

        if (item.SelectedItem != m_lastSelectedItem)
        {
            m_lastSelectedItem = item.SelectedItem;
            needsRepainted = true;
        }

        if (null != item.SelectedItem &&
            string.IsNullOrEmpty(item.SelectedItem.Header))
        {
            item.SelectedItem.Header = "Root item";
            needsRepainted = true;
        }
        
        showFilters = EditorGUILayout.Foldout(showFilters, "Protein Filters");
        if (showFilters)
        {
            EditorUtility.SetDirty(cutObject);

            EditorGUILayout.Separator();
            RecipeTreeUI ui = cutObject.GetComponent<RecipeTreeUI>();

            /*Debug.Log("GR" + ui.currentSelectedIngredient);
            Debug.Log("GL");
            for (int i = 0; i < ui.selectedIngredients.Count; i++)
            {
                Debug.Log(" .~" + ui.selectedIngredients[i]);
            }*/

            //Debug.Log(Random.Range(1, 999));
            //Debug.Log(ui.selectedIngredients.Count);

            if (ui.currentSelectedIngredient != -1)
            {
                /* non-cached working version
                var name = SceneManager.Instance.ProteinNames[ui.currentSelectedIngredient];

                var rangeValues = cutObject.GetRangeValues(ui.currentSelectedIngredient);

                GUILayout.Label("Visibility: " + name + " ~ " + ui.currentSelectedIngredient);
                MultiRangeSlider.HandleCascadeSliderGUI(ref rangeValues);
                EditorGUILayout.Separator();

                cutObject.SetRangeValues(ui.currentSelectedIngredient, rangeValues);

                float st_cutaway = (float) SceneManager.Instance.stats[0];
                float st_all = (float) SceneManager.Instance.stats[1];
                float st_occluding = (float) SceneManager.Instance.stats[2];

                if (!MultiRangeSlider.mouseDragging)
                {
                    rangeValues[0] = st_occluding / st_all;
                    rangeValues[1] = (1.0f - st_cutaway / st_all) - rangeValues[0];
                }
                 */

                currentParameters = new CutParameters()
                {
                    range0 = 0.0f,
                    range1 = 0.0f,

                    countAll = 0,
                    count0 = 0,
                    count1 = 0,

                    value1 = 0.0f,
                    value2 = 0.0f,
                    fuzziness = 0.0f,
                    fuzzinessDistance = 0.0f,
                    fuzzinessCurve = 0.0f
                };

                CutParameters param = null;

                for (int i = 0; i < ui.selectedIngredients.Count; i++)
                {
                    param = cutObject.GetCutParametersFor(ui.selectedIngredients[i]);

                    currentParameters.range0 += param.range0;
                    currentParameters.range1 += param.range1;

                    currentParameters.countAll += param.countAll;
                    currentParameters.count0 += param.count0;
                    currentParameters.count1 += param.count1;

                    currentParameters.value1 += param.value1;
                    currentParameters.value2 += param.value2;
                    currentParameters.fuzziness += param.fuzziness;
                    currentParameters.fuzzinessDistance += param.fuzzinessDistance;
                    currentParameters.fuzzinessCurve += param.fuzzinessCurve;
                }

                float fc = (float)ui.selectedIngredients.Count;

                currentParameters.range0 /= fc;
                currentParameters.range1 /= fc;

                currentParameters.value1 /= fc;
                currentParameters.value2 /= fc;
                currentParameters.fuzziness /= fc;
                currentParameters.fuzzinessDistance /= fc;
                currentParameters.fuzzinessCurve /= fc;

                var name = SceneManager.Instance.ProteinNames[ui.currentSelectedIngredient];

                float[] rangeValues = new float[2] {currentParameters.range0, currentParameters.range1};

                //GUILayout.Label("Visibility: " + name + " ~ " + ui.currentSelectedIngredient);
                GUILayout.Label("~" + rangeValues[0] + " - " + rangeValues[1]);
                MultiRangeSlider.HandleCascadeSliderGUI(ref rangeValues);
                EditorGUILayout.Separator();

                if (!MultiRangeSlider.mouseDragging || once)
                {
                    once = false;

                    if (ui.itemSelected)
                    {
                        cutObject.Value1 = currentParameters.value1;
                        cutObject.Value2 = currentParameters.value2;
                        cutObject.Fuzziness = currentParameters.fuzziness;
                        cutObject.FuzzinessDistance = currentParameters.fuzzinessDistance;
                        cutObject.FuzzinessCurve = currentParameters.fuzzinessCurve;

                        ui.itemSelected = false;
                    }
                    else
                    {
                        currentParameters.value1 = cutObject.Value1;
                        currentParameters.value2 = cutObject.Value2;
                        currentParameters.fuzziness = cutObject.Fuzziness;
                        currentParameters.fuzzinessDistance = cutObject.FuzzinessDistance;
                        currentParameters.fuzzinessCurve = cutObject.FuzzinessCurve;
                    }

                    currentParameters.countAll = SceneManager.Instance.stats[1];
                    currentParameters.count0 = SceneManager.Instance.stats[0];
                    currentParameters.count1 = SceneManager.Instance.stats[2];

                    float st_cutaway = (float)currentParameters.count0;
                    float st_all = (float)currentParameters.countAll;
                    float st_occluding = (float)currentParameters.count1;

                    if (st_all == 0.0f)
                        st_all = 0.001f;

                    rangeValues[0] = st_occluding / st_all;
                    rangeValues[1] = (1.0f - st_cutaway / st_all) - rangeValues[0];

                    MultiRangeSlider.updateMousePositionWhileDragging(rangeValues[0], rangeValues[1]);

                    lastRange0 = rangeValues[0];
                    lastRange1 = rangeValues[1];

                    lastValue1 = currentParameters.value1;
                    lastValue2 = currentParameters.value2;
                }
                else 
                {
                    float d0 = 0.0f;
                    float d1 = 0.0f;

                    if (lastRange0 > rangeValues[0])
                    {
                        d0 = -1.0f + rangeValues[0] / lastRange0;
                    }
                    else if (lastRange0 < rangeValues[0])
                    {
                        d0 = (rangeValues[0] - lastRange0) / (lastRange1 - lastRange0);
                    }

                    if (lastRange1 > rangeValues[1])
                    {
                        d1 = -1.0f + rangeValues[1] / lastRange1;
                    }
                    else if (lastRange1 < rangeValues[1])
                    {
                        d1 = (rangeValues[1] - lastRange1) / (1 - lastRange1 - lastRange0);
                    }

                    if (d0 < -1.0f)
                        d0 = -1.0f;
                    if (d0 > 1.0f)
                        d0 = 1.0f;
                    if (d1 < -1.0f)
                        d1 = -1.0f;
                    if (d1 > 1.0f)
                        d1 = 1.0f;

                    if (Mathf.Abs(d0) > 0.001f)
                        d1 = 0.0f;

                    float v1 = lastValue1 + d1;
                    float v2 = lastValue2 + d0;

                    if (v1 < 0.0f)
                        v1 = 0.0f;
                    if (v1 > 1.0f)
                        v1 = 1.0f;
                    if (v2 < 0.0f)
                        v2 = 0.0f;
                    if (v2 > 1.0f)
                        v2 = 1.0f;


                    currentParameters.value1 = v1;
                    currentParameters.value2 = v2;
                    
                    cutObject.Value1 = currentParameters.value1;
                    cutObject.Value2 = currentParameters.value2;

                    once = true;
                }

                {
                    currentParameters.range0 = rangeValues[0];
                    currentParameters.range1 = rangeValues[1];
                }

                for (int i = 0; i < ui.selectedIngredients.Count; i++)
                {
                    cutObject.SetCutParametersFor(ui.selectedIngredients[i], currentParameters);
                }


                    /*st_cutaway = (float)SceneManager.Instance.stats[0];
                    st_all = (float)SceneManager.Instance.stats[1];
                    st_occluding = (float)SceneManager.Instance.stats[2];

                    if (st_all == 0.0f)
                        st_all = 0.001f;

                    rangeValues[0] = st_occluding / st_all;
                    rangeValues[1] = (1.0f - st_cutaway / st_all) - rangeValues[0];*/

                //cutObject.SetRangeValues(ui.currentSelectedIngredient, rangeValues);


                /*
                1. first of all, each protein type should have its own value1/value2 setting (or maybe all of them. yes, all of them).
                2. when you click on any item, the settings are restored
                3. these settings are used in the shader
                4. when we start dragging, we save the value1 and value2. dragging creates deltas which change them
                    */

                //Debug.Log(st_visible + " / " + st_all);
                //Debug.Log("ú " + rangeValues);

            }
            else
            {
                //GUILayout.Label("Nevym.");
            }

            // Begin scroll view
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
            {
                if (needsRepainted)
                {
                    Repaint();
                    SceneView.RepaintAll();
                }
                
                item.DisplayTreeView(TreeViewControl.DisplayTypes.NONE);
            }
            EditorGUILayout.EndScrollView();

            
        }

        //showFilters = EditorGUILayout.Foldout(showFilters, "Protein Filters");
        //if(showFilters)
        //{
        //    EditorUtility.SetDirty(cutObject);

        //    GUIStyle style_1 = new GUIStyle();
        //    style_1.margin = new RectOffset(10, 0, 0, 0);
        //    ////style_1.padding = new RectOffset(50, 0, 0, 0);

        //    //GUIStyle style_2 = EditorStyles.foldout;
        //    //style_1.margin = new RectOffset(50, 0, 0, 0);
        //    //style_2.padding = new RectOffset(50, 0, 0, 0);

        //    // Begin scroll view
        //    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
        //    {
        //        //GUILayout.Label(""); // + cutObject.gameObject.name);

        //        //EditorGUILayout.Space();

        //        EditorGUILayout.BeginVertical();
        //        {
        //            for (int i = 0; i < cutObject.ProteinCutFilters.Count; i++)
        //            {
        //                cutObject.ProteinCutFilters[i].State = EditorGUILayout.ToggleLeft(cutObject.ProteinCutFilters[i].Name, cutObject.ProteinCutFilters[i].State);

        //                EditorGUILayout.BeginVertical(style_1);
        //                {
        //                    proteinFoldout[i] = EditorGUILayout.Foldout(proteinFoldout[i], "Protein filter params");
        //                    if (proteinFoldout[i])
        //                    {
        //                        MultiRangeSlider.HandleCascadeSliderGUI(ref rangeValues);
        //                    }
        //                }
        //                EditorGUILayout.EndVertical();


        //                GUILayout.Space(3);
        //            }
        //        }
        //        EditorGUILayout.EndVertical();
        //    }
        //    EditorGUILayout.EndScrollView();
        //}
    }
}

[CustomEditor(typeof(CopyCameraEffects))]
public class CopyCameraCustomEffects : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var copyCameraEffects = (CopyCameraEffects)target;
        if (GUILayout.Button("Copy Camera Effects"))
        {
            copyCameraEffects.CopyEffects();
        }

        //if (GUILayout.Button("Clear Scene Camera Effects"))
        //{
        //    copyCameraEffects.ClearSceneCameraEffects();
        //}
    }
}