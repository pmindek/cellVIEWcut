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
    public bool Hidden;

    public CutType CutType;
    
    [Range(0, 1)]
    public float Value1;

    [Range(0, 1)]
    public float Value2;

    [HideInInspector]
    public CutType PreviousCutType;

    [HideInInspector]
    public List<CutItem> ProteinCutFilters = new List<CutItem>();

    public void SetProteinCutFilters(List<string> names)
    {
        foreach (var name in names)
        {
            ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
        }
    }

    void Awake()
    {
        if (ProteinCutFilters.Count == 0)
        {
            SetProteinCutFilters(SceneManager.Instance.ProteinNames);
        }
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

    void Update()
    {
        if (CutType != PreviousCutType || gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            SetMesh();
            PreviousCutType = CutType;
        }

        if (Hidden || Application.isPlaying)
        {
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<TransformHandle>().enabled = false;
        }
        else
        {
            GetComponent<Collider>().enabled = true;
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<TransformHandle>().enabled = true;
        }
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Plane") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<BoxCollider>();
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
