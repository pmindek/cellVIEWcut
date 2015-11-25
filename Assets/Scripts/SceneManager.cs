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
}

struct CutInfoStruct
{
    public Vector4 info;
    public Vector4 info2;
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

    public List<int> HistogramsLookup = new List<int>();

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

    public List<HistStruct> Histograms = new List<HistStruct>();

    public int[] stats = new int[] { 0, 0, 0, 0 };

    public HistStruct[] histograms;

    //--------------------------------------------------------------

    public int NumLodLevels = 0;
    public int TotalNumProteinAtoms = 0;


    public bool isUpdated = false;

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
        var CutInfos = new List<CutInfoStruct>();
        var CutScales = new List<Vector4>();
        var CutPositions = new List<Vector4>();
        var CutRotations = new List<Vector4>();
        var ProteinCutFilters = new List<int>();
        var HistogramProteinTypes = new List<int>();

        //Debug.Log(CutObjects.Count);

        // Fill the protein cut filter buffer

        for (var i = 0; i < ProteinNames.Count; i++)
        {
            foreach (var cutObject in CutObjects)
            {
                //Debug.Log(i + " PCF " + cutObject.ProteinCutFilters.Count());
                //Debug.Log(i + " HPT " + cutObject.HistogramProteinTypes.Count());
                ProteinCutFilters.Add(Convert.ToInt32(cutObject.ProteinCutFilters[i].State));
                HistogramProteinTypes.Add(Convert.ToInt32(cutObject.HistogramProteinTypes[i].State));

                //Debug.Log("---" + i + " -- " + " ~ " + cutObject.HistogramProteinTypes[i].Name + " ~ " + cutObject.HistogramProteinTypes[i].State + " ... " + cutObject.ProteinCutFilters[i].State);
            }
        }

        // For each cut object
        foreach (var cut in CutObjects)
        {
            if (cut == null) throw new Exception("Cut object not found");

            CutScales.Add(cut.transform.localScale);
            CutPositions.Add(cut.transform.position);
            CutRotations.Add(MyUtility.QuanternionToVector4(cut.transform.rotation));

            //CutInfos.Add(new Vector4((float)cut.CutType, cut.Value1, cut.Value2, cut.Inverse ? 1.0f : 0.0f));
        }

        for (int i = 0; i < ProteinNames.Count; i++)
        {
            foreach (var cut in CutObjects)
            {
                if (i < cut.ProteinTypeParameters.Count)
                {
                    CutInfos.Add(new CutInfoStruct
                    {
                        info = new Vector4((float)cut.CutType, cut.ProteinTypeParameters[i].value1, cut.ProteinTypeParameters[i].value2, cut.Inverse ? 1.0f : 0.0f),
                        info2 = new Vector4((float)cut.ProteinTypeParameters[i].fuzziness, (float)cut.ProteinTypeParameters[i].fuzzinessDistance, (float)cut.ProteinTypeParameters[i].fuzzinessCurve, 0.0f)

                        /*info = new Vector4((float)cut.CutType, cut.Value1, cut.Value2, cut.Inverse ? 1.0f : 0.0f),
                        info2 = new Vector4((float)cut.Fuzziness, (float)cut.FuzzinessDistance, (float)cut.FuzzinessCurve, 0.0f)*/
                    });
                }
                else
                {
                    CutInfos.Add(new CutInfoStruct { info = new Vector4(), info2 = new Vector4() });
                }
            }
        }

        GPUBuffer.Instance.CutInfos.SetData(CutInfos.ToArray());
        GPUBuffer.Instance.CutScales.SetData(CutScales.ToArray());
        GPUBuffer.Instance.CutPositions.SetData(CutPositions.ToArray());
        GPUBuffer.Instance.CutRotations.SetData(CutRotations.ToArray());
        GPUBuffer.Instance.ProteinCutFilters.SetData(ProteinCutFilters.ToArray());
        GPUBuffer.Instance.HistogramProteinTypes.SetData(HistogramProteinTypes.ToArray());
        //GPUBuffer.Instance.HistogramStatistics.SetData(new[] { 0, 1, 2, 3 });
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
        if (Instance.NumCutObjects >= GPUBuffer.NumCutsMax) throw new Exception("GPU buffer overflow");
        //if (Instance.ProteinCutFilters.Count >= ComputeBufferManager.NumCutsMax * ComputeBufferManager.NumProteinMax) throw new Exception("GPU buffer overflow");
        if (Instance.NumLodLevels >= GPUBuffer.NumLodMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinNames.Count >= GPUBuffer.NumProteinMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinAtoms.Count >= GPUBuffer.NumProteinAtomMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinAtomClusters.Count >= GPUBuffer.NumProteinAtomClusterMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinAtomClusterCount.Count >= GPUBuffer.NumProteinMax * GPUBuffer.NumLodMax) throw new Exception("GPU buffer overflow");
        if (Instance.ProteinInstancePositions.Count >= GPUBuffer.NumProteinInstancesMax) throw new Exception("GPU buffer overflow");

