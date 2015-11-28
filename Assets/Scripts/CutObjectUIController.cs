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

    public Slider FuzzinessSlider;
    public Slider DistanceSlider;
    public Slider CurveSlider;

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
            listViewUI.SelectedIndex = 0;

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

    public void HideFuzzinessUIPanel(bool value)
    {
        FuzzinessSlider.transform.parent.parent.gameObject.SetActive(!value);
    }

    public void SetFuzzinessSliderValue(float value)
    {
        FuzzinessSlider.value = value;
    }

    public void SetDistanceSliderValue(float value)
    {
        DistanceSlider.value = value;
    }

    public void SetCurveSliderValue(float value)
    {
        CurveSlider.value = value;
    }

    public void OnInvertValueChanged(bool value)
    {
        SceneManager.Instance.GetSelectedCutObject().Inverse = value;
    }

    //public void OnFuzzinessValueChanged(float value)
    //{
    //    int a = 0;
    //}

    //public void OnDistanceValueChanged(float value)
    //{
    //    int a = 0;
    //}

    //public void OnCurveValueChanged(float value)
    //{
    //    int a = 0;
    //}

    public void AddCutObject()
    {
        var cutObject = Instantiate(cutObjectPrefab).GetComponent<CutObject>();
        cutObject.Update();
        cutObject.name = "Cut Object " + (CutObject.UniqueId - 1);
        listViewUI.SelectedIndex = listViewUI.Add(cutObject.name);
        Debug.Log(listViewUI.SelectedIndex);
        previousSelectedIndex = -1;
        //listViewUI.SelectedIndex = listViewUI.DataSource.Count-1;
    }

    public void RemoveCutObject()
    {
        var cache = listViewUI.SelectedIndex;
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

        previousSelectedIndex = -1;
        listViewUI.SelectedIndex = Mathf.Min(cache, listViewUI.DataSource.Count - 1);
    }
}
