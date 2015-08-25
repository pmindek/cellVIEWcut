using System;
using UnityEngine;

[ExecuteInEditMode]
public class ComputeBufferManager : MonoBehaviour
{
    //public int NumProteinAtomMax = 1000000;        
    //public int NumIngredientsMax = 1000;    
    //public int NumProteinInstancesMax = 25000000;      
    //public int NumProteinSphereBatchesMax = 25000000;  
    //public int NumLipidAtomMax = 8000000;    
    //public int NumLipidInstancesMax = 25000000;
    //public int NumDnaAtomsMax = 1000;
    //public int NumDnaControlPointsMax = 1000000;

    [NonSerialized]
    public int NumProteinAtomMax = 0;

    [NonSerialized]
    public int NumIngredientsMax = 0;

    [NonSerialized]
    public int NumProteinInstancesMax = 0;

    [NonSerialized]
    public int NumProteinSphereBatchesMax = 0;

    [NonSerialized]
    public int NumLipidAtomMax = 0;

    [NonSerialized]
    public int NumLipidMax = 0;

    [NonSerialized]
    public int NumLipidInstancesMax = 0;

    [NonSerialized]
    public int NumDnaAtomsMax = 0;

    [NonSerialized]
    public int NumCurveControlPointsMax = 0;

    public ComputeBuffer LodInfos;
    public ComputeBuffer SphereBatchBuffer;

    // Protein buffers
    public ComputeBuffer ProteinInfos;
    public ComputeBuffer ProteinColors;
    public ComputeBuffer ProteinToggleFlags;

    public ComputeBuffer ProteinAtoms;
    public ComputeBuffer ProteinAtomCount;
    public ComputeBuffer ProteinAtomStart;

    public ComputeBuffer ProteinAtomClusters;
    public ComputeBuffer ProteinAtomClusterCount;
    public ComputeBuffer ProteinAtomClusterStart;

    public ComputeBuffer ProteinInstanceInfos;
    public ComputeBuffer ProteinInstanceCullFlags;
    public ComputeBuffer ProteinInstancePositions;
    public ComputeBuffer ProteinInstanceRotations;

    // Curve ingredients buffers
    public ComputeBuffer CurveIngredientsInfos;
    public ComputeBuffer CurveIngredientsColors;
    public ComputeBuffer CurveIngredientsToggleFlags;

    public ComputeBuffer CurveIngredientsAtoms;
    public ComputeBuffer CurveIngredientsAtomCount;
    public ComputeBuffer CurveIngredientsAtomStart;

    public ComputeBuffer CurveControlPointsInfos;
    public ComputeBuffer CurveControlPointsNormals;
    public ComputeBuffer CurveControlPointsPositions;

    //*****//

