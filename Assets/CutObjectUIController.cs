using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIWidgets;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class CutObjectUIController : MonoBehaviour
{
    public GameObject cutObjectPrefab;
    public ListView listViewUI;
    public Combobox comboBox;

    public Slider fuzziness;
    public Slider distance;
    public Slider curve;

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

        for (CutType type = CutType.Plane; type <= CutType.None; type++)
        {
            string value2 = type.ToString();
            comboBox.ListView.Add(value2);
        }
    }
	
	// Update is called once per frame
	void Update ()
	{
	    var selected = listViewUI.SelectedIndicies;

	    if (selected.Count == 1)
	    {
	        
	    }

        var selectedCutObjects = new List<DummyCutObject>();
	    foreach (var s in selected)
	    {
	        selectedCutObjects.Add(cutObjects[s]);
	    }
	}

    public void SetFuzzinessSliderValue(float value)
    {
        fuzziness.value = value;
    }

    public void SetDistanceSliderValue(float value)
    {
        distance.value = value;
    }

    public void SetCurveSliderValue(float value)
    {
        curve.value = value;
    }

    public void OnFuzzinessValueChanged(float value)
    {
        int a = 0;
    }

    public void OnDistanceValueChanged(float value)
    {
        int a = 0;
    }

    public void OnCurveValueChanged(float value)
    {
        int a = 0;
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
