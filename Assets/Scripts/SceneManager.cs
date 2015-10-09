using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

enum InstanceState
{
    Null = -1,           // Instance will not be displayed
    Normal = 0,          // Instance will be displayed with normal color
    Highlighted = 1      // Instance will be displayed with highlighted color
};

[ExecuteInEditMode]
public class SceneManager : MonoBehaviour
{
    // Declare the scene manager as a singleton
    private static SceneManager _instance = null;
    public static SceneManager Instance
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

    public static bool CheckInstance()
    {
        return _instance != null;
    }

    //--------------------------------------------------------------
    
    // Scene data
    public List<Vector4> ProteinInstanceInfos = new List<Vector4>();
    public List<Vector4> ProteinInstancePositions = new List<Vector4>();
    public List<Vector4> ProteinInstanceRotations = new List<Vector4>();

    public List<Vector4> CurveControlPointsInfos = new List<Vector4>();
    public List<Vector4> CurveControlPointsNormals = new List<Vector4>();
    public List<Vector4> CurveControlPointsPositions = new List<Vector4>();

    // Protein ingredients data

    public List<int> ProteinAtomCount = new List<int>();
    public List<int> ProteinAtomStart = new List<int>();
    public List<int> ProteinToggleFlags = new List<int>();
    public List<string> ProteinNames = new List<string>();
    public List<Vector4> ProteinAtoms = new List<Vector4>();
    public List<Vector4> ProteinColors = new List<Vector4>();
    public List<float> ProteinRadii = new List<float>();
    public List<Vector4> ProteinAtomClusters = new List<Vector4>();
    public List<int> ProteinAtomClusterCount = new List<int>();
    public List<int> ProteinAtomClusterStart = new List<int>();

    public string scene_name;
    
    // Curve ingredients data
    
    public List<int> CurveIngredientsAtomStart = new List<int>();
    public List<int> CurveIngredientsAtomCount = new List<int>();
    public List<int> CurveIngredientToggleFlags = new List<int>();
    public List<string> CurveIngredientsNames = new List<string>(); 
    public List<Vector4> CurveIngredientsAtoms = new List<Vector4>();
    public List<Vector4> CurveIngredientsInfos = new List<Vector4>();
    public List<Vector4> CurveIngredientsColors = new List<Vector4>();

    //*****

    // This serves as a cache to avoid calling GameObject.Find on every update because not efficient
    // The cache will be filled automatically via the CutObject script onEnable
    [NonSerialized]
    public List<CutObject> CutObjects = new List<CutObject>();

    //--------------------------------------------------------------

    public int NumLodLevels = 0;
    public int TotalNumProteinAtoms = 0;

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

    void Update()
    {
        UpdateCutObjects();
    }

    void OnUnityReload()
    {
        Debug.Log("Reload Scene");
        UploadAllData();
    }

    //--------------------------------------------------------------

    #region Ingredients

    public void AddIngredient(string ingredientName, Bounds bounds, List<Vector4> atomSpheres, Color color, List<float> clusterLevels = null,
	                          bool nolod = false)
    {
        if (ProteinNames.Contains(ingredientName)) return;
		if (clusterLevels != null) {
			if (NumLodLevels != 0 && NumLodLevels != clusterLevels.Count)
				throw new Exception ("Uneven cluster levels number: " + ingredientName);
		}
        if (color == null) { color = MyUtility.GetRandomColor(); }
        
        ProteinColors.Add(color);
        ProteinToggleFlags.Add(1);
        ProteinNames.Add(ingredientName);
        ProteinRadii.Add(AtomHelper.ComputeRadius(atomSpheres));

        ProteinAtomCount.Add(atomSpheres.Count);
        ProteinAtomStart.Add(ProteinAtoms.Count);
        ProteinAtoms.AddRange(atomSpheres);

        if (clusterLevels != null) {
			NumLodLevels = clusterLevels.Count;
			foreach (var level in clusterLevels) {
				var numClusters = Math.Max (atomSpheres.Count * level, 5);
				List<Vector4> clusterSpheres;
				if (!nolod)
					clusterSpheres = KMeansClustering.GetClusters (atomSpheres, (int)numClusters);
				else
					clusterSpheres = new List<Vector4>(atomSpheres);
				ProteinAtomClusterCount.Add (clusterSpheres.Count);
				ProteinAtomClusterStart.Add (ProteinAtomClusters.Count);
				ProteinAtomClusters.AddRange (clusterSpheres);
			}
		}
    }

