using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public static class TreeUtility
{
    public static string GetNodeName(string nodePath)
    {
        var split = nodePath.Split('.');
        return split.Last();
    }

    public static string GetNodeParentPath(string nodePath)
    {
        var split = nodePath.Split('.').ToList();
        split.Remove(split.Last());

        if (split.Count == 0)
        {
            return "";
        }

        var value = split.First();
        split.Remove(split.First());

        foreach (var s in split)
        {
            value += "." + s;
        }

        return value;
    }

    public static GameObject InstantiateNodeField(string type)
    {
        // Instantiate node
        if (type == "Text")
        {
            return GameObject.Instantiate(Resources.Load("TextField", typeof(GameObject))) as GameObject;
        }
        else if(type == "Toggle")
        {
            return GameObject.Instantiate(Resources.Load("ToggleField", typeof(GameObject))) as GameObject;
        }
        else if (type == "Range")
        {
            return GameObject.Instantiate(Resources.Load("RangeFieldItem", typeof(GameObject))) as GameObject;
        }

        return null;
    }
}
