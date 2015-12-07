using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RenderShadowMap : MonoBehaviour
{
    public Material RenderProteinMaterial;

    public RenderTexture ShadowMap;
    public RenderTexture ShadowMap2;

    /*****/

    private Camera _camera;

    /*****/

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth;
        _camera.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    void OnPostRender()
    {
        Graphics.SetRenderTarget(ShadowMap2);
        GL.Clear(true, true, Color.black);

        RenderUtils.ComputeSphereBatches(_camera);
        RenderUtils.DrawProteinSphereBatches(RenderProteinMaterial, _camera, ShadowMap2.colorBuffer, ShadowMap2.depthBuffer, 1);
    }

    //void OnPostRender()
    //{
    //    Graphics.SetRenderTarget(ShadowMap2.colorBuffer, ShadowMap2.depthBuffer);
    //    GL.Clear(true, true, new Color(-1,-1,-1,-1));

    //    RenderUtils.ComputeSphereBatches(_camera);
    //    RenderUtils.DrawProteinSphereBatches(RenderProteinMaterial, _camera, ShadowMap2.colorBuffer, ShadowMap2.depthBuffer, 0);
    //}
}