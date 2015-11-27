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

    private int previousSelectedIndex = -1;

    // Use this for initialization
    void Start()
    {
        foreach (var cutObject in SceneManager.Instance.CutObjects)
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
    void Update()
    {
        if (listViewUI.SelectedIndex == -1)
            listViewUI.SelectedIndex = SceneManager.Instance.SelectedCutObject;

        if (listViewUI.SelectedIndex >= listViewUI.DataSource.Count)
        {
            listViewUI.SelectedIndex = listViewUI.DataSource.Count - 1;
            SceneManager.Instance.SelectedCutObject = listViewUI.SelectedIndex;
        }

        if (listViewUI.SelectedIndex != previousSelectedIndex)
        {
            for (int i = 0; i < SceneManager.Instance.CutObjects.Count; i++)
            {
                if (i != listViewUI.SelectedIndex) SceneManager.Instance.CutObjects[i].SetHidden(true);
                else SceneManager.Instance.CutObjects[i].SetHidden(false, true);
            }
            previousSelectedIndex = listViewUI.SelectedIndex;
        }

        SceneManager.Instance.SelectedCutObject = listViewUI.SelectedIndex;
        //Debug.Log(listViewUI.SelectedIndex);
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
        var cutObject = Instantiate(cutObjectPrefab).GetComponent<CutObject>();
        cutObject.name = "Cut Object " + CutObject.UniqueId; ;
        listViewUI.SelectedIndex = listViewUI.Add(cutObject.name);
    }

    public void RemoveCutObject()
    {
        if (listViewUI.DataSource.Count > 1)
        {
            var selected = listViewUI.SelectedIndicies;

            foreach (var index in selected)
            {
                listViewUI.Remove(listViewUI.DataSource[index]);
                var go = SceneManager.Instance.CutObjects[index].gameObject;
                DestroyImmediate(go);
            }
        }
    }
}
