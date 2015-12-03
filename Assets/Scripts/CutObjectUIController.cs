using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UIWidgets;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;


public class CutObjectUIController : MonoBehaviour
{
    public GameObject cutObjectPrefab;
    public ListView listViewUI;
    public Combobox comboBox;
    //public Combobox comboBox2;

    public UILineRenderer FuzzinessPlot;
    public Slider FuzzinessSlider;
    public Slider DistanceSlider;
    public Slider CurveSlider;
    public Slider OcclusionSlider;
    public Slider ApertureSlider;
    public Slider CutObjectAlphaSlider;
    public Toggle InvertToggle;

    private int previousSelectedIndex = -1;
    private int previousComboBoxSelectedIndex = -1;

    public delegate void SelectedCutObjectChange();
    public event SelectedCutObjectChange OnSelectedCutObjectChange;

    // Use this for initialization
    void Start()
    {
        var t = GameObject.FindObjectsOfType<ComboBox>();

        foreach (var cutObject in SceneManager.Get.CutObjects)
        {
            listViewUI.Add(cutObject.name);
        }

        if (comboBox.ListView.DataSource.Count == 0)
        {
            for (CutType type = CutType.Plane; type <= CutType.None; type++)
            {
                string value2 = type.ToString();
                comboBox.ListView.Add(value2);
            }
        }

        CutObjectAlphaSlider.value = CutObject.CutObjectAlpha;

        //if (comboBox2.ListView.DataSource.Count == 0)
        //{
        //    comboBox2.ListView.Add("Show Current");
        //    comboBox2.ListView.Add("Show All");
        //    comboBox2.ListView.Add("Hide All");
        //}

        //comboBox2.Set("Show Current", false);

    }

    private bool ignoreUIChanges = false;

    // Update is called once per frame
    void Update()
    {
        if (listViewUI.SelectedIndex == -1)
            listViewUI.SelectedIndex = 0;

        if (listViewUI.SelectedIndex >= listViewUI.DataSource.Count)
        {
            listViewUI.SelectedIndex = listViewUI.DataSource.Count - 1;
            SceneManager.Get.SelectedCutObject = listViewUI.SelectedIndex;
        }

        if (listViewUI.SelectedIndex != previousSelectedIndex)
        {
            for (int i = 0; i < SceneManager.Get.CutObjects.Count; i++)
            {
                if (i != listViewUI.SelectedIndex) SceneManager.Get.CutObjects[i].SetHidden(true);
                else SceneManager.Get.CutObjects[i].SetHidden(false, true);
            }
            previousSelectedIndex = listViewUI.SelectedIndex;
            comboBox.Set(SceneManager.Get.CutObjects[listViewUI.SelectedIndex].CutType.ToString(), false);

            SceneManager.Get.SelectedCutObject = listViewUI.SelectedIndex;

            previousComboBoxSelectedIndex =
                comboBox.ListView.FindIndex(
                    SceneManager.Get.CutObjects[listViewUI.SelectedIndex].CutType.ToString());

            OnSelectedCutObjectChange();
        }
        else if (previousComboBoxSelectedIndex != comboBox.ListView.SelectedIndex)
        {
            SceneManager.Get.GetSelectedCutObject().CutType = GetCutTypeFromName(comboBox.ListView.DataSource[comboBox.ListView.SelectedIndex]);
            SceneManager.Get.GetSelectedCutObject().SetHidden(false, true);
            previousComboBoxSelectedIndex = comboBox.ListView.SelectedIndex;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            CutObject.CutObjectAlpha = Mathf.Max(0.25f, CutObject.CutObjectAlpha);
            SceneManager.Get.GetSelectedCutObject().GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_CutObjectAlpha", Mathf.Max(0.25f, CutObject.CutObjectAlpha));
            

            for (int i = 0; i < SceneManager.Get.CutObjects.Count; i++)
            {
                if (i == listViewUI.SelectedIndex) SceneManager.Get.CutObjects[i].SetHidden(false);
                else
                {
                    SceneManager.Get.CutObjects[i].SetHidden(false);
                    if (SceneManager.Get.CutObjects[i].GetComponent<TransformHandle>().IsEnabled())
                    {

                        listViewUI.Set(SceneManager.Get.CutObjects[i].name, false);
                        //  SelectionManager.Instance.SetHandleSelected();
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            CutObject.CutObjectAlpha = CutObjectAlphaSlider.value;
            SceneManager.Get.GetSelectedCutObject().GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_CutObjectAlpha", CutObject.CutObjectAlpha);

            for (int i = 0; i < SceneManager.Get.CutObjects.Count; i++)
            {
                if (i != listViewUI.SelectedIndex) SceneManager.Get.CutObjects[i].SetHidden(true);
                else SceneManager.Get.CutObjects[i].SetHidden(false);
            }
        }

    }

    // Set UI values

    public void HideOcclusionUIFields(bool value)
    {
        ApertureSlider.transform.parent.gameObject.SetActive(!value);
        OcclusionSlider.transform.parent.gameObject.SetActive(!value);
    }

    public void SetInvertToggleValue(bool value)
    {
        InvertToggle.isOn = value;
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

    public void SetOcclusionUIValue(float value)
    {
        OcclusionSlider.value = value;
    }

    public void SetApertureUIValue(float value)
    {
        ApertureSlider.value = value;
    }

    // Event Callbacks

    public void OnInvertValueChanged(bool value)
    {
        SceneManager.Get.GetSelectedCutObject().Inverse = value;
    }

    public void OnFuzzinessValueChanged(float value)
    {
        FuzzinessPlot.Decay = FuzzinessSlider.value;
        FuzzinessPlot.Gamma = CurveSlider.value;
        FuzzinessPlot.SetVerticesDirty();
    }

    public void OnDistanceValueChanged(float value)
    {
        
    }

    public void OnCurveValueChanged(float value)
    {
        FuzzinessPlot.Decay = FuzzinessSlider.value;
        FuzzinessPlot.Gamma = CurveSlider.value;
        FuzzinessPlot.SetVerticesDirty();
    }

    public void OnObjectAlphaValueChanged(float value)
    {
        CutObject.CutObjectAlpha = value;
        SceneManager.Get.GetSelectedCutObject().GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_CutObjectAlpha", value);
        MyHandleUtility.HandleMaterial.SetFloat("_CutObjectAlpha", value);
    }

    public void AddCutObject()
    {
        var cutObject = Instantiate(cutObjectPrefab).GetComponent<CutObject>();
        cutObject.Update();
        cutObject.name = "Cut Object " + cutObject.Id;
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
                var go = SceneManager.Get.CutObjects[index].gameObject;
                DestroyImmediate(go);
            }
        }

        previousSelectedIndex = -1;
        listViewUI.SelectedIndex = Mathf.Min(cache, listViewUI.DataSource.Count - 1);
    }

    // ******** Misc 

    CutType GetCutTypeFromName(string name)
    {
        for (CutType type = CutType.Plane; type <= CutType.None; type++)
        {
            if (name == type.ToString()) return type;
        }
        throw new Exception("Cut type not found");
    }
}
