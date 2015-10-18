using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SceneRenderer : MonoBehaviour
{
    public int occludeeType = 0;

    public Material _contourMaterial;
    public Material _compositeMaterial;
    public Material _renderProteinsMaterial;
    public Material _renderCurveIngredientsMaterial;

    /*****/

    private int frameCount = 0;
    private RenderTexture _HiZMap;
    private RenderTexture _itemBuffer;
    private ComputeBuffer _argBuffer;
    private ComputeBuffer _proteinInstanceCullFlags;

    /*****/

    public Texture2D noiseTexture = null;
    
    void OnEnable()
    {
        this.hideFlags = HideFlags.None;

        if (_argBuffer == null)
        {
            _argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
            _argBuffer.SetData(new[] { 0, 1, 0, 0 });            
        }

        if (_proteinInstanceCullFlags == null) _proteinInstanceCullFlags = new ComputeBuffer(GPUBuffer.NumProteinInstancesMax, 4);
    }

    void OnDisable()
    {
        
        if (_HiZMap != null)
        {
            _HiZMap.Release();
            DestroyImmediate(_HiZMap);
            _HiZMap = null;
        }

        if (_argBuffer != null)
        {
            _argBuffer.Release();
            _argBuffer = null;
        }

        if (_proteinInstanceCullFlags != null)
        {
            _proteinInstanceCullFlags.Release();
            _proteinInstanceCullFlags = null;
        }
    }

    void SetContourShaderParams()
    {
        // Contour params
        _contourMaterial.SetInt("_ContourOptions", PersistantSettings.Instance.ContourOptions);
        _contourMaterial.SetFloat("_ContourStrength", PersistantSettings.Instance.ContourStrength);
    }

    void SetCurveShaderParams()
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_0", MyUtility.PlaneToVector4(planes[0]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_1", MyUtility.PlaneToVector4(planes[1]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_2", MyUtility.PlaneToVector4(planes[2]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_3", MyUtility.PlaneToVector4(planes[3]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_4", MyUtility.PlaneToVector4(planes[4]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_5", MyUtility.PlaneToVector4(planes[5]));


        _renderCurveIngredientsMaterial.SetInt("_NumSegments", SceneManager.Instance.NumDnaControlPoints);
        _renderCurveIngredientsMaterial.SetInt("_EnableTwist", 1);

        _renderCurveIngredientsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        _renderCurveIngredientsMaterial.SetFloat("_SegmentLength", PersistantSettings.Instance.DistanceContraint);
        _renderCurveIngredientsMaterial.SetInt("_EnableCrossSection", Convert.ToInt32(PersistantSettings.Instance.EnableCrossSection));
        _renderCurveIngredientsMaterial.SetVector("_CrossSectionPlane", new Vector4(PersistantSettings.Instance.CrossSectionPlaneNormal.x, PersistantSettings.Instance.CrossSectionPlaneNormal.y, PersistantSettings.Instance.CrossSectionPlaneNormal.z, PersistantSettings.Instance.CrossSectionPlaneDistance));

        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsInfos", GPUBuffer.Instance.CurveIngredientsInfos);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsColors", GPUBuffer.Instance.CurveIngredientsColors);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsToggleFlags", GPUBuffer.Instance.CurveIngredientsToggleFlags);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtoms", GPUBuffer.Instance.CurveIngredientsAtoms);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomCount", GPUBuffer.Instance.CurveIngredientsAtomCount);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomStart", GPUBuffer.Instance.CurveIngredientsAtomStart);

        _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsInfos", GPUBuffer.Instance.CurveControlPointsInfos);
        _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsNormals", GPUBuffer.Instance.CurveControlPointsNormals);
        _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPoints", GPUBuffer.Instance.CurveControlPointsPositions);
    }

    private void SetProteinShaderParams()
    {
        // Protein params
        _renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        _renderProteinsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        _renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", PersistantSettings.Instance.FirstLevelOffset);
        _renderProteinsMaterial.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);

        _renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffer.Instance.LodInfos);
        _renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos);
        _renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions);
        _renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffer.Instance.ProteinInstanceRotations);

        _renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffer.Instance.ProteinColors);
        _renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffer.Instance.ProteinAtoms);
        _renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffer.Instance.ProteinAtomClusters);
        _renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffer.Instance.SphereBatchBuffer);
    }

    //void ComputeDNAStrands()
    //{
    //    if (!DisplaySettings.Instance.EnableDNAConstraints) return;

    //    int numSegments = SceneManager.Instance.NumDnaSegments;
    //    int numSegmentPairs1 = (int)Mathf.Ceil(numSegments / 2.0f);
    //    int numSegmentPairs2 = (int)Mathf.Ceil(numSegments / 4.0f);

    //    RopeConstraintsCS.SetFloat("_DistanceMin", DisplaySettings.Instance.AngularConstraint);
    //    RopeConstraintsCS.SetFloat("_DistanceMax", DisplaySettings.Instance.DistanceContraint);
    //    RopeConstraintsCS.SetInt("_NumControlPoints", SceneManager.Instance.NumDnaControlPoints);

    //    // Do distance constraints
    //    RopeConstraintsCS.SetInt("_Offset", 0);
    //    RopeConstraintsCS.SetBuffer(0, "_DnaControlPoints", ComputeBufferManager.Instance.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(0, (int)Mathf.Ceil(numSegmentPairs1 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 1);
    //    RopeConstraintsCS.SetBuffer(0, "_DnaControlPoints", ComputeBufferManager.Instance.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(0, (int)Mathf.Ceil(numSegmentPairs1 / 16.0f), 1, 1);

    //    // Do bending constraints
    //    RopeConstraintsCS.SetInt("_Offset", 0);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Instance.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 1);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Instance.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 2);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Instance.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);

    //    RopeConstraintsCS.SetInt("_Offset", 3);
    //    RopeConstraintsCS.SetBuffer(1, "_DnaControlPoints", ComputeBufferManager.Instance.DnaControlPointsPositions);
    //    RopeConstraintsCS.Dispatch(1, (int)Mathf.Ceil(numSegmentPairs2 / 16.0f), 1, 1);
    //}

    void ComputeHiZMap(RenderTexture depthBuffer)
    {
        if (GetComponent<Camera>().pixelWidth == 0 || GetComponent<Camera>().pixelHeight == 0) return;

        // Hierachical depth buffer
        if (_HiZMap == null || _HiZMap.width != GetComponent<Camera>().pixelWidth || _HiZMap.height != GetComponent<Camera>().pixelHeight )
        {
            
            if (_HiZMap != null)
            {
                _HiZMap.Release();
                DestroyImmediate(_HiZMap);
                _HiZMap = null;
            }

            _HiZMap = new RenderTexture(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.RFloat);
            _HiZMap.enableRandomWrite = true;
            _HiZMap.useMipMap = false;
            _HiZMap.isVolume = true;
            _HiZMap.volumeDepth = 24;
            //_HiZMap.filterMode = FilterMode.Point;
            _HiZMap.wrapMode = TextureWrapMode.Clamp;
            _HiZMap.hideFlags = HideFlags.HideAndDontSave;
            _HiZMap.Create();
        }

        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<Camera>().pixelWidth);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<Camera>().pixelHeight);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(0, "_RWHiZMap", _HiZMap);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(0, "_DepthBuffer", depthBuffer);
        ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(0, (int)Mathf.Ceil(GetComponent<Camera>().pixelWidth / 8.0f), (int)Mathf.Ceil(GetComponent<Camera>().pixelHeight / 8.0f), 1);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(1, "_RWHiZMap", _HiZMap);
        for (int i = 1; i < 12; i++)
        {
            ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CurrentLevel", i);
            ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(1, (int)Mathf.Ceil(GetComponent<Camera>().pixelWidth / 8.0f), (int)Mathf.Ceil(GetComponent<Camera>().pixelHeight / 8.0f), 1);
        }
    }

    void ComputeOcclusionCulling(int cullingFilter)
    {
        if (_HiZMap == null || PersistantSettings.Instance.DebugObjectCulling) return;

        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CullingFilter", cullingFilter);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<Camera>().pixelWidth);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<Camera>().pixelHeight);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraViewMatrix", MyUtility.Matrix4X4ToFloatArray(GetComponent<Camera>().worldToCameraMatrix));
        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraProjMatrix", MyUtility.Matrix4X4ToFloatArray(GL.GetGPUProjectionMatrix(GetComponent<Camera>().projectionMatrix, false)));

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(2, "_HiZMap", _HiZMap);

        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinRadii", GPUBuffer.Instance.ProteinRadii);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceInfos", GPUBuffer.Instance.ProteinInstanceInfos);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceCullFlags", _proteinInstanceCullFlags);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions);
            ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(2, SceneManager.Instance.NumProteinInstances, 1, 1);
        }
    }

    void ProteinFillBatchBuffer(int cullingFilter)
    {
        if (SceneManager.Instance.NumProteinInstances <= 0) return;

        // Do sphere batching
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_CullingFilter", cullingFilter);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Instance.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", GetComponent<Camera>().transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_LodLevelsInfos", GPUBuffer.Instance.LodInfos);        
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));

        // Do protein batching
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinRadii", GPUBuffer.Instance.ProteinRadii);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinVisibilityFlag", GPUBuffer.Instance.ProteinToggleFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinSphereBatchInfos", GPUBuffer.Instance.SphereBatchBuffer);
                        
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomCount", GPUBuffer.Instance.ProteinAtomCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomStart", GPUBuffer.Instance.ProteinAtomStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterCount", GPUBuffer.Instance.ProteinAtomClusterCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterStart", GPUBuffer.Instance.ProteinAtomClusterStart);

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceCullFlags", _proteinInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffer.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffer.Instance.ProteinInstanceOcclusionFlags);
        

        // Cutaway
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumCutObjects", SceneManager.Instance.NumCutObjects);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutInfos", GPUBuffer.Instance.CutInfos);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutScales", GPUBuffer.Instance.CutScales);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutPositions", GPUBuffer.Instance.CutPositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutRotations", GPUBuffer.Instance.CutRotations);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinCutFilters", GPUBuffer.Instance.ProteinCutFilters);

        if (noiseTexture == null)
        {
            Debug.Log("Load Texture");
            noiseTexture = (Texture2D)Resources.Load("Textures/noise");
        }

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("noiseTextureW", noiseTexture.width);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("noiseTextureH", noiseTexture.height);
        ComputeShaderManager.Instance.SphereBatchCS.SetTexture(0, "noiseTexture", noiseTexture);

        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(0, SceneManager.Instance.NumProteinInstances, 1, 1);

        // Count sphere batches
        ComputeBuffer.CopyCount(GPUBuffer.Instance.SphereBatchBuffer, _argBuffer, 0);
    }

    int GetBatchCount()
    {
        var batchCount = new int[1];
        _argBuffer.GetData(batchCount);
        return batchCount[0];
    }
    
    void ComputeVisibility()
    {
        // Clear Buffer
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffer.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);

        // Compute item visibility
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetTexture(1, "_ItemBuffer", _itemBuffer);
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(1, "_ProteinInstanceVisibilityFlags", GPUBuffer.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(1, Mathf.CeilToInt(_itemBuffer.width / 8.0f), Mathf.CeilToInt(_itemBuffer.height / 8.0f), 1);
    }

    public Material OcclusionQueriesMaterial;

    void DoOcclusionQueries( int proteinType )
    {
        // Clear occlusion before hand
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffer.Instance.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);


        ////***** Do Depth-Stencil mask *****//

        //Fill the buffer with occludees
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_ProteinType", proteinType + 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Instance.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_ProteinRadii", GPUBuffer.Instance.ProteinRadii, 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos, 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions, 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_OcclusionQueriesBatchSpheres", GPUBuffer.Instance.SphereBatchBuffer, 1);
        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);

        // Count occludees instances
        ComputeBuffer.CopyCount(GPUBuffer.Instance.SphereBatchBuffer, _argBuffer, 0);

        // Prepare and set the render target
        var tempBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.ARGB32);
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.white);

        // Prepare draw call
        OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffer.Instance.ProteinRadii);
        OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos);
        OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions);
        OcclusionQueriesMaterial.SetBuffer("_OcclusionQueriesBatchSpheres", GPUBuffer.Instance.SphereBatchBuffer);
        OcclusionQueriesMaterial.SetPass(0);

        // Draw occludees - bounding sphere only - write to depth and stencil buffer
        Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);

        // Clear occludees buffer
        GPUBuffer.Instance.SphereBatchBuffer.ClearAppendBuffer();

        //***** Do Queries *****//

        //Fill the buffer with occluders
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_ProteinType", -(proteinType + 1));
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Instance.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_ProteinRadii", GPUBuffer.Instance.ProteinRadii, 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos, 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions, 1);
        ComputeShaderManager.Instance.SphereBatchCS.SetResource("_OcclusionQueriesBatchSpheres", GPUBuffer.Instance.SphereBatchBuffer, 1);
        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);

        // Count occluder instances
        ComputeBuffer.CopyCount(GPUBuffer.Instance.SphereBatchBuffer, _argBuffer, 0);

        // Bind the read/write occlusion buffer to the shader
        // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
        Graphics.SetRandomWriteTarget(1, GPUBuffer.Instance.ProteinInstanceOcclusionFlags);
        MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

        // Set the render target
        Graphics.SetRenderTarget(tempBuffer);

        // Prepare draw call
        OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffer.Instance.ProteinRadii);
        OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos);
        OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions);
        OcclusionQueriesMaterial.SetBuffer("_OcclusionQueriesBatchSpheres", GPUBuffer.Instance.SphereBatchBuffer);
        OcclusionQueriesMaterial.SetPass(1);

        // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
        Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
        Graphics.ClearRandomWriteTargets();

        // Clear occluders buffer
        GPUBuffer.Instance.SphereBatchBuffer.ClearAppendBuffer();
        //_argBuffer.LogDebug();

        // Release render target
        RenderTexture.ReleaseTemporary(tempBuffer);
    }
  

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (GetComponent<Camera>().pixelWidth == 0 || GetComponent<Camera>().pixelHeight == 0) return;

        if (SceneManager.Instance.NumProteinInstances == 0 && SceneManager.Instance.NumDnaSegments == 0) 
        {
            Graphics.Blit(src, dst);
            return;         
        }

        if (_itemBuffer == null || _itemBuffer.width != src.width || _itemBuffer.height != src.height)
        {
            if(_itemBuffer != null &&(_itemBuffer.width != src.width || _itemBuffer.height != src.height)) _itemBuffer.Release();

            _itemBuffer= new RenderTexture(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.RInt);
        }
        
        DoOcclusionQueries(occludeeType);

        ///**** Start rendering routine ****/

        // Declare temp buffers
        var colorBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.Depth);
        //var depthNormalsBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGBFloat);
        var compositeColorBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
        var compositeDepthBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.Depth);

        // Clear temp buffers
        Graphics.SetRenderTarget(_itemBuffer);
        GL.Clear(true, true, new Color(-1, 0, 0, 0));

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, false, Color.white);

        // Draw proteins
        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            SetProteinShaderParams();

            ProteinFillBatchBuffer(-1);
            //Debug.Log(GetBatchCount());
            Graphics.SetRenderTarget(_itemBuffer.colorBuffer, depthBuffer.depthBuffer);
            _renderProteinsMaterial.SetPass(0);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            GPUBuffer.Instance.SphereBatchBuffer.ClearAppendBuffer();
            
            ComputeHiZMap(depthBuffer);
            ComputeOcclusionCulling(frameCount);

            ProteinFillBatchBuffer(frameCount);
            //Debug.Log(GetBatchCount());
            Graphics.SetRenderTarget(_itemBuffer.colorBuffer, depthBuffer.depthBuffer);
            _renderProteinsMaterial.SetPass(0);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            GPUBuffer.Instance.SphereBatchBuffer.ClearAppendBuffer();
        }

        // Draw curve ingredients
        if (SceneManager.Instance.NumDnaSegments > 0)
        {
            SetCurveShaderParams();
            Graphics.SetRenderTarget(new[] { colorBuffer.colorBuffer, _itemBuffer.colorBuffer }, depthBuffer.depthBuffer);
            _renderCurveIngredientsMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, Mathf.Max(SceneManager.Instance.NumDnaSegments - 2, 0)); // Do not draw first and last segments
        }

        /////*** Post processing ***/

        // Get color from id buffer
        _compositeMaterial.SetTexture("_IdTexture", _itemBuffer);
        _compositeMaterial.SetBuffer("_ProteinColors", GPUBuffer.Instance.ProteinColors);
        _compositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffer.Instance.ProteinInstanceInfos);
        Graphics.Blit(null, colorBuffer, _compositeMaterial, 3);

        // Compute contours detection
        SetContourShaderParams();
        _contourMaterial.SetTexture("_IdTexture", _itemBuffer);
        Graphics.Blit(colorBuffer, compositeColorBuffer, _contourMaterial, 0);

        // Composite with scene color
        _compositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
        _compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, src, _compositeMaterial, 0);
        Graphics.Blit(src, dst);

        //Composite with scene depth
        _compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, compositeDepthBuffer, _compositeMaterial, 1);

        ////Composite with scene depth normals
        ////_compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        ////Graphics.Blit(null, depthNormalsBuffer, _compositeMaterial, 2);

        //// Set global shader properties
        Shader.SetGlobalTexture("_CameraDepthTexture", compositeDepthBuffer);
        ////Shader.SetGlobalTexture("_CameraDepthNormalsTexture", depthNormalsBuffer);

        /*** Object Picking ***/

        if (SelectionManager.Instance.MouseRightClickFlag)
        {
            SelectionManager.Instance.SetSelectedObject(MyUtility.ReadPixelId(_itemBuffer, SelectionManager.Instance.MousePosition));
            SelectionManager.Instance.MouseRightClickFlag = false;
        }

        // Release temp buffers
        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);
        //RenderTexture.ReleaseTemporary(depthNormalsBuffer);
        RenderTexture.ReleaseTemporary(compositeColorBuffer);
        RenderTexture.ReleaseTemporary(compositeDepthBuffer);

        frameCount++;
    }
}