    public void AddIngredientInstance(string ingredientName, Vector3 position, Quaternion rotation, int unitId = 0)
    {
        if (!ProteinNames.Contains(ingredientName))
        {
            throw new Exception("Ingredient type do not exists");
        }

        var ingredientId = ProteinNames.IndexOf(ingredientName);

        ProteinInstanceInfos.Add(new Vector4(ingredientId, (int)InstanceState.Normal, 0));
        ProteinInstancePositions.Add(position);
        ProteinInstanceRotations.Add(MyUtility.QuanternionToVector4(rotation));

        TotalNumProteinAtoms += ProteinAtomCount[ingredientId];
    }

    public void AddCurveIngredient(string name, string pdbName)
    {
        if (ProteinNames.Contains(name)) return;
        
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

            var atomSphere = new Vector4(0,0,0,3);
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

    public void AddCurve(string name, List<Vector4> path)
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

    #endregion
    
    //--------------------------------------------------------------

    #region Cut Objects

    public void AddCutObject(CutType type)
    {
        var gameObject = Instantiate(Resources.Load("Prefabs/CutObjectPrefab"), Vector3.zero, Quaternion.identity) as GameObject;
        var cutObject = gameObject.GetComponent<CutObject>().CutType = type;
    }

    // Todo: proceed only if changes are made 
    public void UpdateCutObjects()
    {
        var CutInfos = new List<Vector4>();
        var CutScales = new List<Vector4>();
        var CutPositions = new List<Vector4>();
        var CutRotations = new List<Vector4>();
        var ProteinCutFilters = new List<int>();

        //Debug.Log(CutObjects.Count);

        // Fill the protein cut filter buffer
        for (var i = 0; i < ProteinNames.Count; i++)
        {
            foreach (var cutObject in CutObjects)
            {
                ProteinCutFilters.Add(Convert.ToInt32(cutObject.ProteinCutFilters[i].State));
            }
        }

        // For each cut object
        foreach (var cut in CutObjects)
        {
            if (cut == null) throw new Exception("Cut object not found");

            CutScales.Add(cut.transform.localScale);
            CutPositions.Add(cut.transform.position);
            CutInfos.Add(new Vector4((float)cut.CutType, cut.Value1, cut.Value2, 0));
            CutRotations.Add(MyUtility.QuanternionToVector4(cut.transform.rotation));
        }

        ComputeBufferManager.Instance.CutInfos.SetData(CutInfos.ToArray());
        ComputeBufferManager.Instance.CutScales.SetData(CutScales.ToArray());
        ComputeBufferManager.Instance.CutPositions.SetData(CutPositions.ToArray());
        ComputeBufferManager.Instance.CutRotations.SetData(CutRotations.ToArray());
        ComputeBufferManager.Instance.ProteinCutFilters.SetData(ProteinCutFilters.ToArray());
    }

    #endregion

    //--------------------------------------------------------------

    #region Misc

    // Scene data gets serialized on each reload, to clear the scene call this function
    public void ClearScene()
    {
        Debug.Log("Clear Scene");

        NumLodLevels = 0;
        TotalNumProteinAtoms = 0;

        // Clear scene data
        ProteinInstanceInfos.Clear();
        ProteinInstancePositions.Clear();
        ProteinInstanceRotations.Clear();

        // Clear ingredient data
        ProteinNames.Clear();
        ProteinColors.Clear();
        ProteinToggleFlags.Clear();
        ProteinRadii.Clear();

        // Clear atom data
        ProteinAtoms.Clear();
        ProteinAtomCount.Clear();
        ProteinAtomStart.Clear();

        // Clear cluster data
        ProteinAtomClusters.Clear();
        ProteinAtomClusterStart.Clear();
        ProteinAtomClusterCount.Clear();

        // Clear curve data
        CurveIngredientsInfos.Clear();
        CurveIngredientsNames.Clear();
        CurveIngredientsColors.Clear();
        CurveIngredientToggleFlags.Clear();
        CurveIngredientsAtoms.Clear();
        CurveIngredientsAtomCount.Clear();
        CurveIngredientsAtomStart.Clear();
        
        CurveControlPointsPositions.Clear();
        CurveControlPointsNormals.Clear();
        CurveControlPointsInfos.Clear();
    }

    private void CheckBufferSizes()
    {
        if (Instance.NumCutObjects >= ComputeBufferManager.NumCutsMax) throw new Exception("GPU buffer overflow");
        //if (Instance.ProteinCutFilters.Count >= ComputeBufferManager.NumCutsMax * ComputeBufferManager.NumProteinMax) throw new Exception("GPU buffer overflow");
        if (Instance.NumLodLevels >= ComputeBufferManager.NumLodMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinNames.Count >= ComputeBufferManager.NumProteinMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinAtoms.Count >= ComputeBufferManager.NumProteinAtomMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinAtomClusters.Count >= ComputeBufferManager.NumProteinAtomClusterMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinAtomClusterCount.Count >= ComputeBufferManager.NumProteinMax * ComputeBufferManager.NumLodMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinInstancePositions.Count >= ComputeBufferManager.NumProteinInstancesMax) throw new Exception("GPU buffer overflow");

        if (Instance.CurveIngredientsNames.Count >= ComputeBufferManager.NumCurveIngredientMax) throw new Exception("GPU buffer overflow");
        if (Instance.CurveControlPointsPositions.Count >= ComputeBufferManager.NumCurveControlPointsMax) throw new Exception("GPU buffer overflow");
        if (Instance.CurveIngredientsAtoms.Count >= ComputeBufferManager.NumCurveIngredientAtomsMax) throw new Exception("GPU buffer overflow");
    }

    public void UploadAllData()
    {
        CheckBufferSizes();

        ComputeBufferManager.Instance.InitBuffers();

        ComputeBufferManager.Instance.LodInfos.SetData(PersistantSettings.Instance.LodLevels);

        // Upload ingredient data
        ComputeBufferManager.Instance.ProteinRadii.SetData(ProteinRadii.ToArray());
        ComputeBufferManager.Instance.ProteinColors.SetData(ProteinColors.ToArray());
        ComputeBufferManager.Instance.ProteinToggleFlags.SetData(ProteinToggleFlags.ToArray());

        ComputeBufferManager.Instance.ProteinAtoms.SetData(ProteinAtoms.ToArray());
        ComputeBufferManager.Instance.ProteinAtomCount.SetData(ProteinAtomCount.ToArray());
        ComputeBufferManager.Instance.ProteinAtomStart.SetData(ProteinAtomStart.ToArray());

        ComputeBufferManager.Instance.ProteinAtomClusters.SetData(ProteinAtomClusters.ToArray());
        ComputeBufferManager.Instance.ProteinAtomClusterCount.SetData(ProteinAtomClusterCount.ToArray());
        ComputeBufferManager.Instance.ProteinAtomClusterStart.SetData(ProteinAtomClusterStart.ToArray());

        ComputeBufferManager.Instance.ProteinInstanceInfos.SetData(ProteinInstanceInfos.ToArray());
        ComputeBufferManager.Instance.ProteinInstancePositions.SetData(ProteinInstancePositions.ToArray());
        ComputeBufferManager.Instance.ProteinInstanceRotations.SetData(ProteinInstanceRotations.ToArray());

        // Upload curve ingredient data
        ComputeBufferManager.Instance.CurveIngredientsAtoms.SetData(CurveIngredientsAtoms.ToArray());
        ComputeBufferManager.Instance.CurveIngredientsAtomCount.SetData(CurveIngredientsAtomCount.ToArray());
        ComputeBufferManager.Instance.CurveIngredientsAtomStart.SetData(CurveIngredientsAtomStart.ToArray());
        
        ComputeBufferManager.Instance.CurveIngredientsInfos.SetData(CurveIngredientsInfos.ToArray());
        ComputeBufferManager.Instance.CurveIngredientsColors.SetData(CurveIngredientsColors.ToArray());
        ComputeBufferManager.Instance.CurveIngredientsToggleFlags.SetData(CurveIngredientToggleFlags.ToArray());

        ComputeBufferManager.Instance.CurveControlPointsInfos.SetData(CurveControlPointsInfos.ToArray());
        ComputeBufferManager.Instance.CurveControlPointsNormals.SetData(CurveControlPointsNormals.ToArray());
        ComputeBufferManager.Instance.CurveControlPointsPositions.SetData(CurveControlPointsPositions.ToArray());
    }

    public void UploadIngredientToggleData()
    {
        ComputeBufferManager.Instance.ProteinToggleFlags.SetData(ProteinToggleFlags.ToArray());
        ComputeBufferManager.Instance.CurveIngredientsToggleFlags.SetData(CurveIngredientToggleFlags.ToArray());
    }

    public void SetCutObjects()
    {
        var cutObjects = FindObjectsOfType<CutObject>();

        foreach (var cutObject in cutObjects)
        {
            cutObject.ProteinCutFilters.Clear();
            cutObject.SetCutItems(ProteinNames);
        }
    }
    
    #endregion
}
