using System;
using UnityEngine;
using Renderer = UnityEngine.Renderer;

[ExecuteInEditMode]
public class SceneRenderer : MonoBehaviour
{
    public Shader ContourShader;
    public Shader CompositeShader;
    public Shader RenderDnaShader;
    public Shader RenderProteinsShader;
    
    /*****/

    private Material _contourMaterial;
    private Material _compositeMaterial;
    private Material _renderCurveIngredientsMaterial;
    private Material _renderProteinsMaterial;

    /*****/

    private Camera _camera;
    private RenderTexture _HiZMap;
    private ComputeBuffer _argBuffer;

    /*****/
    private int frameCount = 0;
    private bool _rightMouseDown = false;
    private Vector2 _mousePos = new Vector2();

    /*****/

    void OnEnable()
    {
        this.hideFlags = HideFlags.None;

        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth;
        _camera.depthTextureMode |= DepthTextureMode.DepthNormals;

        if (_renderProteinsMaterial == null) _renderProteinsMaterial = new Material(RenderProteinsShader) { hideFlags = HideFlags.HideAndDontSave };
        if (_renderCurveIngredientsMaterial == null) _renderCurveIngredientsMaterial = new Material(RenderDnaShader) { hideFlags = HideFlags.HideAndDontSave };
        if (_compositeMaterial == null) _compositeMaterial = new Material(CompositeShader) { hideFlags = HideFlags.HideAndDontSave };
        if (_contourMaterial == null) _contourMaterial = new Material(ContourShader) { hideFlags = HideFlags.HideAndDontSave };

        if (_argBuffer == null)
        {
            _argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
            _argBuffer.SetData( new [] { 0, 1, 0, 0 });
        }
    }

    void OnDestroy()
    {
        int a = 0;
    }

    void OnDisable()
    {
        if (_renderProteinsMaterial != null) DestroyImmediate(_renderProteinsMaterial);
        if (_renderCurveIngredientsMaterial != null) DestroyImmediate(_renderCurveIngredientsMaterial);
        if (_compositeMaterial != null) DestroyImmediate(_compositeMaterial);
        if (_contourMaterial != null) DestroyImmediate(_contourMaterial); 
        
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
    }

    void OnGUI()
    {
        // Listen mouse click events
        //if (Event.current.type == EventType.MouseDown && Event.current.modifiers == EventModifiers.Control && Event.current.button == 0)
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            _rightMouseDown = true;
            _mousePos = Event.current.mousePosition;
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
        var planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_0", Helper.PlaneToVector4(planes[0]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_1", Helper.PlaneToVector4(planes[1]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_2", Helper.PlaneToVector4(planes[2]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_3", Helper.PlaneToVector4(planes[3]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_4", Helper.PlaneToVector4(planes[4]));
        _renderCurveIngredientsMaterial.SetVector("_FrustrumPlane_5", Helper.PlaneToVector4(planes[5]));


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

    void SetProteinShaderParams()
    {
        // Protein params
        _renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        _renderProteinsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        _renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", PersistantSettings.Instance.FirstLevelOffset);
        _renderProteinsMaterial.SetVector("_CameraForward", _camera.transform.forward);

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
        if (_HiZMap == null || _HiZMap.width != Screen.width || _HiZMap.height != Screen.height)
        {
            if (_HiZMap != null)
            {
                _HiZMap.Release();
                DestroyImmediate(_HiZMap);
                _HiZMap = null;
            }

            _HiZMap = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
            _HiZMap.enableRandomWrite = true;
            _HiZMap.useMipMap = false;
            _HiZMap.isVolume = true;
            _HiZMap.volumeDepth = 24;
            //_HiZMap.filterMode = FilterMode.Point;
            _HiZMap.wrapMode = TextureWrapMode.Clamp;
            _HiZMap.hideFlags = HideFlags.HideAndDontSave;
            _HiZMap.Create();
        }

        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", Screen.width);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", Screen.height);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(0, "_RWHiZMap", _HiZMap);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(0, "_DepthBuffer", depthBuffer);
        ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(0, (int)Mathf.Ceil(Screen.width / 8.0f), (int)Mathf.Ceil(Screen.height / 8.0f), 1);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(1, "_RWHiZMap", _HiZMap);
        for (int i = 1; i < 12; i++)
        {
            ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CurrentLevel", i);
            ComputeShaderManager.Instance.OcclusionCullingCS.Dispatch(1, (int)Mathf.Ceil(Screen.width / 8.0f), (int)Mathf.Ceil(Screen.height / 8.0f), 1);
        }
    }
    
    void ComputeOcclusionCulling(int cullFlag)
    {
        if (_HiZMap == null || PersistantSettings.Instance.DebugObjectCulling) return;

        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_CullFlag", cullFlag);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenWidth", Screen.width);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_ScreenHeight", Screen.height);
        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);

        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_FrustrumPlanes", Helper.FrustrumPlanesAsFloats(_camera));
        ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_EnableCrossSection", Convert.ToInt32(PersistantSettings.Instance.EnableCrossSection));
        ComputeShaderManager.Instance.OcclusionCullingCS.SetVector("_CrossSectionPlane", new Vector4(PersistantSettings.Instance.CrossSectionPlaneNormal.x, PersistantSettings.Instance.CrossSectionPlaneNormal.y, PersistantSettings.Instance.CrossSectionPlaneNormal.z, PersistantSettings.Instance.CrossSectionPlaneDistance));

        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraViewMatrix", Helper.Matrix4X4ToFloatArray(_camera.worldToCameraMatrix));
        ComputeShaderManager.Instance.OcclusionCullingCS.SetFloats("_CameraProjMatrix", Helper.Matrix4X4ToFloatArray(GL.GetGPUProjectionMatrix(_camera.projectionMatrix, false)));

        ComputeShaderManager.Instance.OcclusionCullingCS.SetTexture(2, "_HiZMap", _HiZMap);

        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            ComputeShaderManager.Instance.OcclusionCullingCS.SetInt("_NumInstances", SceneManager.Instance.NumProteinInstances);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_InstanceCullFlags", ComputeBufferManager.Instance.ProteinInstanceCullFlags);
            ComputeShaderManager.Instance.OcclusionCullingCS.SetBuffer(2, "_InstancePositions", ComputeBufferManager.Instance.ProteinInstancePositions);
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
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", _camera.transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", _camera.transform.position);

