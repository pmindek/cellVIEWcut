using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SceneRenderer : MonoBehaviour
{
    public Material ContourMaterial;
    public Material CompositeMaterial;
    public Material OcclusionQueriesMaterial;

    public Material RenderLipidsMaterial;
    public Material RenderProteinsMaterial;
    public Material RenderCurveIngredientsMaterial;

    /*****/
    
    private ComputeBuffer _argBuffer;
    private RenderTexture _floodFillTexturePing;
    private RenderTexture _floodFillTexturePong;

    /*****/

    void OnEnable()
    {
        this.hideFlags = HideFlags.None;

        if (_argBuffer == null)
        {
            _argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
            _argBuffer.SetData(new[] { 0, 1, 0, 0 });            
        }
    }

    void OnDisable()
    {
        if (_argBuffer != null)
        {
            _argBuffer.Release();
            _argBuffer = null;
        }

        if (_floodFillTexturePing != null)
        {
            _floodFillTexturePing.DiscardContents();
            DestroyImmediate(_floodFillTexturePing);
        }

        if (_floodFillTexturePong != null)
        {
            _floodFillTexturePong.DiscardContents();
            DestroyImmediate(_floodFillTexturePong);
        }
    }

    void SetContourShaderParams()
    {
        // Contour params
        ContourMaterial.SetInt("_ContourOptions", PersistantSettings.Instance.ContourOptions);
        ContourMaterial.SetFloat("_ContourStrength", PersistantSettings.Instance.ContourStrength);
    }

    private void SetProteinShaderParams()
    {
        // Protein params
        RenderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        RenderProteinsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        RenderProteinsMaterial.SetFloat("_FirstLevelBeingRange", PersistantSettings.Instance.FirstLevelOffset);
        RenderProteinsMaterial.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);

        RenderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Instance.LodInfo);
        RenderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        RenderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
        RenderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Instance.ProteinInstanceRotations);

        RenderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Instance.ProteinColors);
        RenderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Instance.ProteinAtoms);
        RenderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Instance.ProteinAtomClusters);
        RenderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Instance.SphereBatches);
    }

    //void SetCurveShaderParams()
    //{
    //    var planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_0", MyUtility.PlaneToVector4(planes[0]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_1", MyUtility.PlaneToVector4(planes[1]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_2", MyUtility.PlaneToVector4(planes[2]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_3", MyUtility.PlaneToVector4(planes[3]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_4", MyUtility.PlaneToVector4(planes[4]));
    //    _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_5", MyUtility.PlaneToVector4(planes[5]));


    //    _renderCurveIngredientsMaterial.SetInt("_NumSegments", SceneManager.Instance.NumDnaControlPoints);
    //    _renderCurveIngredientsMaterial.SetInt("_EnableTwist", 1);

    //    _renderCurveIngredientsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
    //    _renderCurveIngredientsMaterial.SetFloat("_SegmentLength", PersistantSettings.Instance.DistanceContraint);
    //    _renderCurveIngredientsMaterial.SetInt("_EnableCrossSection", Convert.ToInt32(PersistantSettings.Instance.EnableCrossSection));
    //    _renderCurveIngredientsMaterial.SetVector("_CrossSectionPlane", new Vector4(PersistantSettings.Instance.CrossSectionPlaneNormal.x, PersistantSettings.Instance.CrossSectionPlaneNormal.y, PersistantSettings.Instance.CrossSectionPlaneNormal.z, PersistantSettings.Instance.CrossSectionPlaneDistance));

    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsInfos", GPUBuffer.Instance.CurveIngredientsInfos);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsColors", GPUBuffer.Instance.CurveIngredientsColors);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsToggleFlags", GPUBuffer.Instance.CurveIngredientsToggleFlags);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtoms", GPUBuffer.Instance.CurveIngredientsAtoms);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomCount", GPUBuffer.Instance.CurveIngredientsAtomCount);
    //    _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomStart", GPUBuffer.Instance.CurveIngredientsAtomStart);

    //    _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsInfos", GPUBuffer.Instance.CurveControlPointsInfos);
    //    _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsNormals", GPUBuffer.Instance.CurveControlPointsNormals);
    //    _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPoints", GPUBuffer.Instance.CurveControlPointsPositions);
    //}

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

    //void ComputeHiZMap(RenderTexture depthBuffer)
    //{
    //    if (GetComponent<Camera>().pixelWidth == 0 || GetComponent<Camera>().pixelHeight == 0) return;

    //    // Hierachical depth buffer
    //    if (_HiZMap == null || _HiZMap.width != GetComponent<Camera>().pixelWidth || _HiZMap.height != GetComponent<Camera>().pixelHeight )
    //    {
            
    //        if (_HiZMap != null)
    //        {
    //            _HiZMap.Release();
    //            DestroyImmediate(_HiZMap);
    //            _HiZMap = null;
    //        }

    //        _HiZMap = new RenderTexture(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.RFloat);
    //        _HiZMap.enableRandomWrite = true;
    //        _HiZMap.useMipMap = false;
    //        _HiZMap.isVolume = true;
    //        _HiZMap.volumeDepth = 24;
    //        //_HiZMap.filterMode = FilterMode.Point;
    //        _HiZMap.wrapMode = TextureWrapMode.Clamp;
    //        _HiZMap.hideFlags = HideFlags.HideAndDontSave;
    //        _HiZMap.Create();
    //    }

    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<Camera>().pixelWidth);
    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<Camera>().pixelHeight);

    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(0, "_RWHiZMap", _HiZMap);
    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(0, "_DepthBuffer", depthBuffer);
    //    ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(0, (int)Mathf.Ceil(GetComponent<Camera>().pixelWidth / 8.0f), (int)Mathf.Ceil(GetComponent<Camera>().pixelHeight / 8.0f), 1);

    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(1, "_RWHiZMap", _HiZMap);
    //    for (int i = 1; i < 12; i++)
    //    {
    //        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CurrentLevel", i);
    //        ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(1, (int)Mathf.Ceil(GetComponent<Camera>().pixelWidth / 8.0f), (int)Mathf.Ceil(GetComponent<Camera>().pixelHeight / 8.0f), 1);
    //    }
    //}

    //void ComputeOcclusionCulling(int cullingFilter)
    //{
    //    if (_HiZMap == null || PersistantSettings.Instance.DebugObjectCulling) return;

    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CullingFilter", cullingFilter);
    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<Camera>().pixelWidth);
    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<Camera>().pixelHeight);
    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);

    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraViewMatrix", MyUtility.Matrix4X4ToFloatArray(GetComponent<Camera>().worldToCameraMatrix));
    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraProjMatrix", MyUtility.Matrix4X4ToFloatArray(GL.GetGPUProjectionMatrix(GetComponent<Camera>().projectionMatrix, false)));

    //    ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(2, "_HiZMap", _HiZMap);

    //    if (SceneManager.Instance.NumProteinInstances > 0)
    //    {
    //        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
    //        ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinRadii", GPUBuffer.Instance.ProteinRadii);
    //        ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceInfos", GPUBuffer.Instance.ProteinInstanceInfos);
    //        ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceCullFlags", _proteinInstanceCullFlags);
    //        ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstancePositions", GPUBuffer.Instance.ProteinInstancePositions);
    //        ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(2, SceneManager.Instance.NumProteinInstances, 1, 1);
    //    }
    //}

    void DebugSphereBatchCount()
    {
        var batchCount = new int[1];
        _argBuffer.GetData(batchCount);
        Debug.Log(batchCount[0]);
    }

    void ComputeVisibility(RenderTexture itemBuffer)
    {
        //// Clear Buffer
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 1), 1, 1);

        // Compute item visibility
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetTexture(1, "_ItemBuffer", itemBuffer);
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(1, "_FlagBuffer", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(1, Mathf.CeilToInt(itemBuffer.width / 8.0f), Mathf.CeilToInt(itemBuffer.height / 8.0f), 1);
    }

    void FetchHistogramValues()
    {
        // Fetch histograms from GPU
        var histograms = new HistStruct[SceneManager.Instance.SceneHierarchy.Count];
        GPUBuffers.Instance.Histograms.GetData(histograms);

        // Clear histograms
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(2, "_Histograms", GPUBuffers.Instance.Histograms);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(2, Mathf.CeilToInt(SceneManager.Instance.HistogramData.Count / 64.0f), 1, 1);

        foreach (var histogram in histograms)
        {
            int addWhere = histogram.parent;
            while (addWhere >= 0)
            {
                histograms[addWhere].all += histogram.all;
                histograms[addWhere].cutaway += histogram.cutaway;
                histograms[addWhere].visible += histogram.visible;
                addWhere = histograms[addWhere].parent;
            }
        }

        SceneManager.Instance.HistogramData = histograms.ToList();

        int a = 0;
    }

    void ComputeProteinObjectSpaceCutAways()
    {
        if (SceneManager.Instance.NumProteinInstances <= 0) return;

        // Cutaways params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumCutObjects", SceneManager.Instance.NumCutObjects);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumIngredientTypes", SceneManager.Instance.NumAllIngredients);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutInfos", GPUBuffers.Instance.CutInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutScales", GPUBuffers.Instance.CutScales);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutPositions", GPUBuffers.Instance.CutPositions);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutRotations", GPUBuffers.Instance.CutRotations);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_Histograms", GPUBuffers.Instance.Histograms);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetVector("_CameraForward", Camera.main.transform.forward);

        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetTexture(0, "noiseTexture", noiseTexture);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureW", noiseTexture.width);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureH", noiseTexture.height);

        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinCutFilters", GPUBuffer.Instance.ProteinCutFilters);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramStatistics", GPUBuffer.Instance.HistogramStatistics);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramProteinTypes", GPUBuffer.Instance.HistogramProteinTypes);
        
        // Other params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_NumInstances", SceneManager.Instance.NumProteinInstances);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);
    }

    void ComputeLipidObjectSpaceCutAways()
    {
        if (SceneManager.Instance.NumProteinInstances <= 0) return;

        // Cutaways params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumCutObjects", SceneManager.Instance.NumCutObjects);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumIngredientTypes", SceneManager.Instance.NumAllIngredients);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutInfos", GPUBuffers.Instance.CutInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutScales", GPUBuffers.Instance.CutScales);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutPositions", GPUBuffers.Instance.CutPositions);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutRotations", GPUBuffers.Instance.CutRotations);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_Histograms", GPUBuffers.Instance.Histograms);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);

        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetTexture(0, "noiseTexture", noiseTexture);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureW", noiseTexture.width);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureH", noiseTexture.height);

        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinCutFilters", GPUBuffer.Instance.ProteinCutFilters);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramStatistics", GPUBuffer.Instance.HistogramStatistics);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramProteinTypes", GPUBuffer.Instance.HistogramProteinTypes);

        // Other params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_TypeId", SceneManager.Instance.ProteinIngredientNames.Count);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_NumInstances", SceneManager.Instance.NumLipidInstances);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);
    }

    void ComputeDistanceTransform(RenderTexture inputTexture)
    {
        var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);
        Graphics.SetRenderTarget(tempBuffer);
        Graphics.Blit(inputTexture, tempBuffer);

        // Prepare and set the render target
        if (_floodFillTexturePing == null)
        {
            _floodFillTexturePing = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
            _floodFillTexturePing.enableRandomWrite = true;
            _floodFillTexturePing.filterMode = FilterMode.Point;
        }

        Graphics.SetRenderTarget(_floodFillTexturePing);
        GL.Clear(true, true, new Color(-1, -1, -1, -1));

        if (_floodFillTexturePong == null)
        {
            _floodFillTexturePong = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
            _floodFillTexturePong.enableRandomWrite = true;
            _floodFillTexturePong.filterMode = FilterMode.Point;
        }

        Graphics.SetRenderTarget(_floodFillTexturePong);
        GL.Clear(true, true, new Color(-1, -1, -1, -1));

        float widthScale = inputTexture.width/512.0f;
        float heightScale = inputTexture.height/512.0f;

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 2);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);

        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 4);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 8);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 16);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 32);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 64);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 128);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 256);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Instance.FloodFillCS.SetInt("_StepSize", 512 / 512);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Instance.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Instance.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Instance.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        RenderTexture.ReleaseTemporary(tempBuffer);
    }

    //[ImageEffectOpaque]
    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    void ComputeViewSpaceCutAways()
    {
        //ComputeProteinObjectSpaceCutAways();
        //ComputeLipidObjectSpaceCutAways();

        // Prepare and set the render target
        var tempBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.ARGB32);
        //var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);

        var resetCutSnapshot = SceneManager.Instance.ResetCutSnapshot;
        SceneManager.Instance.ResetCutSnapshot = -1;

        if (resetCutSnapshot > 0)
        {
            // Discard occluding instances according to value2
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Instance.Histograms);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Instance.Histograms);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);
        }

        foreach (var cutObject in SceneManager.Instance.CutObjects)
        {
            //********************************************************//

            var internalState = 0;
            //Debug.Log(cutObject.CurrentLockState.ToString());

            if (cutObject.CurrentLockState == LockState.Restore)
            {
                Debug.Log("We restore");
                internalState = 2;
                cutObject.CurrentLockState = LockState.Unlocked;
            }

            if (cutObject.CurrentLockState == LockState.Consumed)
            {
                continue;
            }

            if (cutObject.CurrentLockState == LockState.Locked)
            {
                Debug.Log("We consume");
                internalState = 1;
                cutObject.CurrentLockState = LockState.Consumed;
            }

            //********************************************************//

            var maskLipid = false;
            var cullLipid = false;

            var maskProtein = false;
            var cullProtein = false;

            //Fill the buffer with occludees mask falgs
            var maskFlags1 = new List<int>();
            var maskFlags2 = new List<int>();
            var value2List = new List<float>();

            foreach (var cutParam in cutObject.IngredientCutParameters)
            {
                if (cutParam.IsFocus )
                {
                    if (cutParam.Id < SceneManager.Instance.NumProteinIngredients) maskProtein = true;
                    else maskLipid = true;
                }

                if (!cutParam.IsFocus)
                { 
                    if (cutParam.Id < SceneManager.Instance.NumProteinIngredients) cullProtein = true;
                    else cullLipid = true;
                }

                maskFlags1.Add(cutParam.IsFocus ? 1 : 0);
                maskFlags2.Add(!cutParam.IsFocus? 1 : 0);
                value2List.Add(cutParam.value2);
            }

            //if (!cullProtein && !cullLipid) continue;

            //********************************************************//

            // Upload Mask Params to GPU
            GPUBuffers.Instance.IngredientMaskParams.SetData(maskFlags1.ToArray());

            //***** Compute Depth-Stencil mask *****//

            // First clear mask buffer
            Graphics.SetRenderTarget(tempBuffer);
            GL.Clear(true, true, Color.blue);
            
            //***** Compute Protein Mask *****//
            if (maskProtein)
            {
                // Always clear append buffer before usage
                GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

                //Fill the buffer with occludees
                ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Instance.NumProteinInstances);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);

                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);

                // Count occludees instances
                ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, _argBuffer, 0);

                // Prepare draw call
                OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
                OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
                OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
                OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
                OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                OcclusionQueriesMaterial.SetPass(0);

                // Draw occludees - bounding sphere only - write to depth and stencil buffer
                Graphics.SetRenderTarget(tempBuffer);
                Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            }
            
            //***** Compute Lipid Mask *****//
            if (maskLipid)
            {
                // Always clear append buffer before usage
                GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

                //Fill the buffer with occludees
                ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Instance.NumLipidInstances);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);

                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                ComputeShaderManager.Instance.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);

                // Count occludees instances
                ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, _argBuffer, 0);

                //DebugSphereBatchCount();

                // Prepare draw call
                OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
                OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
                OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
                OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                OcclusionQueriesMaterial.SetPass(2);

                // Draw occludees - bounding sphere only - write to depth and stencil buffer
                Graphics.SetRenderTarget(tempBuffer);
                Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            }

            ComputeDistanceTransform(tempBuffer);

            //Graphics.Blit(_floodFillTexturePong, dst);
            //Graphics.Blit(_floodFillTexturePong, dst, CompositeMaterial,4);
            //Graphics.Blit(tempBuffer, dst);
            //break;

            /////**** Compute Queries ***//
            
            if (cullProtein)
            {
                GPUBuffers.Instance.IngredientMaskParams.SetData(maskFlags2.ToArray());

                // Always clear append buffer before usage
                GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

                //Fill the buffer with occluders
                ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Instance.NumProteinInstances);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);

                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);

                // Count occluder instances
                ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, _argBuffer, 0);

                DebugSphereBatchCount();

                // Clear protein occlusion buffer 
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
                ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);
                
                // Bind the read/write occlusion buffer to the shader
                // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
                Graphics.SetRandomWriteTarget(1, GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
                MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

                // Set the render target
                Graphics.SetRenderTarget(tempBuffer);

                OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Instance.CutInfo);
                OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

                OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
                OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
                OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
                OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
                OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                OcclusionQueriesMaterial.SetPass(1);

                // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
                Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
                Graphics.ClearRandomWriteTargets();

                // Fill buffer with occlusion values
                GPUBuffers.Instance.IngredientMaskParams.SetData(value2List.ToArray());

                //// Discard occluding instances according to value2
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Instance.Histograms);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
                ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);
            }

            if (cullLipid)
            {
                GPUBuffers.Instance.IngredientMaskParams.SetData(maskFlags2.ToArray());

                // Always clear append buffer before usage
                GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

                //Fill the buffer with occluders
                ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Instance.NumLipidInstances);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);

                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
                ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                ComputeShaderManager.Instance.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);

                // Count occluder instances
                ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, _argBuffer, 0);

                

                // Clear lipid occlusion buffer 
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
                ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);

                // Bind the read/write occlusion buffer to the shader
                // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
                Graphics.SetRandomWriteTarget(1, GPUBuffers.Instance.LipidInstanceOcclusionFlags);
                MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

                // Set the render target
                Graphics.SetRenderTarget(tempBuffer);

                OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
                OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
                OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
                OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
                OcclusionQueriesMaterial.SetPass(3);

                // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
                Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
                Graphics.ClearRandomWriteTargets();

                // Fill buffer with occlusion values
                GPUBuffers.Instance.IngredientMaskParams.SetData(value2List.ToArray());

                //// Discard occluding instances according to value2
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Instance.Histograms);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
                ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
                ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);
            }
        }

        // Release render target
        RenderTexture.ReleaseTemporary(tempBuffer);
    }

    void ComputeSphereBatches()
    {
        if (SceneManager.Instance.NumProteinInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Instance.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", GetComponent<Camera>().transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomCount", GPUBuffers.Instance.ProteinAtomCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomStart", GPUBuffers.Instance.ProteinAtomStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterCount", GPUBuffers.Instance.ProteinAtomClusterCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterStart", GPUBuffers.Instance.ProteinAtomClusterStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_LodLevelsInfos", GPUBuffers.Instance.LodInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinSphereBatchInfos", GPUBuffers.Instance.SphereBatches);

        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Instance.NumProteinInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, _argBuffer, 0);
    }

    void ComputeLipidSphereBatches()
    {
        if (SceneManager.Instance.NumLipidInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Instance.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Instance.NumLipidInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", GetComponent<Camera>().transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstanceOcclusionFlags", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidSphereBatches", GPUBuffers.Instance.SphereBatches);
        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(2, Mathf.CeilToInt(SceneManager.Instance.NumLipidInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, _argBuffer, 0);

    }

    void DrawProteinSphereBatches(RenderTexture colorBuffer, RenderTexture depthBuffer)
    {
        SetProteinShaderParams();
        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        RenderProteinsMaterial.SetPass(0);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
    }

    void DrawLipidSphereBatches(RenderTexture colorBuffer, RenderTexture depthBuffer)
    {
        RenderLipidsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        RenderLipidsMaterial.SetBuffer("_LipidSphereBatches", GPUBuffers.Instance.SphereBatches);
        RenderLipidsMaterial.SetBuffer("_LipidAtomPositions", GPUBuffers.Instance.LipidAtomPositions);
        //RenderLipidsMaterial.SetBuffer("_LipidInstanceInfos", GPUBuffer.Instance.LipidInstanceInfos);
        RenderLipidsMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
        RenderLipidsMaterial.SetPass(0);

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (GetComponent<Camera>().pixelWidth == 0 || GetComponent<Camera>().pixelHeight == 0) return;

        if (SceneManager.Instance.NumProteinInstances == 0 && SceneManager.Instance.NumLipidInstances == 0)
        {
            Graphics.Blit(src, dst);
            return;
        }

        ComputeProteinObjectSpaceCutAways();
        ComputeLipidObjectSpaceCutAways();
        ComputeViewSpaceCutAways();

        ///**** Start rendering routine ***

        // Declare temp buffers
        var colorBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.Depth);
        var itemBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.RInt);
        //var depthNormalsBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGBFloat);
        var compositeColorBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
        var compositeDepthBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.Depth);

        // Clear temp buffers
        Graphics.SetRenderTarget(itemBuffer);
        GL.Clear(true, true, new Color(-1, 0, 0, 0));

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, true, Color.black);

        // Draw proteins
        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            ComputeSphereBatches();
            DrawProteinSphereBatches(itemBuffer, depthBuffer);
            ComputeVisibility(itemBuffer);
        }

        // Draw Lipids
        if (SceneManager.Instance.LipidInstanceInfos.Count > 0)
        {
            ComputeLipidSphereBatches();
            DrawLipidSphereBatches(itemBuffer, depthBuffer);
        }

        //// Draw curve ingredients
        //if (SceneManager.Instance.NumDnaSegments > 0)
        //{
        //    SetCurveShaderParams();
        //    Graphics.SetRenderTarget(new[] { colorBuffer.colorBuffer, _itemBuffer.colorBuffer }, depthBuffer.depthBuffer);
        //    _renderCurveIngredientsMaterial.SetPass(0);
        //    Graphics.DrawProcedural(MeshTopology.Points, Mathf.Max(SceneManager.Instance.NumDnaSegments - 2, 0)); // Do not draw first and last segments
        //}

        //ComputeVisibility(itemBuffer);
        FetchHistogramValues();

        ///////*** Post processing ***/

        // Get color from id buffer
        CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
        CompositeMaterial.SetBuffer("_ProteinColors", GPUBuffers.Instance.ProteinColors);
        CompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        CompositeMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
        Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);

        // Compute contours detection
        SetContourShaderParams();
        ContourMaterial.SetTexture("_IdTexture", itemBuffer);
        Graphics.Blit(colorBuffer, compositeColorBuffer, ContourMaterial, 0);

        Graphics.Blit(compositeColorBuffer, dst);

        // Composite with scene color
        CompositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
        CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, src, CompositeMaterial, 0);
        Graphics.Blit(src, dst);

        //Composite with scene depth
        CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, compositeDepthBuffer, CompositeMaterial, 1);

        ////Composite with scene depth normals
        ////_compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        ////Graphics.Blit(null, depthNormalsBuffer, _compositeMaterial, 2);

        //// Set global shader properties
        Shader.SetGlobalTexture("_CameraDepthTexture", compositeDepthBuffer);
        ////Shader.SetGlobalTexture("_CameraDepthNormalsTexture", depthNormalsBuffer);

        /*** Object Picking ***/

        if (SelectionManager.Instance.MouseRightClickFlag)
        {
            SelectionManager.Instance.SetSelectedObject(MyUtility.ReadPixelId(itemBuffer, SelectionManager.Instance.MousePosition));
            SelectionManager.Instance.MouseRightClickFlag = false;
        }

        // Release temp buffers
        RenderTexture.ReleaseTemporary(itemBuffer);
        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);
        //RenderTexture.ReleaseTemporary(depthNormalsBuffer);
        RenderTexture.ReleaseTemporary(compositeColorBuffer);
        RenderTexture.ReleaseTemporary(compositeDepthBuffer);
    }
}
