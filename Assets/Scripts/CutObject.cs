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
    public Mesh SphereMesh;
    public Mesh PlaneMesh;
    public Mesh CubeMesh;

    public float Value1;
    public float Value2;

    [HideInInspector]
    public CutType CutType;

    [HideInInspector]
    public List<CutItem> CutItems = new List<CutItem>();

    public void SetCutItems(List<string> names)
    {
        foreach(var name in names)
        {
            CutItems.Add(new CutItem() { Name = name, State = true });
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
        if (SceneManager.Instance.CutObjects.Contains(this))
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
            foreach (var b in CutItems.Where(b => b.Name == a))
            {
                contains = true;
            }

            if (!contains) AB.Add(a);
        }

        // find elements present in the destination but not in the source
        // these elements will be removed from the input source
        var BA = new List<string>();
        foreach (var b in CutItems)
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
            CutItems.Add(new CutItem() { Name = a, State = true });
        }

        // remove old elements
        foreach (var b in BA)
        {
            // find index of the element to remove 
            var index = -1;
            for (var i = 0; i < CutItems.Count; i++)
            {
                if (CutItems[i].Name != b) continue;
                index = i;
                break;
            }

            if(index == -1) throw new Exception();

            CutItems.RemoveAt(index);
        }
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                GetComponent<MeshFilter>().mesh = PlaneMesh;
            break;

            case CutType.Sphere:
                GetComponent<MeshFilter>().mesh = SphereMesh;
                break;

            case CutType.Cube:
                GetComponent<MeshFilter>().mesh = CubeMesh;
                break;
        }
    }
}