        // Frustrum culling + cross section
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", Helper.FrustrumPlanesAsFloats(_camera));
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableCrossSection", Convert.ToInt32(PersistantSettings.Instance.EnableCrossSection));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CrossSectionPlane", new Vector4(PersistantSettings.Instance.CrossSectionPlaneNormal.x, PersistantSettings.Instance.CrossSectionPlaneNormal.y, PersistantSettings.Instance.CrossSectionPlaneNormal.z, PersistantSettings.Instance.CrossSectionPlaneDistance));
        
        // Do protein batching
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_LodLevelsInfos", ComputeBufferManager.Instance.LodInfos);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceInfo", ComputeBufferManager.Instance.ProteinInstanceInfos);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstancePositions", ComputeBufferManager.Instance.ProteinInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinSphereBatchInfos", ComputeBufferManager.Instance.SphereBatchBuffer);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinVisibilityFlag", ComputeBufferManager.Instance.ProteinToggleFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceCullFlags", ComputeBufferManager.Instance.ProteinInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomCount", ComputeBufferManager.Instance.ProteinAtomCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomStart", ComputeBufferManager.Instance.ProteinAtomStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterCount", ComputeBufferManager.Instance.ProteinAtomClusterCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterStart", ComputeBufferManager.Instance.ProteinAtomClusterStart);

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
        // Return if no instances to draw
        if (SceneManager.Instance.NumProteinInstances == 0 && SceneManager.Instance.NumDnaSegments == 0)
        {
            Graphics.Blit(src, dst); return;
        }

        /**** Start rendering routine ****/

        // Declare temp buffers
        var idBuffer = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.RInt);
        var colorBuffer = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(src.width, src.height, 32, RenderTextureFormat.Depth);
        var depthNormalsBuffer = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        var colorCompositeBuffer = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        var depthCompositeBuffer = RenderTexture.GetTemporary(src.width, src.height, 32, RenderTextureFormat.Depth);

        // Clear temp buffers
        Graphics.SetRenderTarget(idBuffer);
        GL.Clear(true, true, new Color(-1, 0, 0, 0));

        Graphics.SetRenderTarget(depthNormalsBuffer);
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0, 0));
        
        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, true, Color.white);
        
        /**** Draw proteins ****/

        SetProteinShaderParams();

        if (SceneManager.Instance.NumProteinInstances > 0)
        {
            ProteinFillBatchBuffer(-1);
            //Debug.Log(GetBatchCount());
            Graphics.SetRenderTarget(idBuffer.colorBuffer, depthBuffer.depthBuffer);
            _renderProteinsMaterial.SetPass(0);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer);
            ClearBatchBuffer();
        }

        ComputeHiZMap(depthBuffer);
        ComputeOcclusionCulling(frameCount);

        if (SceneManager.Instance.NumProteinInstances > 0)
        {
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
        
        /*** Post processing ***/

        // Get color from id buffer
        _compositeMaterial.SetTexture("_IdTexture", idBuffer);
        _compositeMaterial.SetBuffer("_ProteinColors", ComputeBufferManager.Instance.ProteinColors);
        _compositeMaterial.SetBuffer("_ProteinInstanceInfo", ComputeBufferManager.Instance.ProteinInstanceInfos);
        Graphics.Blit(null, colorBuffer, _compositeMaterial, 3);
        
        SetContourShaderParams();

        // Compute contours detection
        _contourMaterial.SetTexture("_IdTexture", idBuffer);
        Graphics.Blit(colorBuffer, colorCompositeBuffer, _contourMaterial, 0);
        Graphics.Blit(colorCompositeBuffer, colorBuffer);

        // Compute final compositing with the rest of the scene
        _compositeMaterial.SetTexture("_ColorTexture", colorBuffer);
        _compositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.SetRenderTarget(colorCompositeBuffer.colorBuffer, depthCompositeBuffer.depthBuffer);
        GL.Clear(true, true, new Color(1, 1, 1, 1));
        Graphics.Blit(src, _compositeMaterial, 1);

        // Blit final color buffer to dst buffer
        Graphics.Blit(colorCompositeBuffer, dst);

        // Set final depth buffer to global depth
        Shader.SetGlobalTexture("_CameraDepthTexture", depthCompositeBuffer);
        Shader.SetGlobalTexture("_CameraDepthNormalsTexture ", depthNormalsBuffer); // It is important to set this otherwise AO will show ghosts

        /*** Object Picking ***/

        if (_rightMouseDown)
        {
            SceneManager.Instance.SetSelectedElement(Helper.ReadPixelId(idBuffer, _mousePos));
            _rightMouseDown = false;
        }

        // Release temp buffers
        RenderTexture.ReleaseTemporary(idBuffer);
        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);
        RenderTexture.ReleaseTemporary(depthNormalsBuffer);
        RenderTexture.ReleaseTemporary(colorCompositeBuffer);
        RenderTexture.ReleaseTemporary(depthCompositeBuffer);

        frameCount ++;
    }
}