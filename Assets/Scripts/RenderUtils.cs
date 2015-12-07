using System;
using UnityEngine;
using System.Collections;

public static class RenderUtils
{
    public static void DrawProteinSphereBatches(Material renderProteinsMaterial, Camera camera, RenderBuffer colorBuffer, RenderBuffer depthBuffer, int pass)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", PersistantSettings.Instance.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Instance.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Instance.ProteinInstanceInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Instance.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Instance.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Instance.ProteinColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Instance.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Instance.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Instance.SphereBatches);

        Graphics.SetRenderTarget(colorBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(pass);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Instance.ArgBuffer);
    }

    public static void ComputeSphereBatches(Camera camera)
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Instance.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", PersistantSettings.Instance.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(PersistantSettings.Instance.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", camera.transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", camera.transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(camera));

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

        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Instance.SphereBatches, GPUBuffers.Instance.ArgBuffer, 0);
    }
}
