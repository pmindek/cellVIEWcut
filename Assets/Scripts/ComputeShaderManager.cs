using System;
using UnityEngine;

[ExecuteInEditMode]
public class ComputeShaderManager : MonoBehaviour
{
    public ComputeShader KMeansCS;
    public ComputeShader ReadPixelCS;
    public ComputeShader FloodFillCS;
    public ComputeShader SphereBatchCS;
    public ComputeShader OcclusionCullingCS;
    public ComputeShader ObjectSpaceCutAwaysCS;
    public ComputeShader ComputeVisibilityCS;

    // Declare the shader manager as a singleton
    private static ComputeShaderManager _instance = null;
    public static ComputeShaderManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ComputeShaderManager>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_ComputeShaderManager");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_ComputeShaderManager"); // { hideFlags = HideFlags.HideInInspector };
                    _instance = go.AddComponent<ComputeShaderManager>();
                }
            }

            return _instance;
        }
    }
}
