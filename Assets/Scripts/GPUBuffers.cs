using System;
using UnityEngine;

[ExecuteInEditMode]
public class GPUBuffers : MonoBehaviour
{

    public static int NumIngredientMax = 100;
    public static int NumSceneHierarchyNodes = 200;

    public static int NumLipidAtomMax = 10000000;
    public static int NumLipidInstancesMax = 1000000;

    //cutaways
    public static int NumCutsMax = 100;

    public static int NumLodMax = 10;
    public static int NumProteinMax = 100;
    public static int NumProteinAtomMax = 3000000;
    public static int NumProteinAtomClusterMax = 100000;
    public static int NumProteinInstancesMax = 100000;
    public static int NumProteinSphereBatchesMax = 1000000;

    public static int NumCurveIngredientMax = 10;
    public static int NumCurveIngredientAtomsMax = 1000;
    public static int NumCurveControlPointsMax = 1000000;

    public ComputeBuffer LodInfo;
    public ComputeBuffer SphereBatches;
    
    public ComputeBuffer IngredientStates;
    public ComputeBuffer IngredientMaskParams;

    // Protein buffers
    public ComputeBuffer ProteinRadii;
    public ComputeBuffer ProteinColors;

    public ComputeBuffer ProteinAtoms;
    public ComputeBuffer ProteinAtomCount;
    public ComputeBuffer ProteinAtomStart;

    public ComputeBuffer ProteinAtomClusters;
    public ComputeBuffer ProteinAtomClusterCount;
    public ComputeBuffer ProteinAtomClusterStart;

    public ComputeBuffer ProteinInstanceInfo;
    public ComputeBuffer ProteinInstancePositions;
    public ComputeBuffer ProteinInstanceRotations;
    public ComputeBuffer ProteinInstanceCullFlags;
    public ComputeBuffer ProteinInstanceOcclusionFlags;
    public ComputeBuffer ProteinInstanceVisibilityFlags;

    // lipid buffers
    public ComputeBuffer LipidAtomPositions;
    public ComputeBuffer LipidInstanceInfo;
    public ComputeBuffer LipidInstancePositions;
    public ComputeBuffer LipidInstanceCullFlags;
    public ComputeBuffer LipidInstanceOcclusionFlags;
    public ComputeBuffer LipidInstanceVisibilityFlags;

    // Curve ingredients buffers
    public ComputeBuffer CurveIngredientsInfo;
    public ComputeBuffer CurveIngredientsColors;
    public ComputeBuffer CurveIngredientsToggleFlags;

    public ComputeBuffer CurveIngredientsAtoms;
    public ComputeBuffer CurveIngredientsAtomCount;
    public ComputeBuffer CurveIngredientsAtomStart;

    public ComputeBuffer CurveControlPointsInfo;
    public ComputeBuffer CurveControlPointsNormals;
    public ComputeBuffer CurveControlPointsPositions;

    // Cut Objects
    public ComputeBuffer CutItems;
    public ComputeBuffer CutInfo;
    public ComputeBuffer CutScales;
    public ComputeBuffer CutPositions;
    public ComputeBuffer CutRotations;
    //public ComputeBuffer ProteinCutFilters;
    //public ComputeBuffer HistogramProteinTypes;
    //public ComputeBuffer HistogramStatistics;
    public ComputeBuffer HistogramsLookup;
    public ComputeBuffer Histograms;

    //*****//