    // Declare the buffer manager as a singleton
    private static ComputeBufferManager _instance = null;
    public static ComputeBufferManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ComputeBufferManager>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_ComputeBufferManager");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_ComputeBufferManager") {hideFlags = HideFlags.HideInInspector};
                    _instance = go.AddComponent<ComputeBufferManager>();
                }
            }

            return _instance;
        }
    }

    // Hack to clear append buffer
    public static void ClearAppendBuffer(ComputeBuffer appendBuffer)
    {
        // This resets the append buffer buffer to 0
        var dummy1 = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.ARGB32);
        var dummy2 = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.ARGB32);
        var active = RenderTexture.active;

        Graphics.SetRandomWriteTarget(1, appendBuffer);
        Graphics.Blit(dummy1, dummy2);
        Graphics.ClearRandomWriteTargets();

        RenderTexture.active = active;

        dummy1.Release();
        dummy2.Release();
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
        if (NumProteinInstancesMax == 0) return;

        // Before declaring new buffers make sure that all the previous buffers are cleared
        ReleaseBuffers();

        if (LodInfos == null) LodInfos = new ComputeBuffer(8, 16);
        if (SphereBatchBuffer == null) SphereBatchBuffer = new ComputeBuffer(NumProteinSphereBatchesMax, 16, ComputeBufferType.Append);

        //*****//

        if (ProteinColors == null) ProteinColors = new ComputeBuffer(NumIngredientsMax, 16);
        if (ProteinToggleFlags == null) ProteinToggleFlags = new ComputeBuffer(NumIngredientsMax, 4);

        if (ProteinAtoms == null) ProteinAtoms = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomCount == null) ProteinAtomCount = new ComputeBuffer(NumIngredientsMax, 4);
        if (ProteinAtomStart == null) ProteinAtomStart = new ComputeBuffer(NumIngredientsMax, 4);

        if (ProteinAtomClusters == null) ProteinAtomClusters = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomClusterCount == null) ProteinAtomClusterCount = new ComputeBuffer(NumIngredientsMax, 16);
        if (ProteinAtomClusterStart == null) ProteinAtomClusterStart = new ComputeBuffer(NumIngredientsMax, 16);

        if (ProteinInstanceInfos == null) ProteinInstanceInfos = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceCullFlags == null) ProteinInstanceCullFlags = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstancePositions == null) ProteinInstancePositions = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceRotations == null) ProteinInstanceRotations = new ComputeBuffer(NumProteinInstancesMax, 16);

        //*****//

        if (CurveIngredientsInfos == null) CurveIngredientsInfos = new ComputeBuffer(32, 16);
        if (CurveIngredientsColors == null) CurveIngredientsColors = new ComputeBuffer(32, 16);
        if (CurveIngredientsToggleFlags == null) CurveIngredientsToggleFlags = new ComputeBuffer(32, 4);
        
        if (CurveIngredientsAtomCount == null) CurveIngredientsAtomCount = new ComputeBuffer(32, 4);
        if (CurveIngredientsAtomStart == null) CurveIngredientsAtomStart = new ComputeBuffer(32, 4);
        if (CurveIngredientsAtoms == null) CurveIngredientsAtoms = new ComputeBuffer(NumDnaAtomsMax, 16);
        
        if (CurveControlPointsInfos == null) CurveControlPointsInfos = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsNormals == null) CurveControlPointsNormals = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsPositions == null) CurveControlPointsPositions = new ComputeBuffer(NumCurveControlPointsMax, 16);
	}
	
	// Flush buffers on exit
	void ReleaseBuffers ()
    {
        if (LodInfos != null) { LodInfos.Release(); LodInfos = null; }
        if (SphereBatchBuffer != null) { SphereBatchBuffer.Release(); SphereBatchBuffer = null; }

        //*****//
	    
        if (ProteinColors != null) { ProteinColors.Release(); ProteinColors = null; }
	    if (ProteinToggleFlags != null) { ProteinToggleFlags.Release(); ProteinToggleFlags = null; }
        
        if (ProteinAtoms != null) { ProteinAtoms.Release(); ProteinAtoms = null; }
	    if (ProteinAtomCount != null) { ProteinAtomCount.Release(); ProteinAtomCount = null; }
	    if (ProteinAtomStart != null) { ProteinAtomStart.Release(); ProteinAtomStart = null; }
        
        if (ProteinAtomClusters != null) { ProteinAtomClusters.Release(); ProteinAtomClusters = null; }
	    if (ProteinAtomClusterCount != null) { ProteinAtomClusterCount.Release(); ProteinAtomClusterCount = null; }
	    if (ProteinAtomClusterStart != null) { ProteinAtomClusterStart.Release(); ProteinAtomClusterStart = null; }

        if (ProteinInstanceInfos != null) { ProteinInstanceInfos.Release(); ProteinInstanceInfos = null; }
        if (ProteinInstanceCullFlags != null) { ProteinInstanceCullFlags.Release(); ProteinInstanceCullFlags = null; }
        if (ProteinInstancePositions != null) { ProteinInstancePositions.Release(); ProteinInstancePositions = null; }
        if (ProteinInstanceRotations != null) { ProteinInstanceRotations.Release(); ProteinInstanceRotations = null; }

        //*****//

        if (CurveIngredientsInfos != null) { CurveIngredientsInfos.Release(); CurveIngredientsInfos = null; }
        if (CurveIngredientsColors != null) { CurveIngredientsColors.Release(); CurveIngredientsColors = null; }
        if (CurveIngredientsToggleFlags != null) { CurveIngredientsToggleFlags.Release(); CurveIngredientsToggleFlags = null; }

        if (CurveIngredientsAtoms != null) { CurveIngredientsAtoms.Release(); CurveIngredientsAtoms = null; }
        if (CurveIngredientsAtomCount != null) { CurveIngredientsAtomCount.Release(); CurveIngredientsAtomCount = null; }
        if (CurveIngredientsAtomStart != null) { CurveIngredientsAtomStart.Release(); CurveIngredientsAtomStart = null; }

        if (CurveControlPointsInfos != null) { CurveControlPointsInfos.Release(); CurveControlPointsInfos = null; }
        if (CurveControlPointsNormals != null) { CurveControlPointsNormals.Release(); CurveControlPointsNormals = null; }
        if (CurveControlPointsPositions != null) { CurveControlPointsPositions.Release(); CurveControlPointsPositions = null; }
	}
}
