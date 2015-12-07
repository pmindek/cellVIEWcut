using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

enum InstanceState
{
    Null = -1,           // Instance will not be displayed
    Normal = 0,          // Instance will be displayed with normal color
    Highlighted = 1      // Instance will be displayed with highlighted color
}

struct CutInfoStruct
{
    public Vector4 info;
    public Vector4 info2;
    public Vector4 info3;
}

public struct HistStruct
{
    public int parent; //also write data to this id, unless it is < 0

    public int all;
    public int cutaway;
    public int occluding;
    public int visible;

    public int pad0;
    public int pad1;
    public int pad2;
}

[ExecuteInEditMode]
public class SceneManager : MonoBehaviour
{
    // Declare the scene manager as a singleton
    private static SceneManager _instance = null;

    public static SceneManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<SceneManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_SceneManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_SceneManager") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<SceneManager>();
            }

            _instance.OnUnityReload();
            return _instance;
        }
    }

    public void Awake()
    {
        var s = SceneManager.Get;
    }

    public void OnEnable()
    {
        
    }

    public static bool CheckInstance()
    {
        return _instance != null;
    }

    //--------------------------------------------------------------

    public string scene_name;
    
    // Scene data
    public List<string> SceneHierarchy = new List<string>();

    public List<Vector4> IngredientProperties = new List<Vector4>();

    // Lipid data 
    public List<string> LipidIngredientNames = new List<string>();
    public List<Vector4> LipidAtomPositions = new List<Vector4>();
    public List<Vector4> LipidInstanceInfos = new List<Vector4>();
    public List<Vector4> LipidInstancePositions = new List<Vector4>();
    
    // Protein ingredients data
    public List<Vector4> ProteinInstanceInfos = new List<Vector4>();
    public List<Vector4> ProteinInstancePositions = new List<Vector4>();
    public List<Vector4> ProteinInstanceRotations = new List<Vector4>();

    public List<int> ProteinAtomCount = new List<int>();
    public List<int> ProteinAtomStart = new List<int>();
    public List<int> ProteinToggleFlags = new List<int>();
    public List<string> ProteinIngredientNames = new List<string>();
    public List<Vector4> ProteinAtoms = new List<Vector4>();
    public List<Vector4> ProteinColors = new List<Vector4>();
    public List<float> ProteinRadii = new List<float>();
    public List<Vector4> ProteinAtomClusters = new List<Vector4>();
    public List<int> ProteinAtomClusterCount = new List<int>();
    public List<int> ProteinAtomClusterStart = new List<int>();
    
    // Curve ingredients data
    public List<int> CurveIngredientsAtomStart = new List<int>();
    public List<int> CurveIngredientsAtomCount = new List<int>();
    public List<int> CurveIngredientToggleFlags = new List<int>();
    public List<string> CurveIngredientsNames = new List<string>();
    public List<Vector4> CurveIngredientsAtoms = new List<Vector4>();
    public List<Vector4> CurveIngredientsInfos = new List<Vector4>();
    public List<Vector4> CurveIngredientsColors = new List<Vector4>();
    
    public List<Vector4> CurveControlPointsInfos = new List<Vector4>();
    public List<Vector4> CurveControlPointsNormals = new List<Vector4>();
    public List<Vector4> CurveControlPointsPositions = new List<Vector4>();

    // Histogram data

    [NonSerialized]
    public List<HistStruct> HistogramData = new List<HistStruct>();

    [NonSerialized]
    public List<int> ProteinToNodeLookup = new List<int>();

    [NonSerialized]
    public List<int> NodeToProteinLookup = new List<int>();

    //*****

    // This serves as a cache to avoid calling GameObject.Find on every update because not efficient
    // The cache will be filled automatically via the CutObject script onEnable

    [NonSerialized]
    public int ResetCutSnapshot = -1;

    [NonSerialized] public int SelectedCutObject = 0;
    [NonSerialized] public List<CutObject> CutObjects = new List<CutObject>();

    public CutObject GetSelectedCutObject()
    {
        return CutObjects[SelectedCutObject];
    }

    public List<CutObject> GetSelectedCutObjects()
    {
        var selectedCutObjects = new List<CutObject>();
        selectedCutObjects.Add(CutObjects[SelectedCutObject]);
        return selectedCutObjects;
    }

    //*** Ingredients ****//

    private List<string> _ingredientNames = new List<string>();
    public List<string> AllIngredientNames
    {
        get
        {
            if(_ingredientNames.Count != (ProteinIngredientNames.Count + LipidIngredientNames.Count))
            {
                _ingredientNames.Clear();
                _ingredientNames.AddRange(ProteinIngredientNames);
                _ingredientNames.AddRange(LipidIngredientNames);
            }

            return _ingredientNames;
        }
    }

    [NonSerialized]
    public List<int> AllIngredientStates = new List<int>(); 

    public int NumAllIngredients
    {
        get { return AllIngredientNames.Count; }
    }

    //--------------------------------------------------------------

    public int NumLodLevels = 0;
    public int TotalNumProteinAtoms = 0;

    public int NumLipidIngredients
    {
        get { return LipidIngredientNames.Count; }
    }

    public int NumProteinIngredients
    {
        get { return ProteinIngredientNames.Count; }
    }

    public int NumLipidInstances
    {
        get { return LipidInstancePositions.Count; }
    }

    public int NumProteinInstances
    {
        get { return ProteinInstancePositions.Count; }
    }

    public int NumCutObjects
    {
        get { return CutObjects.Count; }
    }

    public int NumDnaControlPoints
    {
        get { return CurveControlPointsPositions.Count; }
    }

    public int NumDnaSegments
    {
        get { return Math.Max(CurveControlPointsPositions.Count - 1, 0); }
    }

    

    //--------------------------------------------------------------
    
    private void Update()
    {
        UpdateCutObjects();
    }

    private void OnUnityReload()
    {
        Debug.Log("Reload Scene");
        
        UploadAllData();
    }

    //--------------------------------------------------------------

    #region Ingredients

    public void AddIngredientToHierarchy(string ingredientUrlPath)
    {
        var urlPathSplit = MyUtility.SplitUrlPath(ingredientUrlPath);

        if (urlPathSplit.Count() == 1)
        {
            if (!SceneHierarchy.Contains(urlPathSplit.First()))
                SceneHierarchy.Add(urlPathSplit.First());
        }
        else
        {
            var parentUrlPath = MyUtility.GetParentUrlPath(ingredientUrlPath);
            
            if (!SceneHierarchy.Contains(parentUrlPath))
            {
                AddIngredientToHierarchy(parentUrlPath);
            }

            if (!SceneHierarchy.Contains(ingredientUrlPath))
            {
                SceneHierarchy.Add(ingredientUrlPath);
            }
            else
            {
                throw new Exception("Ingredient path already used");
            }
        }
    }

    //*** Protein Ingredients ****//

    public void AddIngredientProperties(int atomCount, int instanceCount)
    {
        IngredientProperties.Add(new Vector4(atomCount, instanceCount));
    }

    public void AddProteinIngredient(string path, Bounds bounds, List<Vector4> atomSpheres, Color color,
        List<float> clusterLevels = null,
        bool nolod = false)
    {
        if (SceneHierarchy.Contains(path)) throw new Exception("Invalid protein path: " + path); 
        if (ProteinIngredientNames.Contains(path)) throw new Exception("Invalid protein path: " + path);

        if (clusterLevels != null)
        {
            if (NumLodLevels != 0 && NumLodLevels != clusterLevels.Count)
                throw new Exception("Uneven cluster levels number: " + path);
        }
        if (color == null)
        {
            color = MyUtility.GetRandomColor();
        }

        
        AddIngredientToHierarchy(path);


        ProteinColors.Add(color);
        ProteinToggleFlags.Add(1);
        ProteinIngredientNames.Add(path);
        ProteinRadii.Add(AtomHelper.ComputeRadius(atomSpheres));

        ProteinAtomCount.Add(atomSpheres.Count);
        ProteinAtomStart.Add(ProteinAtoms.Count);
        ProteinAtoms.AddRange(atomSpheres);

        if (clusterLevels != null)
        {
            NumLodLevels = clusterLevels.Count;
            foreach (var level in clusterLevels)
            {
                var numClusters = Math.Max(atomSpheres.Count*level, 5);
                List<Vector4> clusterSpheres;
                if (!nolod)
                    clusterSpheres = KMeansClustering.GetClusters(atomSpheres, (int) numClusters);
                else
                    clusterSpheres = new List<Vector4>(atomSpheres);
                ProteinAtomClusterCount.Add(clusterSpheres.Count);
                ProteinAtomClusterStart.Add(ProteinAtomClusters.Count);
                ProteinAtomClusters.AddRange(clusterSpheres);
            }
        }
    }

    public void AddProteinInstance(string path, Vector3 position, Quaternion rotation, int unitId = 0)
    {
        if (!ProteinIngredientNames.Contains(path))
        {
            throw new Exception("Ingredient path do not exists");
        }

        var ingredientId = ProteinIngredientNames.IndexOf(path);

        ProteinInstanceInfos.Add(new Vector4(ingredientId, (int) InstanceState.Normal, 0));
        ProteinInstancePositions.Add(position);
        ProteinInstanceRotations.Add(MyUtility.QuanternionToVector4(rotation));

        TotalNumProteinAtoms += ProteinAtomCount[ingredientId];
    }

    //*** Curve Ingredients ****//

    public void AddCurveIngredient(string name, string pdbName)
    {
        if (ProteinIngredientNames.Contains(name)) return;

        int numSteps = 1;
        float twistAngle = 0;
        float segmentLength = 34.0f;
        var color = MyUtility.GetRandomColor();

        if (name.Contains("DNA"))
        {
            numSteps = 12;
            twistAngle = 34.3f;
            segmentLength = 34.0f;
            color = Color.yellow;

            var atomSpheres = PdbLoader.LoadAtomSpheresBiomt(pdbName);
            CurveIngredientsAtomCount.Add(atomSpheres.Count);
            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtoms.AddRange(atomSpheres);
        }
        else if (name.Contains("mRNA"))
        {
            numSteps = 12;
            twistAngle = 34.3f;
            segmentLength = 34.0f;
            color = Color.red;

            var atomSpheres = PdbLoader.LoadAtomSpheresBiomt(pdbName);
            CurveIngredientsAtomCount.Add(atomSpheres.Count);
            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtoms.AddRange(atomSpheres);
        }
        else if (name.Contains("peptide"))
        {
            numSteps = 10;
            twistAngle = 0;
            segmentLength = 20.0f;
            color = Color.magenta;

            var atomSphere = new Vector4(0, 0, 0, 3);
            CurveIngredientsAtomCount.Add(1);
            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtoms.Add(atomSphere);
        }
        else if (name.Contains("lypoglycane"))
        {
            numSteps = 10;
            twistAngle = 0;
            segmentLength = 20;
            color = Color.green;

            var atomSphere = new Vector4(0, 0, 0, 8);
            CurveIngredientsAtomCount.Add(1);
            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtoms.Add(atomSphere);
        }
        else
        {
            throw new Exception("Curve ingredient unknown");
        }

        CurveIngredientsNames.Add(name);
        CurveIngredientsColors.Add(color);
        CurveIngredientToggleFlags.Add(1);
        CurveIngredientsInfos.Add(new Vector4(numSteps, twistAngle, segmentLength));
    }

    public void AddCurveIntance(string name, List<Vector4> path)
    {
        if (!CurveIngredientsNames.Contains(name))
        {
            throw new Exception("Curve ingredient type do not exists");
        }

        var curveIngredientId = CurveIngredientsNames.IndexOf(name);
        var positions = MyUtility.ResampleControlPoints(path, CurveIngredientsInfos[curveIngredientId].z);
        var normals = MyUtility.GetSmoothNormals(positions);

        var curveId = CurveControlPointsPositions.Count;
        var curveType = CurveIngredientsNames.IndexOf(name);

        for (int i = 0; i < positions.Count; i++)
        {
            CurveControlPointsInfos.Add(new Vector4(curveId, curveType, 0, 0));
        }

        CurveControlPointsNormals.AddRange(normals);
        CurveControlPointsPositions.AddRange(positions);

        //Debug.Log(positions.Count);
    }

    //*** Membrane Ingredients ****//

    public void AddMembrane(string filePath, Vector3 position, Quaternion rotation)
    {
        var pathInner = "root.membrane.inner_membrane";
        var pathOuter = "root.membrane.outer_membrane";

        AddIngredientToHierarchy(pathInner);
        AddIngredientToHierarchy(pathOuter);

        LipidIngredientNames.Clear();
        LipidIngredientNames.Add(pathInner);
        LipidIngredientNames.Add(pathOuter);

        LipidAtomPositions.Clear();
        LipidInstanceInfos.Clear();
        LipidInstancePositions.Clear();

        var currentLipidAtoms = new List<Vector4>();
        var membraneData = MyUtility.ReadBytesAsFloats(filePath);

        var ingredientIdInner = AllIngredientNames.IndexOf(pathInner);
        var ingredientIdOuter = AllIngredientNames.IndexOf(pathOuter);

        var step = 5;
        var dataIndex = 0;
        var lipidAtomStart = 0;
        var previousLipidId = -1;

        while (true)
        {
            var flushCurrentBatch = false;
            var breakAfterFlushing = false;

            if (dataIndex >= membraneData.Count())
            {
                flushCurrentBatch = true;
                breakAfterFlushing = true;
            }
            else
            {
                var lipidId = (int)membraneData[dataIndex + 4];
                if (previousLipidId < 0) previousLipidId = lipidId;
                if (lipidId != previousLipidId)
                {
                    flushCurrentBatch = true;
                    previousLipidId = lipidId;
                }
            }

            if (flushCurrentBatch)
            {
                var bounds = AtomHelper.ComputeBounds(currentLipidAtoms);
                var center = new Vector4(bounds.center.x, bounds.center.y, bounds.center.z, 0);
                for (var j = 0; j < currentLipidAtoms.Count; j++) currentLipidAtoms[j] -= center;

                var innerMembrane = Vector3.Magnitude(bounds.center) < 727;

                Vector4 batchPosition = position + bounds.center;
                batchPosition.w = Vector3.Magnitude(bounds.extents);

                LipidInstancePositions.Add(batchPosition);
                LipidInstanceInfos.Add(new Vector4(innerMembrane ? ingredientIdInner : ingredientIdOuter, lipidAtomStart, currentLipidAtoms.Count));

                lipidAtomStart += currentLipidAtoms.Count;
                LipidAtomPositions.AddRange(currentLipidAtoms);
                currentLipidAtoms.Clear();

                if (breakAfterFlushing) break;
            }

            var currentAtom = new Vector4(membraneData[dataIndex], membraneData[dataIndex + 1], membraneData[dataIndex + 2], AtomHelper.AtomRadii[(int)membraneData[dataIndex + 3]]);
            currentLipidAtoms.Add(currentAtom);
            dataIndex += step;
        }

        int a = 0;
    }

    #endregion

    //--------------------------------------------------------------

    #region Cut Objects

    //public void AddCutObject(CutType type)
    //{
    //    var gameObject =
    //        Instantiate(Resources.Load("Prefabs/CutObjectPrefab"), Vector3.zero, Quaternion.identity) as GameObject;
    //    var cutObject = gameObject.GetComponent<CutObject>().CutType = type;
    //}

    // Todo: proceed only if changes are made 
    public void UpdateCutObjects()
    {
        var CutInfos = new List<CutInfoStruct>();
        var CutScales = new List<Vector4>();
        var CutPositions = new List<Vector4>();
        var CutRotations = new List<Vector4>();

        // For each cut object
        foreach (var cut in CutObjects)
        {
            if (cut == null) throw new Exception("Cut object not fofund");

            CutScales.Add(cut.transform.localScale);
            CutPositions.Add(cut.transform.position);
            CutRotations.Add(MyUtility.QuanternionToVector4(cut.transform.rotation));
            //CutInfos.Add(new Vector4((float)cut.CutType, cut.Value1, cut.Value2, cut.Inverse ? 1.0f : 0.0f));
        }

        foreach (var cut in CutObjects)
        {
            foreach (var cutParam in cut.IngredientCutParameters)
            {
                CutInfos.Add(new CutInfoStruct
                {
                    info = new Vector4((float) cut.CutType, cutParam.value1, cutParam.value2, cut.Inverse ? 1.0f : 0.0f),
                    info2 = new Vector4(cutParam.fuzziness, cutParam.fuzzinessDistance, cutParam.fuzzinessCurve, cutParam.Aperture),
                    info3 = new Vector4(0,0,0,0)
                });
            }
        }

        GPUBuffers.Instance.CutInfo.SetData(CutInfos.ToArray());
        GPUBuffers.Instance.CutScales.SetData(CutScales.ToArray());
        GPUBuffers.Instance.CutPositions.SetData(CutPositions.ToArray());
        GPUBuffers.Instance.CutRotations.SetData(CutRotations.ToArray());
        //GPUBuffer.Instance.ProteinCutFilters.SetData(ProteinCutFilters.ToArray());
        //GPUBuffer.Instance.HistogramProteinTypes.SetData(HistogramProteinTypes.ToArray());
        //GPUBuffer.Instance.HistogramStatistics.SetData(new[] { 0, 1, 2, 3 });
    }

    public void UpdateCutObjectParams()
    {
        CutObjects.Clear();
        foreach (var cutObject in FindObjectsOfType<CutObject>())
        {
            cutObject.InitCutParameters();
            CutObjects.Add(cutObject);
        }
    }

    #endregion

    //--------------------------------------------------------------

    #region Misc

    // Scene data gets serialized on each reload, to clear the scene call this function
    public void ClearScene()
    {
        System.GC.Collect();

        Debug.Log("Clear Scene");

        NumLodLevels = 0;
        TotalNumProteinAtoms = 0;

        // Clear all lists
        foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType.FullName.Contains("System.Collections.Generic.List"))
            {
                var v = field.GetValue(this) as IList;
                v.Clear();
                //int a = 0;
            }
        }

        //// Clear lipid data
        //LipidIngredientNames.Clear();
        //LipidAtomPositions.Clear();
        //LipidInstanceInfos.Clear();
        //LipidInstancePositions.Clear();

        //// Clear scene data
        //ProteinInstanceInfos.Clear();
        //ProteinInstancePositions.Clear();
        //ProteinInstanceRotations.Clear();

        //// Clear ingredient data
        //ProteinIngredientNames.Clear();
        //ProteinColors.Clear();
        //ProteinToggleFlags.Clear();
        //ProteinRadii.Clear();

        //// Clear atom data
        //ProteinAtoms.Clear();
        //ProteinAtomCount.Clear();
        //ProteinAtomStart.Clear();

        //// Clear cluster data
        //ProteinAtomClusters.Clear();
        //ProteinAtomClusterStart.Clear();
        //ProteinAtomClusterCount.Clear();

        //// Clear curve data
        //CurveIngredientsInfos.Clear();
        //CurveIngredientsNames.Clear();
        //CurveIngredientsColors.Clear();
        //CurveIngredientToggleFlags.Clear();
        //CurveIngredientsAtoms.Clear();
        //CurveIngredientsAtomCount.Clear();
        //CurveIngredientsAtomStart.Clear();
        
        //CurveControlPointsPositions.Clear();
        //CurveControlPointsNormals.Clear();
        //CurveControlPointsInfos.Clear();

        UploadAllData();
    }

    private void CheckBufferSizes()
    {
        if (Get.NumCutObjects >= GPUBuffers.NumCutsMax) throw new Exception("GPU buffer overflow");
        //if (Instance.ProteinCutFilters.Count >= ComputeBufferManager.NumCutsMax * ComputeBufferManager.NumProteinMax) throw new Exception("GPU buffer overflow");
        if (Get.NumLodLevels >= GPUBuffers.NumLodMax) throw new Exception("GPU buffer overflow");
        if (Get.ProteinIngredientNames.Count >= GPUBuffers.NumProteinMax) throw new Exception("GPU buffer overflow");
        if (Get.ProteinAtoms.Count >= GPUBuffers.NumProteinAtomMax) throw new Exception("GPU buffer overflow");
        if (Get.ProteinAtomClusters.Count >= GPUBuffers.NumProteinAtomClusterMax) throw new Exception("GPU buffer overflow");
        if (Get.ProteinAtomClusterCount.Count >= GPUBuffers.NumProteinMax * GPUBuffers.NumLodMax) throw new Exception("GPU buffer overflow");
        if (Get.ProteinInstancePositions.Count >= GPUBuffers.NumProteinInstancesMax) throw new Exception("GPU buffer overflow");

        if (Get.CurveIngredientsNames.Count >= GPUBuffers.NumCurveIngredientMax) throw new Exception("GPU buffer overflow");
        if (Get.CurveControlPointsPositions.Count >= GPUBuffers.NumCurveControlPointsMax) throw new Exception("GPU buffer overflow");
        if (Get.CurveIngredientsAtoms.Count >= GPUBuffers.NumCurveIngredientAtomsMax) throw new Exception("GPU buffer overflow");
    }

    public void UploadAllData()
    {
        System.GC.Collect();

        InitHistogramLookups();
        UpdateCutObjectParams();

        CheckBufferSizes();
        GPUBuffers.Instance.InitBuffers();
        GPUBuffers.Instance.ArgBuffer.SetData(new[] { 0, 1, 0, 0 });


        GPUBuffers.Instance.IngredientProperties.SetData(IngredientProperties.ToArray());

        // Upload histogram info
        GPUBuffers.Instance.Histograms.SetData(HistogramData.ToArray());
        GPUBuffers.Instance.HistogramsLookup.SetData(ProteinToNodeLookup.ToArray());

        // Upload Lod levels info
        GPUBuffers.Instance.LodInfo.SetData(PersistantSettings.Instance.LodLevels);

        // Upload ingredient data
        GPUBuffers.Instance.ProteinRadii.SetData(ProteinRadii.ToArray());
        GPUBuffers.Instance.ProteinColors.SetData(ProteinColors.ToArray());
        GPUBuffers.Instance.IngredientMaskParams.SetData(ProteinToggleFlags.ToArray());

        GPUBuffers.Instance.ProteinAtoms.SetData(ProteinAtoms.ToArray());
        GPUBuffers.Instance.ProteinAtomCount.SetData(ProteinAtomCount.ToArray());
        GPUBuffers.Instance.ProteinAtomStart.SetData(ProteinAtomStart.ToArray());

        GPUBuffers.Instance.ProteinAtomClusters.SetData(ProteinAtomClusters.ToArray());
        GPUBuffers.Instance.ProteinAtomClusterCount.SetData(ProteinAtomClusterCount.ToArray());
        GPUBuffers.Instance.ProteinAtomClusterStart.SetData(ProteinAtomClusterStart.ToArray());

        GPUBuffers.Instance.ProteinInstanceInfo.SetData(ProteinInstanceInfos.ToArray());
        GPUBuffers.Instance.ProteinInstancePositions.SetData(ProteinInstancePositions.ToArray());
        GPUBuffers.Instance.ProteinInstanceRotations.SetData(ProteinInstanceRotations.ToArray());

        // Upload curve ingredient data
        GPUBuffers.Instance.CurveIngredientsAtoms.SetData(CurveIngredientsAtoms.ToArray());
        GPUBuffers.Instance.CurveIngredientsAtomCount.SetData(CurveIngredientsAtomCount.ToArray());
        GPUBuffers.Instance.CurveIngredientsAtomStart.SetData(CurveIngredientsAtomStart.ToArray());
        
        GPUBuffers.Instance.CurveIngredientsInfo.SetData(CurveIngredientsInfos.ToArray());
        GPUBuffers.Instance.CurveIngredientsColors.SetData(CurveIngredientsColors.ToArray());
        GPUBuffers.Instance.CurveIngredientsToggleFlags.SetData(CurveIngredientToggleFlags.ToArray());

        GPUBuffers.Instance.CurveControlPointsInfo.SetData(CurveControlPointsInfos.ToArray());
        GPUBuffers.Instance.CurveControlPointsNormals.SetData(CurveControlPointsNormals.ToArray());
        GPUBuffers.Instance.CurveControlPointsPositions.SetData(CurveControlPointsPositions.ToArray());

        // Upload lipid data
        GPUBuffers.Instance.LipidAtomPositions.SetData(LipidAtomPositions.ToArray());
        GPUBuffers.Instance.LipidInstanceInfo.SetData(LipidInstanceInfos.ToArray());
        GPUBuffers.Instance.LipidInstancePositions.SetData(LipidInstancePositions.ToArray());
    }

    void InitHistogramLookups()
    {
        // Init histogram GPU buffer
        HistogramData.Clear();
        foreach (var path in SceneHierarchy)
        {
            var hist = new HistStruct
            {
                parent = -1,
                all = 0,
                cutaway = 0,
                occluding = 0,
                visible = 0
            };

            if (MyUtility.IsPathRoot(path))
            {
                hist.parent = -1;
            }
            else
            {
                var parentPath = MyUtility.GetParentUrlPath(path);
                if(!SceneHierarchy.Contains(parentPath)) throw new Exception("Hierarchy corrupted");
                hist.parent = SceneHierarchy.IndexOf(parentPath);
            }

            HistogramData.Add(hist);
        }

        //*******************************//

        ProteinToNodeLookup.Clear();

        foreach (var ingredientName in AllIngredientNames)
        {
            if (SceneHierarchy.Contains(ingredientName))
            {
                ProteinToNodeLookup.Add(SceneHierarchy.IndexOf(ingredientName));
            }
        }

        //*******************************//

        NodeToProteinLookup.Clear();

        foreach (var path in SceneHierarchy)
        {
            if (AllIngredientNames.Contains(path))
            {
                NodeToProteinLookup.Add(AllIngredientNames.IndexOf(path));
            }
            else
            {
                NodeToProteinLookup.Add(-1);
            }
        }

        int a = 0;
    }
    
    #endregion

    
}
