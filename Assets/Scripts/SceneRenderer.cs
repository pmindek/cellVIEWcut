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
    public Camera ShadowCamera;
    public RenderShadowMap RenderShadowMap;
    //public RenderTexture ShadowMap;
    //public RenderTexture ShadowMap2;

    public Material ContourMaterial;
    public Material CompositeMaterial;
    public Material OcclusionQueriesMaterial;

    public Material RenderLipidsMaterial;
    public Material RenderProteinsMaterial;
    public Material RenderCurveIngredientsMaterial;

    /*****/

    private Camera _camera;
    private RenderTexture _floodFillTexturePing;
    private RenderTexture _floodFillTexturePong;

    /*****/

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth;
        _camera.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    void OnDisable()
    {
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

    public static void SetProteinShaderParams(Camera camera)
    {
        
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
        GPUBuffers.Instance.ArgBuffer.GetData(batchCount);
        Debug.Log(batchCount[0]);
    }

    void ComputeVisibility(RenderTexture itemBuffer)
    {
        //// Clear Buffer
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 1), 1, 1);

        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.LipidInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 1), 1, 1);

        // Compute item visibility
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetTexture(1, "_ItemBuffer", itemBuffer);
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(1, "_ProteinInstanceVisibilityFlags", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(1, "_LipidInstanceVisibilityFlags", GPUBuffers.Instance.LipidInstanceVisibilityFlags);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(1, Mathf.CeilToInt(itemBuffer.width / 8.0f), Mathf.CeilToInt(itemBuffer.height / 8.0f), 1);
    }

    void FetchHistogramValues()
    {
        // Fetch histograms from GPU
        var histograms = new HistStruct[SceneManager.Get.SceneHierarchy.Count];
        GPUBuffers.Instance.Histograms.GetData(histograms);

        // Clear histograms
        ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(2, "_Histograms", GPUBuffers.Instance.Histograms);
        ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(2, Mathf.CeilToInt(SceneManager.Get.HistogramData.Count / 64.0f), 1, 1);

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

        SceneManager.Get.HistogramData = histograms.ToList();

        int a = 0;
    }

    public int WeightThreshold;

    void ComputeProteinObjectSpaceCutAways()
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Cutaways params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_WeightThreshold", WeightThreshold);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_IngredientProperties", GPUBuffers.Instance.IngredientProperties);


        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutInfos", GPUBuffers.Instance.CutInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutScales", GPUBuffers.Instance.CutScales);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutPositions", GPUBuffers.Instance.CutPositions);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutRotations", GPUBuffers.Instance.CutRotations);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_Histograms", GPUBuffers.Instance.Histograms);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetVector("_CameraForward", _camera.transform.forward);

        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetTexture(0, "noiseTexture", noiseTexture);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureW", noiseTexture.width);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureH", noiseTexture.height);

        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinCutFilters", GPUBuffer.Instance.ProteinCutFilters);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramStatistics", GPUBuffer.Instance.HistogramStatistics);
        //ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramProteinTypes", GPUBuffer.Instance.HistogramProteinTypes);
        
        // Other params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffers.Instance.ProteinInstanceVisibilityFlags);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
    }

    void ComputeLipidObjectSpaceCutAways()
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Cutaways params
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
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
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_TypeId", SceneManager.Get.ProteinIngredientNames.Count);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceVisibilityFlags", GPUBuffers.Instance.LipidInstanceVisibilityFlags);

        ComputeShaderManager.Instance.ObjectSpaceCutAwaysCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
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

    void ComputeOcclusionMaskLEqual(RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
    {
        // First clear mask buffer
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.blue);

        //***** Compute Protein Mask *****//
        if (maskProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);

            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            OcclusionQueriesMaterial.SetPass(0);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
        }

        //***** Compute Lipid Mask *****//
        if (maskLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);

            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            ComputeShaderManager.Instance.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            OcclusionQueriesMaterial.SetPass(2);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
        }
    }

    

    void ComputeOcclusionMaskGEqual(RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
    {
        // First clear mask buffer
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.blue, 0);

        //***** Compute Protein Mask *****//
        if (maskProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);

            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            OcclusionQueriesMaterial.SetPass(4);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
        }

        //***** Compute Lipid Mask *****//
        if (maskLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);

            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            ComputeShaderManager.Instance.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            OcclusionQueriesMaterial.SetPass(5);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
        }
    }

    void ComputeOcclusionQueries(RenderTexture tempBuffer, CutObject cutObject, int cutObjectIndex, int internalState, bool cullProtein, bool cullLipid)
    {
        if (cullProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occluders
            ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);

            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            ComputeShaderManager.Instance.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occluder instances
            ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Clear protein occlusion buffer 
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Bind the read/write occlusion buffer to the shader
            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
            Graphics.SetRandomWriteTarget(1, GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

            // Set the render target
            Graphics.SetRenderTarget(tempBuffer);

            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Instance.CutInfo);
            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

            OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Instance.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            OcclusionQueriesMaterial.SetPass(1);

            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
            Graphics.ClearRandomWriteTargets();

            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_CutInfo", GPUBuffers.Instance.CutInfo);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Instance.Histograms);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Instance.ProteinInstanceCullFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        }

        if (cullLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occluders
            ComputeShaderManager.Instance.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);

            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            ComputeShaderManager.Instance.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occluder instances
            ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

            // Clear lipid occlusion buffer 
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Bind the read/write occlusion buffer to the shader
            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
            Graphics.SetRandomWriteTarget(1, GPUBuffers.Instance.LipidInstanceOcclusionFlags);
            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

            // Set the render target
            Graphics.SetRenderTarget(tempBuffer);

            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Instance.CutInfo);
            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

            OcclusionQueriesMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Instance.SphereBatches);
            OcclusionQueriesMaterial.SetPass(3);

            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
            Graphics.ClearRandomWriteTargets();

            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_CutInfo", GPUBuffers.Instance.CutInfo);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Instance.Histograms);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        }
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

        var resetCutSnapshot = SceneManager.Get.ResetCutSnapshot;
        SceneManager.Get.ResetCutSnapshot = -1;

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
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Instance.Histograms);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Instance.HistogramsLookup);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_IngredientMaskParams", GPUBuffers.Instance.IngredientMaskParams);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        }

        var cutObjectId = -1;

        foreach (var cutObject in SceneManager.Get.CutObjects)
        {
            cutObjectId++;

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
            var maskFlags = new List<int>();
            var cullFlags = new List<int>();

            foreach (var cutParam in cutObject.IngredientCutParameters)
            {
                var isMask = cutParam.IsFocus;
                var isCulled = !cutParam.IsFocus && (cutParam.Aperture > 0 || cutParam.value2 < 1);

                if (cutParam.IsFocus )
                {
                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) maskProtein = true;
                    else maskLipid = true;
                }

                if (isCulled)
                { 
                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) cullProtein = true;
                    else cullLipid = true;
                }

                maskFlags.Add(isMask ? 1 : 0);
                cullFlags.Add(isCulled ? 1 : 0);
            }

            //if (!cullProtein && !cullLipid) continue;

            //********************************************************//

            //***** Compute Depth-Stencil mask *****//

            // Upload Occludees flags to GPU
            GPUBuffers.Instance.IngredientMaskParams.SetData(maskFlags.ToArray());
            ComputeOcclusionMaskLEqual(tempBuffer, maskProtein, maskLipid);
            //ComputeOcclusionMaskGEqual(tempBuffer, maskProtein, maskLipid);
            //Graphics.Blit(tempBuffer, dst);
            //break;

            ComputeDistanceTransform(tempBuffer);

            //Graphics.Blit(_floodFillTexturePong, dst, CompositeMaterial, 4);
            //break;

            /////**** Compute Queries ***//

            // Upload Occluders flags to GPU
            GPUBuffers.Instance.IngredientMaskParams.SetData(cullFlags.ToArray());
            ComputeOcclusionQueries(tempBuffer, cutObject, cutObjectId, internalState, cullProtein, cullLipid);
        }

        // Release render target
        RenderTexture.ReleaseTemporary(tempBuffer);
    }
    
    void ComputeLipidSphereBatches()
    {
        if (SceneManager.Get.NumLipidInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumLipidInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", GetComponent<Camera>().transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstancePositions", GPUBuffers.Instance.LipidInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstanceCullFlags", GPUBuffers.Instance.LipidInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidInstanceOcclusionFlags", GPUBuffers.Instance.LipidInstanceOcclusionFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(2, "_LipidSphereBatches", GPUBuffers.Instance.SphereBatches);
        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(2, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);

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
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
    }

    [Range(0, 15)]
    public float LightSize;  // 2.0

    [Range(-5, 5)]
    public float ShadowOffset;  // 0.25 

    [Range(-5, 5)]
    public float ShadowFactor; // 2

    [Range(-5,5)]
    public float ShadowBias;

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (GetComponent<Camera>().pixelWidth == 0 || GetComponent<Camera>().pixelHeight == 0) return;

        if (SceneManager.Get.NumProteinInstances == 0 && SceneManager.Get.NumLipidInstances == 0)
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
        if (SceneManager.Get.NumProteinInstances > 0)
        {
            RenderUtils.ComputeSphereBatches(_camera);

            ////RenderProteinsMaterial.SetInt("_EnableShadows", Convert.ToInt32(DisplaySettings.Instance.EnableShadows));
            //RenderProteinsMaterial.SetTexture("_ShadowMap", ShadowMap);
            //RenderProteinsMaterial.SetVector("_ShadowCameraForward", ShadowCamera.transform.forward);
            //RenderProteinsMaterial.SetVector("_ShadowCameraWorldPos", ShadowCamera.transform.position);
            //RenderProteinsMaterial.SetMatrix("_ShadowCameraViewMatrix", ShadowCamera.worldToCameraMatrix);
            //RenderProteinsMaterial.SetMatrix("_ShadowCameraProjMatrix", GL.GetGPUProjectionMatrix(ShadowCamera.projectionMatrix, false));
            //RenderProteinsMaterial.SetMatrix("_ShadowCameraViewProjMatrix", GL.GetGPUProjectionMatrix(ShadowCamera.projectionMatrix, false) * ShadowCamera.worldToCameraMatrix);

            RenderUtils.DrawProteinSphereBatches(RenderProteinsMaterial, _camera, itemBuffer.colorBuffer, depthBuffer.depthBuffer, 0);
        }

        // Draw Lipids
        if (SceneManager.Get.LipidInstanceInfos.Count > 0)
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

        ComputeVisibility(itemBuffer);
        FetchHistogramValues();

        ///////*** Post processing ***/

        // Get color from id buffer
        CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
        CompositeMaterial.SetBuffer("_IngredientStates", GPUBuffers.Instance.IngredientStates);
        CompositeMaterial.SetBuffer("_ProteinColors", GPUBuffers.Instance.ProteinColors);
        CompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        CompositeMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Instance.LipidInstanceInfo);
        Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);

        // Compute contours detection
        SetContourShaderParams();
        ContourMaterial.SetTexture("_IdTexture", itemBuffer);
        Graphics.Blit(colorBuffer, compositeColorBuffer, ContourMaterial, 0);


        ////*********** Experimental **************//

        ////// Clear Buffer
        //ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
        //ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 1), 1, 1);

        //// Compute item visibility
        //ComputeShaderManager.Instance.ComputeVisibilityCS.SetTexture(5, "_ItemBuffer2", ShadowMap2);
        //ComputeShaderManager.Instance.ComputeVisibilityCS.SetBuffer(5, "_ProteinInstanceVisibilityFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
        //ComputeShaderManager.Instance.ComputeVisibilityCS.Dispatch(5, Mathf.CeilToInt(ShadowMap2.width / 8.0f), Mathf.CeilToInt(ShadowMap2.height / 8.0f), 1);

        ////CompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        ////CompositeMaterial.SetBuffer("_ProteinAtomCount", GPUBuffers.Instance.ProteinAtomCount);

        ////CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
        ////CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
        ////CompositeMaterial.SetBuffer("_ProteinInstanceShadowFlags", GPUBuffers.Instance.ProteinInstanceOcclusionFlags);
        ////Graphics.Blit(compositeColorBuffer, colorBuffer, CompositeMaterial, 6);

        ////***************************************//

        CompositeMaterial.SetFloat("_LightSize", LightSize);
        CompositeMaterial.SetFloat("_ShadowOffset", ShadowOffset);
        CompositeMaterial.SetFloat("_ShadowFactor", ShadowFactor);
        CompositeMaterial.SetFloat("_ShadowBias", ShadowBias);

        //Collect shadows
        var matrix = GetComponent<Camera>().cameraToWorldMatrix;
        CompositeMaterial.SetMatrix("_InverseView", matrix);
        CompositeMaterial.SetTexture("_ShadowMap", RenderShadowMap.ShadowMap2);
        CompositeMaterial.SetMatrix("_ShadowCameraViewMatrix", ShadowCamera.worldToCameraMatrix);
        CompositeMaterial.SetMatrix("_ShadowCameraViewProjMatrix", GL.GetGPUProjectionMatrix(ShadowCamera.projectionMatrix, false) * ShadowCamera.worldToCameraMatrix);
        CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(compositeColorBuffer, colorBuffer, CompositeMaterial, 5);

        

        // Composite with scene color
        //CompositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
        CompositeMaterial.SetTexture("_ColorTexture", colorBuffer);
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
