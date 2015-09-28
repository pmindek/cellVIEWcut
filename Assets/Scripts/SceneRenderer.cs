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
    public Material _contourMaterial;
    public Material _compositeMaterial;
    public Material _renderProteinsMaterial;
    public Material _renderCurveIngredientsMaterial;

    /*****/
    
    private RenderTexture _HiZMap;
    private ComputeBuffer _argBuffer;
    private ComputeBuffer _proteinInstanceCullFlags;

    /*****/
    private int frameCount = 0;
    private bool _mouseClick = false;
    private float _mouseClickDownTime;
    private Vector2 _mousePos = new Vector2();

    /*****/
    
    void OnEnable()
    {
        this.hideFlags = HideFlags.None;

        if (_argBuffer == null)
        {
            _argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
            _argBuffer.SetData(new[] { 0, 1, 0, 0 });            
        }

        if (_proteinInstanceCullFlags == null) _proteinInstanceCullFlags = new ComputeBuffer(ComputeBufferManager.NumProteinInstancesMax, 4);

#if UNITY_EDITOR
        if (GetComponent<Camera>() == MyUtility.GetWindowDontShow<SceneView>().camera)
        {
            SceneView.onSceneGUIDelegate = null;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }
#endif
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

#if UNITY_EDITOR
    void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            _mouseClickDownTime = Time.realtimeSinceStartup;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 1)
        {
            _mouseClickDownTime = 0;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
        {
            var delta = Time.realtimeSinceStartup - _mouseClickDownTime;
            if (delta < 0.5f)
            {
                _mouseClick = true;
                _mousePos = Event.current.mousePosition;
            }
        }
    }