    // Declare the buffer manager as a singleton
    private static GPUBuffers _instance = null;
    public static GPUBuffers Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GPUBuffers>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_ComputeBufferManager");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_ComputeBufferManager") {hideFlags = HideFlags.HideInInspector};
                    _instance = go.AddComponent<GPUBuffers>();
                }
            }
            return _instance;
        }
    }

    void OnEnable()
    {
        InitBuffers();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }
    
    public void InitBuffers ()
    {
        if (LodInfo == null) LodInfo = new ComputeBuffer(8, 16);
        if (SphereBatches == null) SphereBatches = new ComputeBuffer(NumProteinSphereBatchesMax, 16, ComputeBufferType.Append);

        if (IngredientStates == null) IngredientStates = new ComputeBuffer(NumIngredientMax, 4);
        if (IngredientMaskParams == null) IngredientMaskParams = new ComputeBuffer(NumIngredientMax, 4);

        //*****//
        if (ProteinRadii == null) ProteinRadii = new ComputeBuffer(NumProteinMax, 4);
        if (ProteinColors == null) ProteinColors = new ComputeBuffer(NumProteinMax, 16);

        if (ProteinAtoms == null) ProteinAtoms = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomClusters == null) ProteinAtomClusters = new ComputeBuffer(NumProteinAtomClusterMax, 16);

        if (ProteinAtomCount == null) ProteinAtomCount = new ComputeBuffer(NumProteinMax, 4);
        if (ProteinAtomStart == null) ProteinAtomStart = new ComputeBuffer(NumProteinMax, 4);
        if (ProteinAtomClusterCount == null) ProteinAtomClusterCount = new ComputeBuffer(NumProteinMax * NumLodMax, 4);
        if (ProteinAtomClusterStart == null) ProteinAtomClusterStart = new ComputeBuffer(NumProteinMax * NumLodMax, 4);

        if (ProteinInstanceInfo == null) ProteinInstanceInfo = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstancePositions == null) ProteinInstancePositions = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceRotations == null) ProteinInstanceRotations = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceCullFlags == null) ProteinInstanceCullFlags = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstanceOcclusionFlags == null) ProteinInstanceOcclusionFlags = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstanceVisibilityFlags == null) ProteinInstanceVisibilityFlags = new ComputeBuffer(NumProteinInstancesMax, 4);

        //*****//

        if (LipidAtomPositions == null) LipidAtomPositions = new ComputeBuffer(NumLipidAtomMax, 16);
        if (LipidInstanceInfo == null) LipidInstanceInfo = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstancePositions == null) LipidInstancePositions = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstanceCullFlags == null) LipidInstanceCullFlags = new ComputeBuffer(NumLipidInstancesMax, 4);
        if (LipidInstanceOcclusionFlags == null) LipidInstanceOcclusionFlags = new ComputeBuffer(NumLipidInstancesMax, 4);
        if (LipidInstanceVisibilityFlags == null) LipidInstanceVisibilityFlags = new ComputeBuffer(NumLipidInstancesMax, 4);

        //*****//

        if (CurveIngredientsInfo == null) CurveIngredientsInfo = new ComputeBuffer(NumCurveIngredientMax, 16);
        if (CurveIngredientsColors == null) CurveIngredientsColors = new ComputeBuffer(NumCurveIngredientMax, 16);
        if (CurveIngredientsToggleFlags == null) CurveIngredientsToggleFlags = new ComputeBuffer(NumCurveIngredientMax, 4);

        if (CurveIngredientsAtomCount == null) CurveIngredientsAtomCount = new ComputeBuffer(NumCurveIngredientMax, 4);
        if (CurveIngredientsAtomStart == null) CurveIngredientsAtomStart = new ComputeBuffer(NumCurveIngredientMax, 4);
        if (CurveIngredientsAtoms == null) CurveIngredientsAtoms = new ComputeBuffer(NumCurveIngredientAtomsMax, 16);
        
        if (CurveControlPointsInfo == null) CurveControlPointsInfo = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsNormals == null) CurveControlPointsNormals = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsPositions == null) CurveControlPointsPositions = new ComputeBuffer(NumCurveControlPointsMax, 16);

        //*****//
        
        if (CutInfo == null) CutInfo = new ComputeBuffer(NumCutsMax * NumProteinMax, 32);
        if (CutScales == null) CutScales = new ComputeBuffer(NumCutsMax, 16);
        if (CutPositions == null) CutPositions = new ComputeBuffer(NumCutsMax, 16);
        if (CutRotations == null) CutRotations = new ComputeBuffer(NumCutsMax, 16);
        //if (ProteinCutFilters == null) ProteinCutFilters = new ComputeBuffer(NumCutsMax * NumProteinMax, 4);
        //if (HistogramProteinTypes == null) HistogramProteinTypes = new ComputeBuffer(NumCutsMax * NumProteinMax, 4);
        //if (HistogramStatistics == null) HistogramStatistics = new ComputeBuffer(4, 4);
        if (HistogramsLookup == null) HistogramsLookup = new ComputeBuffer(NumProteinMax, 4);
        if (Histograms == null) Histograms = new ComputeBuffer(NumSceneHierarchyNodes, 32);

    }
	
	// Flush buffers on exit
	void ReleaseBuffers ()
    {
        // Cutaways
        if (CutInfo != null) { CutInfo.Release(); CutInfo = null; }
        if (CutScales != null) { CutScales.Release(); CutScales = null; }
        if (CutPositions != null) { CutPositions.Release(); CutPositions = null; }
        if (CutRotations != null) { CutRotations.Release(); CutRotations = null; }
        //if (ProteinCutFilters != null) { ProteinCutFilters.Release(); ProteinCutFilters = null; }
        //if (HistogramProteinTypes != null) { HistogramProteinTypes.Release(); HistogramProteinTypes = null; }
        //if (HistogramStatistics != null) { HistogramStatistics.Release(); HistogramStatistics = null; }
        if (HistogramsLookup != null) { HistogramsLookup.Release(); HistogramsLookup = null; }
        if (Histograms != null) { Histograms.Release(); Histograms = null; }

        //*****//

        if (LodInfo != null) { LodInfo.Release(); LodInfo = null; }
        if (SphereBatches != null) { SphereBatches.Release(); SphereBatches = null; }
        
        if (IngredientStates != null) { IngredientStates.Release(); IngredientStates = null; }
        if (IngredientMaskParams != null) { IngredientMaskParams.Release(); IngredientMaskParams = null; }

        //*****//

        if (ProteinRadii != null) { ProteinRadii.Release(); ProteinRadii = null; }
        if (ProteinColors != null) { ProteinColors.Release(); ProteinColors = null; }
        
        if (ProteinAtoms != null) { ProteinAtoms.Release(); ProteinAtoms = null; }
	    if (ProteinAtomCount != null) { ProteinAtomCount.Release(); ProteinAtomCount = null; }
	    if (ProteinAtomStart != null) { ProteinAtomStart.Release(); ProteinAtomStart = null; }
        
        if (ProteinAtomClusters != null) { ProteinAtomClusters.Release(); ProteinAtomClusters = null; }
	    if (ProteinAtomClusterCount != null) { ProteinAtomClusterCount.Release(); ProteinAtomClusterCount = null; }
	    if (ProteinAtomClusterStart != null) { ProteinAtomClusterStart.Release(); ProteinAtomClusterStart = null; }

        if (ProteinInstanceInfo != null) { ProteinInstanceInfo.Release(); ProteinInstanceInfo = null; }
        if (ProteinInstancePositions != null) { ProteinInstancePositions.Release(); ProteinInstancePositions = null; }
        if (ProteinInstanceRotations != null) { ProteinInstanceRotations.Release(); ProteinInstanceRotations = null; }
        if (ProteinInstanceCullFlags != null) { ProteinInstanceCullFlags.Release(); ProteinInstanceCullFlags = null; }
        if (ProteinInstanceOcclusionFlags != null) { ProteinInstanceOcclusionFlags.Release(); ProteinInstanceOcclusionFlags = null; }
        if (ProteinInstanceVisibilityFlags != null) { ProteinInstanceVisibilityFlags.Release(); ProteinInstanceVisibilityFlags = null; }

        //*****//

        if (CurveIngredientsInfo != null) { CurveIngredientsInfo.Release(); CurveIngredientsInfo = null; }
        if (CurveIngredientsColors != null) { CurveIngredientsColors.Release(); CurveIngredientsColors = null; }
        if (CurveIngredientsToggleFlags != null) { CurveIngredientsToggleFlags.Release(); CurveIngredientsToggleFlags = null; }

        if (CurveIngredientsAtoms != null) { CurveIngredientsAtoms.Release(); CurveIngredientsAtoms = null; }
        if (CurveIngredientsAtomCount != null) { CurveIngredientsAtomCount.Release(); CurveIngredientsAtomCount = null; }
        if (CurveIngredientsAtomStart != null) { CurveIngredientsAtomStart.Release(); CurveIngredientsAtomStart = null; }

        if (CurveControlPointsInfo != null) { CurveControlPointsInfo.Release(); CurveControlPointsInfo = null; }
        if (CurveControlPointsNormals != null) { CurveControlPointsNormals.Release(); CurveControlPointsNormals = null; }
        if (CurveControlPointsPositions != null) { CurveControlPointsPositions.Release(); CurveControlPointsPositions = null; }

        //*****//

        if (LipidAtomPositions != null) { LipidAtomPositions.Release(); LipidAtomPositions = null; }
        if (LipidInstanceInfo != null) { LipidInstanceInfo.Release(); LipidInstanceInfo = null; }
        if (LipidInstancePositions != null) { LipidInstancePositions.Release(); LipidInstancePositions = null; }
        if (LipidInstanceCullFlags != null) { LipidInstanceCullFlags.Release(); LipidInstanceCullFlags = null; }
        if (LipidInstanceOcclusionFlags != null) { LipidInstanceOcclusionFlags.Release(); LipidInstanceOcclusionFlags = null; }
        if (LipidInstanceVisibilityFlags != null) { LipidInstanceVisibilityFlags.Release(); LipidInstanceVisibilityFlags = null; }
    }
}
