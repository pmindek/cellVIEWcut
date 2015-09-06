using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum CutType
{
    Plane = 0,
    Sphere = 1,
    Cube = 2,
    Cylinder = 3,
    Cone = 4
};

[System.Serializable]
public class CutItem
{
    public string Name;
    public bool State;
}

[ExecuteInEditMode]
public class CutObject : MonoBehaviour
{
    public Material CutObjectMaterial;

    public CutType CutType;

    [HideInInspector]
    public CutType PreviousCutType;
    
    public bool Display = true;
    
    [Range(0,1)]
    public float Value1;

    [Range(0, 1)]
    public float Value2;

    [HideInInspector]
    public List<CutItem> ProteinCutFilters = new List<CutItem>();

    public void SetCutItems(List<string> names)
    {
        foreach(var name in names)
        {
            ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
        }
    }

    void Awake()
    {
        Debug.Log("Init cut object");
        ProteinCutFilters.Clear();
        SetCutItems(SceneManager.Instance.ProteinNames);
    }

    void OnEnable()
    {
        // Register this object in the cut object cache
        if (!SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Add(this);
        }
    }

    void OnDisable()
    {
        // De-register this object in the cut object cache
        if (SceneManager.CheckInstance() && SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Remove(this);
        }
    }

    // This function is meant to keep exisiting cut item and to preserve their original state
    // While removing items which are not present in the source list and that are present in the destination list
    // While also adding elements from the input list and which are not present in the destination list
    public void ResetCutItems(List<string> names)
    {
        // find elements present in source but not in destination
        // these elements will be added to the desitnation afterwards
        var AB = new List<string>();
        foreach (var a in names)
        {
            var contains = false;
            foreach (var b in ProteinCutFilters.Where(b => b.Name == a))
            {
                contains = true;
            }

            if (!contains) AB.Add(a);
        }

        // find elements present in the destination but not in the source
        // these elements will be removed from the input source
        var BA = new List<string>();
        foreach (var b in ProteinCutFilters)
        {
            var contains = false;
            foreach (var a in names.Where(a => b.Name == a))
            {
                contains = true;
            }

            if (!contains) BA.Add(b.Name);
        }

        // add new elements
        foreach (var a in AB)
        {
            ProteinCutFilters.Add(new CutItem() { Name = a, State = true });
        }

        // remove old elements
        foreach (var b in BA)
        {
            // find index of the element to remove 
            var index = -1;
            for (var i = 0; i < ProteinCutFilters.Count; i++)
            {
                if (ProteinCutFilters[i].Name != b) continue;
                index = i;
                break;
            }

            if(index == -1) throw new Exception();

            ProteinCutFilters.RemoveAt(index);
        }
    }
    
    void OnRenderObject()
    {
        if (!Display) return;

        if (CutType != PreviousCutType || gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            SetMesh();
            PreviousCutType = CutType;
        }

        var depthBuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 32, RenderTextureFormat.Depth);
        Graphics.SetRenderTarget(Graphics.activeColorBuffer, depthBuffer.depthBuffer);

        CutObjectMaterial.SetPass(0);
        Graphics.DrawMeshNow(gameObject.GetComponent<MeshFilter>().sharedMesh, transform.localToWorldMatrix);

        RenderTexture.ReleaseTemporary(depthBuffer);
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Plane") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Sphere:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Sphere") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<SphereCollider>();
                break;

            case CutType.Cube:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cube") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Cone:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cone") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Cylinder:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cylinder") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;
        }
    }
}