#endif    

    void OnGUI()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            _mouseClickDownTime = Time.realtimeSinceStartup;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 1)
        {
            _mouseClickDownTime = 0;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
        {
            var delta = Time.realtimeSinceStartup - _mouseClickDownTime;
            if (delta < 0.5f)
            {
                _mouseClick = true;
                _mousePos = Event.current.mousePosition;
            }
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

        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsInfos", ComputeBufferManager.Instance.CurveIngredientsInfos);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsColors", ComputeBufferManager.Instance.CurveIngredientsColors);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsToggleFlags", ComputeBufferManager.Instance.CurveIngredientsToggleFlags);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtoms", ComputeBufferManager.Instance.CurveIngredientsAtoms);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomCount", ComputeBufferManager.Instance.CurveIngredientsAtomCount);
        _renderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomStart", ComputeBufferManager.Instance.CurveIngredientsAtomStart);

        _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsInfos", ComputeBufferManager.Instance.CurveControlPointsInfos);
        _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsNormals", ComputeBufferManager.Instance.CurveControlPointsNormals);
        _renderCurveIngredientsMaterial.SetBuffer("_DnaControlPoints", ComputeBufferManager.Instance.CurveControlPointsPositions);
    }

    private void SetProteinShaderParams()
    {
        // Protein params
        _renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        _renderProteinsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        _renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", PersistantSettings.Instance.FirstLevelOffset);
        _renderProteinsMaterial.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);

        _renderProteinsMaterial.SetBuffer("_LodLevelsInfos", ComputeBufferManager.Instance.LodInfos);
        _renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", ComputeBufferManager.Instance.ProteinInstanceInfos);
        _renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", ComputeBufferManager.Instance.ProteinInstancePositions);
        _renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", ComputeBufferManager.Instance.ProteinInstanceRotations);

        _renderProteinsMaterial.SetBuffer("_ProteinColors", ComputeBufferManager.Instance.ProteinColors);
        _renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", ComputeBufferManager.Instance.ProteinAtoms);
        _renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", ComputeBufferManager.Instance.ProteinAtomClusters);
        _renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", ComputeBufferManager.Instance.SphereBatchBuffer);
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
        // Hierachical depth buffer
        if (_HiZMap == null || _HiZMap.width != GetComponent<Camera>().pixelWidth || _HiZMap.height != GetComponent<Camera>().pixelHeight)
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

    void ComputeOcclusionCulling(int cullFlag)
    {
        if (_HiZMap == null || PersistantSettings.Instance.DebugObjectCulling) return;

        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CullFlag", cullFlag);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", GetComponent<Camera>().pixelWidth);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", GetComponent<Camera>().pixelHeight);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraViewMatrix", MyUtility.Matrix4X4ToFloatArray(GetComponent<Camera>().worldToCameraMatrix));
        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraProjMatrix", MyUtility.Matrix4X4ToFloatArray(GL.GetGPUProjectionMatrix(GetComponent<Camera>().projectionMatrix, false)));

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(2, "_HiZMap", _HiZMap);

        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinRadii", ComputeBufferManager.Instance.ProteinRadii);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceInfos", ComputeBufferManager.Instance.ProteinInstanceInfos);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstanceCullFlags", _proteinInstanceCullFlags);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_ProteinInstancePositions", ComputeBufferManager.Instance.ProteinInstancePositions);
            ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(2, SceneManager.Instance.NumProteinInstances, 1, 1);
        }
    }

    void ProteinFillBatchBuffer(int cullFlagFilter)
    {
        if (SceneManager.Instance.NumProteinInstances <= 0) return;

        // Do sphere batching
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_CullFlagFilter", cullFlagFilter);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Instance.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", GetComponent<Camera>().transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", GetComponent<Camera>().transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_LodLevelsInfos", ComputeBufferManager.Instance.LodInfos);        
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(GetComponent<Camera>()));

        // Do protein batching
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinRadii", ComputeBufferManager.Instance.ProteinRadii);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinVisibilityFlag", ComputeBufferManager.Instance.ProteinToggleFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinSphereBatchInfos", ComputeBufferManager.Instance.SphereBatchBuffer);
                        
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomCount", ComputeBufferManager.Instance.ProteinAtomCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomStart", ComputeBufferManager.Instance.ProteinAtomStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterCount", ComputeBufferManager.Instance.ProteinAtomClusterCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterStart", ComputeBufferManager.Instance.ProteinAtomClusterStart);

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceInfo", ComputeBufferManager.Instance.ProteinInstanceInfos);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstancePositions", ComputeBufferManager.Instance.ProteinInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceCullFlags", _proteinInstanceCullFlags);

        // Cutaway
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumCutObjects", SceneManager.Instance.NumCutObjects);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutInfos", ComputeBufferManager.Instance.CutInfos);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutScales", ComputeBufferManager.Instance.CutScales);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutPositions", ComputeBufferManager.Instance.CutPositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_CutRotations", ComputeBufferManager.Instance.CutRotations);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinCutFilters", ComputeBufferManager.Instance.ProteinCutFilters);

        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(0, SceneManager.Instance.NumProteinInstances, 1, 1);

        // Count sphere batches
        ComputeBuffer.CopyCount(ComputeBufferManager.Instance.SphereBatchBuffer, _argBuffer, 0);
    }

    int GetBatchCount()
    {
        var batchCount = new int[1];
        _argBuffer.GetData(batchCount);
        return batchCount[0];
    }

    void ClearBatchBuffer()
    {
        // Clear append buffer
        ComputeBufferManager.ClearAppendBuffer(ComputeBufferManager.Instance.SphereBatchBuffer);
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (SceneManager.Instance.NumProteinInstances == 0 && SceneManager.Instance.NumDnaSegments == 0) 
        {
            Graphics.Blit(src, dst);
            return;         
        }        

        /**** Start rendering routine ****/

        // Declare temp buffers
        var idBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.RInt);
        var colorBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.Depth);
        var depthNormalsBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGBFloat);
        var compositeColorBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 0, RenderTextureFormat.ARGB32);        
        var compositeDepthBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.Depth);
        
        // Clear temp buffers
        Graphics.SetRenderTarget(idBuffer);
        GL.Clear(true, true, new Color(-1, 0, 0, 0));

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, false, Color.white);

        // Draw proteins
        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            SetProteinShaderParams();
            ProteinFillBatchBuffer(-1);
            Graphics.SetRenderTarget(idBuffer.colorBuffer, depthBuffer.depthBuffer);
            _renderProteinsMaterial.SetPass(0);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            ClearBatchBuffer();

            ComputeHiZMap(depthBuffer);
            ComputeOcclusionCulling(frameCount);

            ProteinFillBatchBuffer(Time.frameCount);
            //Debug.Log(GetBatchCount());
            Graphics.SetRenderTarget(idBuffer.colorBuffer, depthBuffer.depthBuffer);
            _renderProteinsMaterial.SetPass(0);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            ClearBatchBuffer();
        }

        // Draw curve ingredients
        if (SceneManager.Instance.NumDnaSegments > 0)
        {
            SetCurveShaderParams();
            Graphics.SetRenderTarget(new[] { colorBuffer.colorBuffer, idBuffer.colorBuffer }, depthBuffer.depthBuffer);
            _renderCurveIngredientsMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, Mathf.Max(SceneManager.Instance.NumDnaSegments - 2, 0)); // Do not draw first and last segments
        }

        ///*** Post processing ***/

        // Get color from id buffer
        _compositeMaterial.SetTexture("_IdTexture", idBuffer);
        _compositeMaterial.SetBuffer("_ProteinColors", ComputeBufferManager.Instance.ProteinColors);
        _compositeMaterial.SetBuffer("_ProteinInstanceInfo", ComputeBufferManager.Instance.ProteinInstanceInfos);
        Graphics.Blit(null, colorBuffer, _compositeMaterial, 3);
        
        // Compute contours detection
        SetContourShaderParams();
        _contourMaterial.SetTexture("_IdTexture", idBuffer);
        Graphics.Blit(colorBuffer, compositeColorBuffer, _contourMaterial, 0);

        // Composite with scene color
        _compositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
        _compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, src, _compositeMaterial, 0);
        Graphics.Blit(src, dst);
        
        //Composite with scene depth
        _compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, compositeDepthBuffer, _compositeMaterial, 1);

        //Composite with scene depth normals
        _compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, depthNormalsBuffer, _compositeMaterial, 2);

        // Set global shader properties
        Shader.SetGlobalTexture("_CameraDepthTexture", compositeDepthBuffer);
        Shader.SetGlobalTexture("_CameraDepthNormalsTexture", depthNormalsBuffer);

        /*** Object Picking ***/

        if (_mouseClick)
        {
            SelectionManager.Instance.SetSelectedElement(MyUtility.ReadPixelId(idBuffer, _mousePos));
            _mouseClick = false;
        }

        // Release temp buffers
        RenderTexture.ReleaseTemporary(idBuffer);
        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);
        RenderTexture.ReleaseTemporary(depthNormalsBuffer);
        RenderTexture.ReleaseTemporary(compositeColorBuffer);
        RenderTexture.ReleaseTemporary(compositeDepthBuffer);

        frameCount++;
    }
}
