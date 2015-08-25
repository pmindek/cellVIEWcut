//C# Example

using System;
using UnityEditor;
using UnityEngine;

public class CustomWindow : EditorWindow
{
    //[MenuItem("cellVIEW/Debug Commnad")]
    //static void AddMoleculeInstance()
    //{
    //    //CellPackLoader.Debug();
    //}

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Load cellPACK results")]
    public static void LoadCellPackResults()
    {
        SceneManager.Instance.ClearScene();
        CellPackLoader.LoadCellPackResults();
        EditorUtility.SetDirty(SceneManager.Instance);
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Clear scene")]
    public static void ClearScene()
    {
        SceneManager.Instance.ClearScene();
        EditorUtility.SetDirty(SceneManager.Instance);
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Show Window")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(CustomWindow));
    }

    private bool showOptions;
    private bool showIngredients;
    private bool showLodOptions;
    private bool toggleSelectAll = true;

    private Vector2 _scrollPos;
    private readonly string[] _contourOptionsLabels = { "Show Contour", "Hide Contour", "Contour Only" };
	
    void OnGUI()
    {
        EditorUtility.SetDirty(PersistantSettings.Instance);

        GUIStyle style_1 = new GUIStyle();
        style_1.margin = new RectOffset(10, 10, 10, 10);

        // Begin scroll view
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, style_1, GUILayout.ExpandWidth(true));
        {
            if (GUILayout.Button("Load cellPACK results"))
            {
                CellPackLoader.LoadCellPackResults();
            }
            
            if (GUILayout.Button("Clear Scene"))
            {
                ClearScene();
            }

            EditorGUILayout.Space();
            
#region Option Panel

            showOptions = EditorGUILayout.Foldout(showOptions, "Show Options");
            if (showOptions)
            {
                GUIStyle style_2 = new GUIStyle(GUI.skin.box);
                style_2.margin = new RectOffset(10, 10, 10, 10);
                style_2.padding = new RectOffset(10, 10, 10, 10);

                EditorGUILayout.BeginVertical(style_2);
                {
                    EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    PersistantSettings.Instance.Scale = EditorGUILayout.Slider("Global scale", PersistantSettings.Instance.Scale, 0.01f, 1);
                    PersistantSettings.Instance.ContourStrength = EditorGUILayout.Slider("Contour strength", PersistantSettings.Instance.ContourStrength, 0, 1);
                    PersistantSettings.Instance.ContourOptions = EditorGUILayout.Popup("Contours Options", PersistantSettings.Instance.ContourOptions, _contourOptionsLabels);
                    //DisplaySettings.Instance.EnableOcclusionCulling = EditorGUILayout.Toggle("Enable Culling", DisplaySettings.Instance.EnableOcclusionCulling);
                    //DisplaySettings.Instance.DebugObjectCulling = EditorGUILayout.Toggle("Debug Culling", DisplaySettings.Instance.DebugObjectCulling);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    //EditorGUILayout.LabelField("DNA Settings", EditorStyles.boldLabel);
                    //EditorGUI.indentLevel++;
                    //PersistantSettings.Instance.EnableDNAConstraints = EditorGUILayout.Toggle("Enable DNA Constraints", PersistantSettings.Instance.EnableDNAConstraints);
                    ////DisplaySettings.Instance.DistanceContraint = EditorGUILayout.Slider("Distance Constraint", DisplaySettings.Instance.DistanceContraint, 0.01f, 100);
                    ////DisplaySettings.Instance.AngularConstraint = EditorGUILayout.Slider("Angular Constraint", DisplaySettings.Instance.AngularConstraint, 0.01f, 100);
                    ////DisplaySettings.Instance.NumStepsPerSegment = EditorGUILayout.IntField("Num Steps Per Segment", DisplaySettings.Instance.NumStepsPerSegment);

                    ////DisplaySettings.Instance.EnableTwist = EditorGUILayout.Toggle("Enable Twist", DisplaySettings.Instance.EnableTwist);
                    ////DisplaySettings.Instance.TwistFactor = EditorGUILayout.Slider("Twist Factor", DisplaySettings.Instance.TwistFactor, -360.0f, 360);
                    //EditorGUI.indentLevel--;
                    //EditorGUILayout.Space();

                    PersistantSettings.Instance.EnableCrossSection = EditorGUILayout.BeginToggleGroup("Cross Section", PersistantSettings.Instance.EnableCrossSection);
                    EditorGUI.indentLevel++;
                    PersistantSettings.Instance.CrossSectionPlaneNormal = EditorGUILayout.Vector3Field("Plane Normal", PersistantSettings.Instance.CrossSectionPlaneNormal).normalized;
                    PersistantSettings.Instance.CrossSectionPlaneDistance = EditorGUILayout.FloatField("Plane Distance", PersistantSettings.Instance.CrossSectionPlaneDistance);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndToggleGroup();
                    EditorGUILayout.Space();

                    PersistantSettings.Instance.EnableLod = EditorGUILayout.BeginToggleGroup("Level of Detail", PersistantSettings.Instance.EnableLod);
                    {
                        PersistantSettings.Instance.FirstLevelOffset = EditorGUILayout.FloatField("First Level Being Range", PersistantSettings.Instance.FirstLevelOffset);

                        EditorGUI.indentLevel++;
                        for (int i = 0; i <= SceneManager.Instance.NumLodLevels; i++)
                        {
                            EditorGUILayout.LabelField("Level " + i, EditorStyles.boldLabel);
                            EditorGUI.indentLevel++;

                            var x = EditorGUILayout.FloatField("End Range", PersistantSettings.Instance.LodLevels[i].x);
                            var y = EditorGUILayout.FloatField("Min Radius", PersistantSettings.Instance.LodLevels[i].y);
                            var z = EditorGUILayout.FloatField("Max Radius", PersistantSettings.Instance.LodLevels[i].z);

                            var lodInfo = new Vector4(x, y, z, 1);

                            if (PersistantSettings.Instance.LodLevels[i] != lodInfo)
                            {
                                PersistantSettings.Instance.LodLevels[i] = lodInfo;
                                ComputeBufferManager.Instance.LodInfos.SetData(PersistantSettings.Instance.LodLevels);
                            }

                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                EditorGUILayout.EndVertical();
            }

#endregion
            
            EditorGUILayout.Space();

#region Ingredient Panel

            if (SceneManager.Instance.ProteinNames.Count > 0 || SceneManager.Instance.CurveIngredientsNames.Count > 0)
            {
                showIngredients = EditorGUILayout.Foldout(showIngredients, "Show Ingredients");

                if (showIngredients)
                {
                    var setDirty = false;

                    var style4 = new GUIStyle();
                    style4.margin = new RectOffset(10, 10, 5, 5);
                    style4.padding = new RectOffset(10, 10, 0, 0);

                    EditorGUILayout.BeginVertical(style4);
                    {
                        var newToggleSelectAll = EditorGUILayout.ToggleLeft("Select All", toggleSelectAll);

                        if (newToggleSelectAll != toggleSelectAll)
                        {
                            for (int i = 0; i < SceneManager.Instance.ProteinToggleFlags.Count; i++)
                            {
                                SceneManager.Instance.ProteinToggleFlags[i] = Convert.ToInt32(newToggleSelectAll);
                            }

                            for (int i = 0; i < SceneManager.Instance.CurveIngredientToggleFlags.Count; i++)
                            {
                                SceneManager.Instance.CurveIngredientToggleFlags[i] = Convert.ToInt32(newToggleSelectAll);
                            }

                            toggleSelectAll = newToggleSelectAll;
                            setDirty = true;
                        }

                    }
                    EditorGUILayout.EndVertical();

                    var style5 = new GUIStyle(GUI.skin.box);
                    style5.margin = new RectOffset(10, 10, 5, 10);
                    style5.padding = new RectOffset(10, 10, 10, 10);

                    EditorGUILayout.BeginVertical(style5);
                    {
                        for (int i = 0; i < SceneManager.Instance.ProteinNames.Count; i++)
                        {
                            var toggle = Convert.ToBoolean(SceneManager.Instance.ProteinToggleFlags[i]);
                            var newToggle = EditorGUILayout.ToggleLeft(SceneManager.Instance.ProteinNames[i], toggle);
                            if (toggle != newToggle)
                            {
                                SceneManager.Instance.ProteinToggleFlags[i] = Convert.ToInt32(newToggle);
                                setDirty = true;
                            }

                            GUILayout.Space(3);
                        }

                        for (int i = 0; i < SceneManager.Instance.CurveIngredientsNames.Count; i++)
                        {
                            var toggle = Convert.ToBoolean(SceneManager.Instance.CurveIngredientToggleFlags[i]);
                            var newToggle =
                                EditorGUILayout.ToggleLeft(SceneManager.Instance.CurveIngredientsNames[i], toggle);
                            if (toggle != newToggle)
                            {
                                SceneManager.Instance.CurveIngredientToggleFlags[i] = Convert.ToInt32(newToggle);
                                setDirty = true;
                            }

                            GUILayout.Space(3);
                        }
                    }
                    EditorGUILayout.EndVertical();

                    if (setDirty)
                    {
                        EditorUtility.SetDirty(SceneManager.Instance);
                        SceneManager.Instance.UploadIngredientToggleData();
                    }
                }
            }

#endregion
            
        }

        EditorGUILayout.EndScrollView();
    }
}