using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIWidgets;

public class CutObjectUIController : MonoBehaviour
{
    public GameObject cutObjectPrefab;
    public ListView listViewUI;

    private List<DummyCutObject> cutObjects = new List<DummyCutObject>();

	// Use this for initialization
	void Start ()
	{
	    var cuts = GameObject.FindObjectsOfType<DummyCutObject>();
        cutObjects.AddRange(cuts);

	    foreach (var cutObject in cutObjects)
	    {
	        listViewUI.Add(cutObject.name);
	    }
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}
    
    public void AddCutObject()
    {
        cutObjects.Add(Instantiate(cutObjectPrefab).GetComponent<DummyCutObject>());
        listViewUI.Add(cutObjects.Last().name);
    }

    public void RemoveCutObject()
    {
        var selected = listViewUI.SelectedIndicies;

        foreach (var index in selected)
        {
            listViewUI.Remove(listViewUI.DataSource[index]);
            var go = cutObjects[index].gameObject;
            cutObjects.RemoveAt(index);
            DestroyImmediate(go);
        }
    }
}
