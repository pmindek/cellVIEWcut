using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum InstanceState
{
    Null = -1,           // Instance will not be displayed
    Normal = 0,          // Instance will be displayed with normal color
    Highlighted = 1      // Instance will be displayed with highlighted color
};

[ExecuteInEditMode]
public class SceneManager : MonoBehaviour
{
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
    public List<float> ProteinBoundingSpheres = new List<float>();
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

    //--------------------------------------------------------------

    public int NumLodLevels = 0;
    public int SelectedElement = -1;
    public int TotalNumProteinAtoms = 0;

    public int NumProteinInstances
    {
        get { return ProteinInstancePositions.Count; }
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

    //--------------------------------------------------------------
    
    void Update()
    {

    }

    //--------------------------------------------------------------

    #region Protein_functions

    public void AddIngredient(string ingredientName, Bounds bounds, List<Vector4> atomSpheres, Color color, List<float> clusterLevels = null)
    {
        if (ProteinNames.Contains(ingredientName)) return;

        if (NumLodLevels != 0 && NumLodLevels != clusterLevels.Count)
            throw new Exception("Uneven cluster levels number: " + ingredientName);

        if (color == null) { color = Helper.GetRandomColor(); }
        
        ProteinNames.Add(ingredientName);
        ProteinColors.Add(color);
        ProteinToggleFlags.Add(1);
        ProteinBoundingSpheres.Add(Vector3.Magnitude(bounds.extents));

        ProteinAtomCount.Add(atomSpheres.Count);
        ProteinAtomStart.Add(ProteinAtoms.Count);
        ProteinAtoms.AddRange(atomSpheres);

        if (clusterLevels != null)
        {
            NumLodLevels = clusterLevels.Count;

            foreach (var level in clusterLevels)
            {
                var numClusters = Math.Max(atomSpheres.Count * level, 5);
                var clusterSpheres = KMeansClustering.GetClusters(atomSpheres, (int)numClusters);

                ProteinAtomClusterCount.Add(clusterSpheres.Count);
                ProteinAtomClusterStart.Add(ProteinAtomClusters.Count);
                ProteinAtomClusters.AddRange(clusterSpheres);
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

        Vector4 instancePosition = position;
        instancePosition.w = ProteinBoundingSpheres[ingredientId];

        ProteinInstanceInfos.Add(new Vector4(ingredientId, (int)InstanceState.Normal, unitId));
        ProteinInstancePositions.Add(instancePosition);
        ProteinInstanceRotations.Add(Helper.QuanternionToVector4(rotation));

        TotalNumProteinAtoms += ProteinAtomCount[ingredientId];
    }

    #endregion
    
    #region Curve_functions

    public void AddCurveIngredient(string name, string pdbName)
    {
        if (ProteinNames.Contains(name)) return;
        
        int numSteps = 1;
        float twistAngle = 0;
        float segmentLength = 34.0f;
        var color = Helper.GetRandomColor();

        if (name.Contains("DNA"))
        {
            numSteps = 12;
            twistAngle = 34.3f;
            segmentLength = 34.0f;
            color = Color.yellow;

            var atomSpheres = PdbLoader.LoadAtomSpheres(pdbName);
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

            var atomSpheres = PdbLoader.LoadAtomSpheres(pdbName);
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
        var positions = ResampleControlPoints(path, CurveIngredientsInfos[curveIngredientId].z);
        var normals = GetSmoothNormals(positions);

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
    
    private List<Vector4> ResampleControlPoints(List<Vector4> controlPoints, float segmentLength)
    {
        int nP = controlPoints.Count;
        //insert a point at the end and at the begining
        controlPoints.Insert(0, controlPoints[0] + (controlPoints[0] - controlPoints[1]) / 2.0f);
        controlPoints.Add(controlPoints[nP - 1] + (controlPoints[nP - 1] - controlPoints[nP - 2]) / 2.0f);

        var resampledControlPoints = new List<Vector4>();
        resampledControlPoints.Add(controlPoints[0]);
        resampledControlPoints.Add(controlPoints[1]);

        var currentPointId = 1;
        var currentPosition = controlPoints[currentPointId];

        //distance = DisplaySettings.Instance.DistanceContraint;
        float lerpValue = 0.0f;

        // Normalize the distance between control points
        while (true)
        {
            if (currentPointId + 2 >= controlPoints.Count) break;
            //if (currentPointId + 2 >= 100) break;

            var cp0 = controlPoints[currentPointId - 1];
            var cp1 = controlPoints[currentPointId];
            var cp2 = controlPoints[currentPointId + 1];
            var cp3 = controlPoints[currentPointId + 2];

            var found = false;

            for (; lerpValue <= 1; lerpValue += 0.01f)
            {
                var candidate = Helper.CubicInterpolate(cp0, cp1, cp2, cp3, lerpValue);
                var d = Vector3.Distance(currentPosition, candidate);

                if (d > segmentLength)
                {
                    resampledControlPoints.Add(candidate);
                    currentPosition = candidate;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                lerpValue = 0;
                currentPointId++;
            }
        }

        return resampledControlPoints;
    }

    public List<Vector4> GetSmoothNormals(List<Vector4> controlPoints)
    {
        var smoothNormals = new List<Vector4>();
        var crossDirection = Vector3.up;

        var p0 = controlPoints[0];
        var p1 = controlPoints[1];
        var p2 = controlPoints[2];

        smoothNormals.Add(Vector3.Normalize(Vector3.Cross(p0 - p1, p2 - p1)));

        for (int i = 1; i < controlPoints.Count - 1; i++)
        {
            p0 = controlPoints[i - 1];
            p1 = controlPoints[i];
            p2 = controlPoints[i + 1];

            var t = Vector3.Normalize(p2 - p0);
            var b = Vector3.Normalize(Vector3.Cross(t, smoothNormals.Last()));
            var n = -Vector3.Normalize(Vector3.Cross(t, b));

            smoothNormals.Add(n);
        }

        smoothNormals.Add(controlPoints.Last());

        return smoothNormals;
    }

    #endregion
    
    #region Misc_functions

    public void SetSelectedElement(int elementId)
    {
        Debug.Log("Selected element id: " + elementId);
        SelectedElement = elementId;
    }

    private void OnUnityReload()
    {
        Debug.Log("Reload Scene");

        //_instance.ClearScene();
        _instance.UploadAllData();
    }

    // Scene data gets serialized on each reload, to clear the scene call this function
    public void ClearScene()
    {
        Debug.Log("Clear Scene");

        NumLodLevels = 0;
        SelectedElement = -1;
        TotalNumProteinAtoms = 0;

        // Clear scene data
        ProteinInstanceInfos.Clear();
        ProteinInstancePositions.Clear();
        ProteinInstanceRotations.Clear();

        // Clear ingredient data
        ProteinNames.Clear();
        ProteinColors.Clear();
        ProteinToggleFlags.Clear();
        ProteinBoundingSpheres.Clear();

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

        //ComputeBufferManager.Instance.InitBuffers();
        ComputeBufferManager.Instance.LodInfos.SetData(PersistantSettings.Instance.LodLevels);

        // Upload ingredient data
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

    #endregion
}