        if (Instance.CurveIngredientsNames.Count >= GPUBuffer.NumCurveIngredientMax) throw new Exception("GPU buffer overflow");
        if (Instance.CurveControlPointsPositions.Count >= GPUBuffer.NumCurveControlPointsMax) throw new Exception("GPU buffer overflow");
        if (Instance.CurveIngredientsAtoms.Count >= GPUBuffer.NumCurveIngredientAtomsMax) throw new Exception("GPU buffer overflow");
    }

    public void UploadAllData()
    {
        CheckBufferSizes();

        GPUBuffer.Instance.InitBuffers();

        HistogramsLookup.Clear();

        Debug.Log("NOW UPDATING");
        //HistogramsLookup.Add(0);

        foreach (var node in PersistantSettings.Instance.hierachy)
        {
            HistStruct hist = new HistStruct();
            hist.parent = -1;
            hist.all = 0;
            hist.cutaway = 0;
            hist.occluding = 0;
            hist.visible = 0;

            string parentPath = TreeUtility.GetNodeParentPath(node.path);

            if (string.IsNullOrEmpty(parentPath))
            {
                hist.parent = -1;
            }
            else
            {
                int index = 0;
                foreach (var node0 in PersistantSettings.Instance.hierachy)
                {
                    if (parentPath == node0.path)
                    {
                        hist.parent = index;
                        break;
                    }
                    index++;
                }
            }

            Histograms.Add(hist);

            //TreeViewController.AddNodeObject(node.path, new object[] { node.name }, "Text");
            Debug.Log(node.path + " ~~ " + node.name);
            Debug.Log(":::::::: " + hist.parent);
        }

        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");


        foreach (var name in ProteinNames)
        {
            /*if (name.Contains("cytoplasme") || name.Contains("membrane") || name.Contains("surface") ||
                name.Contains("interior"))
            {
                HistogramsLookup.Add(-1);
            }
            else*/
            {
                string[] parts = name.Split(new char[] { '_' }, 2);

                if (parts.Length > 1)
                {
                    Debug.Log("////////// " + parts[1]);

                    int index = 0;
                    foreach (var node in PersistantSettings.Instance.hierachy)
                    {
                        if (node.name == parts[1] && !HistogramsLookup.Contains(index))
                        {
                            break;
                        }
                        index++;
                    }

                    Debug.Log("found at " + index);

                    HistogramsLookup.Add(index);
                }
            }

            Debug.Log(name);
        }

        Debug.Log("-------------------------------------------------------------");

        foreach (var lk in HistogramsLookup)
        {
            Debug.Log(lk);
        }


        GPUBuffer.Instance.HistogramStatistics.SetData(new[] { 0, 0, 0, 4 });

        //todo - fill histograms lookup
        GPUBuffer.Instance.HistogramsLookup.SetData(HistogramsLookup.ToArray());
        GPUBuffer.Instance.Histograms.SetData(Histograms.ToArray());

        GPUBuffer.Instance.LodInfos.SetData(PersistantSettings.Instance.LodLevels);

        // Upload ingredient data
        GPUBuffer.Instance.ProteinRadii.SetData(ProteinRadii.ToArray());
        GPUBuffer.Instance.ProteinColors.SetData(ProteinColors.ToArray());
        GPUBuffer.Instance.ProteinToggleFlags.SetData(ProteinToggleFlags.ToArray());

        GPUBuffer.Instance.ProteinAtoms.SetData(ProteinAtoms.ToArray());
        GPUBuffer.Instance.ProteinAtomCount.SetData(ProteinAtomCount.ToArray());
        GPUBuffer.Instance.ProteinAtomStart.SetData(ProteinAtomStart.ToArray());

        GPUBuffer.Instance.ProteinAtomClusters.SetData(ProteinAtomClusters.ToArray());
        GPUBuffer.Instance.ProteinAtomClusterCount.SetData(ProteinAtomClusterCount.ToArray());
        GPUBuffer.Instance.ProteinAtomClusterStart.SetData(ProteinAtomClusterStart.ToArray());

        GPUBuffer.Instance.ProteinInstanceInfos.SetData(ProteinInstanceInfos.ToArray());
        GPUBuffer.Instance.ProteinInstancePositions.SetData(ProteinInstancePositions.ToArray());
        GPUBuffer.Instance.ProteinInstanceRotations.SetData(ProteinInstanceRotations.ToArray());

        // Upload curve ingredient data
        GPUBuffer.Instance.CurveIngredientsAtoms.SetData(CurveIngredientsAtoms.ToArray());
        GPUBuffer.Instance.CurveIngredientsAtomCount.SetData(CurveIngredientsAtomCount.ToArray());
        GPUBuffer.Instance.CurveIngredientsAtomStart.SetData(CurveIngredientsAtomStart.ToArray());
        
        GPUBuffer.Instance.CurveIngredientsInfos.SetData(CurveIngredientsInfos.ToArray());
        GPUBuffer.Instance.CurveIngredientsColors.SetData(CurveIngredientsColors.ToArray());
        GPUBuffer.Instance.CurveIngredientsToggleFlags.SetData(CurveIngredientToggleFlags.ToArray());

        GPUBuffer.Instance.CurveControlPointsInfos.SetData(CurveControlPointsInfos.ToArray());
        GPUBuffer.Instance.CurveControlPointsNormals.SetData(CurveControlPointsNormals.ToArray());
        GPUBuffer.Instance.CurveControlPointsPositions.SetData(CurveControlPointsPositions.ToArray());
    }

    public void UploadIngredientToggleData()
    {
        GPUBuffer.Instance.ProteinToggleFlags.SetData(ProteinToggleFlags.ToArray());
        GPUBuffer.Instance.CurveIngredientsToggleFlags.SetData(CurveIngredientToggleFlags.ToArray());
    }

    public void SetCutObjects()
    {
        var cutObjects = FindObjectsOfType<CutObject>();

        foreach (var cutObject in cutObjects)
        {
            cutObject.ProteinCutFilters.Clear();
            cutObject.HistogramProteinTypes.Clear();
            cutObject.HistogramRanges.Clear();

            cutObject.SetCutItems(ProteinNames);
        }
    }
    
    #endregion

    public int GetProteinId(String name)
    {
        int index = 0;

        foreach (var node in PersistantSettings.Instance.hierachy)
        {
            if (node.name == name)
            {
                return index;
            }

            index++;
        }

        return -1;
    }
}
