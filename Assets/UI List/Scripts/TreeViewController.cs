using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using UnityEngine.EventSystems;

public class TreeViewController : MonoBehaviour, IEventSystemHandler
{
    public int TextFontSize;

    public float ArrowSize;
    public float TextFieldSize;

    public float Indent;
    public float Spacing;
    public float TopPadding;
    public float LeftPadding;
    public bool EnableLensEffect = false;

    public GameObject BaseItemPrefab;
    public CutObjectUIController cutObjectUiController;

    //*******//

    private float lastRange0;
    private float lastRange1;
    private float lastValue1;
    private float lastValue2;
    private float rangeAdjust0;
    private float rangeAdjust1;
    private float lastValueChange0;
    private float lastValueChange1;
    private float lastRangeChange0;
    private float lastRangeChange1;
    private CutParameters currentParameters;

    private bool once = false;
    private BaseItem onceItem = null;

    //*******//

    private BaseItem _selectedNode;
    private List<BaseItem> _rootNodes;

    private float _currentDragValue = 0.0f;
    private bool _setHistogramsDirty = true;

    // Use this for initialization
    void Start()
    {
        if (_rootNodes == null) _rootNodes = new List<BaseItem>();
        foreach (var node in PersistantSettings.Instance.hierachy)
        {
            AddNodeObject(node.path, new object[] { node.name }, "Text");
        }
        InitNodeItems();

        // Register event callbacks
        foreach (var node in _rootNodes)
        {
            node.RangeFieldItem.CustomRangeSliderUi.RangeSliderDrag += OnRangeSliderDrag;
            node.PointerClick += OnNodePointerClick;
        }

        HideFuzzinessUIPanel(false);
        SetFuzzinessUIValues(0.1f, 0.5f, 0.9f);

        cutObjectUiController.OnSelectedCutObjectChange += OnSelectedCutObjectChange;

        _selectedNode = _rootNodes[0];
    }
    
    public void HideFuzzinessUIPanel(bool value)
    {
        cutObjectUiController.HideFuzzinessUIPanel(value);
    }

    public void OnFuzzinessChanged(float value)
    {
        Debug.Log(value);
    }

    public void OnFuzzinessDistanceChanged(float value)
    {
        Debug.Log(value);
    }

    public void OnFuzzinessCurveChanged(float value)
    {
        Debug.Log(value);
    }

    public void OnSelectedCutObjectChange()
    {
        UpdateSelectedNodeOcclusionValue();
        UpdateAllToggles();
        if (_rootNodes == null || SceneManager.Instance.histograms == null) return;
    }

    
    
    public
        void SetOcclusionUIValue(float value)
    {
        cutObjectUiController.SetOcclusionUIValue(value);
    }

    public void SetFuzzinessUIValues(float value1, float value2, float value3)
    {
        cutObjectUiController.SetFuzzinessSliderValue(value1);
        cutObjectUiController.SetDistanceSliderValue(value2);
        cutObjectUiController.SetCurveSliderValue(value3);
    }

    public void OnNodePointerClick(BaseItem selectedNode)
    {
        _selectedNode = selectedNode;
        UpdateSelectedNodeOcclusionValue();
    }

    private List<BaseItem> GetAllLeaves(BaseItem baseItem)
    {
        var selectedLeafNodes = new List<BaseItem>();
        if (_selectedNode.IsLeafNode()) selectedLeafNodes.Add(_selectedNode);
        else selectedLeafNodes.AddRange(_selectedNode.GetAllLeafChildren());
        return selectedLeafNodes;
    }

    private bool _ignoreUIChangesFlag;
    private float _currentOcclusionValue;

    public void UpdateSelectedNodeOcclusionValue()
    {
        _ignoreUIChangesFlag = true;
        var averageValues = GetAverageCutParamsFromLeafNodes(GetAllLeaves(_selectedNode));
        _currentOcclusionValue = averageValues.value2;
        SetOcclusionUIValue(_currentOcclusionValue);
    }
    
    public void OnOcclusionUIValueChanged(float value)
    {
        _currentOcclusionValue = value;

        if (!_ignoreUIChangesFlag)
        {
            foreach (var leafNodes in GetAllLeaves(_selectedNode))
            {
                foreach (var cutObject in SceneManager.Instance.CutObjects)
                {
                    cutObject.ProteinTypeParameters[SceneManager.Instance.NodeToProteinLookup[leafNodes.Id]].value2 = _currentOcclusionValue;
                }
            }
        }
        else
        {
            _ignoreUIChangesFlag = false;
        }
    }

    void UpdateAllToggles()
    {
        foreach (var node in _rootNodes)
        {
            if (node.IsLeafNode())
            {
                var toggleState = SceneManager.Instance.GetSelectedCutObject().ProteinTypeParameters[SceneManager.Instance.NodeToProteinLookup[node.Id]].IsFocus;
                node.RangeFieldItem.Toggle.SetState(toggleState);
                node.RangeFieldItem.LockToggle.gameObject.SetActive(toggleState);

                if (toggleState && SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Unlocked)
                {
                    node.RangeFieldItem.LockToggle.Locked = false;
                }

                if (toggleState && SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Locked)
                {
                    node.RangeFieldItem.LockToggle.Locked = true;
                }
            }
        }

        foreach (var node in _rootNodes)
        {
            if (!node.IsLeafNode())
            {
                var toggleState = node.HasSomeChildrenFocus();
                node.RangeFieldItem.Toggle.SetState(toggleState);
                node.RangeFieldItem.LockToggle.gameObject.SetActive(toggleState);

                if (toggleState && SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Unlocked)
                {
                    node.RangeFieldItem.LockToggle.Locked = false;
                }

                if (toggleState && SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Locked)
                {
                    node.RangeFieldItem.LockToggle.Locked = true;
                }
            }
        }
    }

    public void OnFocusToggleClick(BaseItem item)
    {
        var value = item.RangeFieldItem.Toggle.isOn;

        item.RangeFieldItem.Toggle.SetState(value);
        item.RangeFieldItem.LockToggle.gameObject.SetActive(value);

        if (item.IsLeafNode())
        {
            var cutObject = SceneManager.Instance.GetSelectedCutObject();
            SceneManager.Instance.GetSelectedCutObject().ProteinTypeParameters[SceneManager.Instance.NodeToProteinLookup[item.Id]].IsFocus = value;
        }

        foreach (var child in item.GetAllChildren())
        {
            if (child.IsLeafNode())
            {
                var cutObject = SceneManager.Instance.GetSelectedCutObject();
                SceneManager.Instance.GetSelectedCutObject().ProteinTypeParameters[SceneManager.Instance.NodeToProteinLookup[child.Id]].IsFocus = value;
            }

            child.RangeFieldItem.Toggle.SetState(value);
            child.RangeFieldItem.LockToggle.gameObject.SetActive(value);
        }

        SetAllLockState(false);
    }

    public void SetAllLockState(bool value)
    {
        foreach (var node in _rootNodes)
        {
            if(node.RangeFieldItem.Toggle.isOn)
                node.RangeFieldItem.LockToggle.SetState(value);
        }

        if (value && SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Unlocked)
        {
            SceneManager.Instance.GetSelectedCutObject().CurrentLockState = LockState.Locked;
        }

        if (!value && SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Consumed)
        {
            SceneManager.Instance.GetSelectedCutObject().CurrentLockState = LockState.Restore;
        }
    }

    

    //public void OnLockToggle(BaseItem baseItem)
    //{
    //    bool lockValue = baseItem.RangeFieldItem.LockToggle.Locked;
    //    SetAllFocusNodeLockState(lockValue);
    //}

    //public void OnToggleItem1(BaseItem baseItem)
    //{
    //    var value = baseItem.RangeFieldItem.Toggle.isOn;

    //    foreach (var child in baseItem.Children)
    //    {
    //        child.RangeFieldItem.Toggle.isOn = value;
    //    }

    //    if (baseItem.IsLeafNode())
    //    {
    //        foreach (var cutObject in SceneManager.Instance.GetSelectedCutObjects())
    //        {
    //            cutObject.SetFocusFor(SceneManager.Instance.NodeToProteinLookup[baseItem.Id], value);
    //        }
    //    }

    //    SetAllFocusNodeLockState(false);
    //}

    //public void SetAllFocusNodeLockState(bool locked)
    //{
    //    foreach (var node in _rootNodes)
    //    {
    //        node.RangeFieldItem.LockToggle.SetState(locked);
    //    }

    //    if (SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Unlocked && locked)
    //    {
    //        SceneManager.Instance.GetSelectedCutObject().CurrentLockState = LockState.Locked;
    //    }
    //    else if (SceneManager.Instance.GetSelectedCutObject().CurrentLockState == LockState.Consumed && !locked)
    //    {
    //        SceneManager.Instance.GetSelectedCutObject().CurrentLockState = LockState.Restore;
    //    }
    //}

    //public void OnToggleItem2(BaseItem baseItem)
    //{
    //    var value = baseItem.RangeFieldItem.Toggle2.isOn;

    //    foreach (var child in baseItem.Children)
    //    {
    //        child.RangeFieldItem.SetToggle2(value);
    //    }
    //}

    //public List<bool> GetAllToggleItems1()
    //{
    //    return _rootNodes.Select(node => node.RangeFieldItem.Toggle1.isOn).ToList();
    //}

    //public List<bool> GetAllToggleItems2()
    //{
    //    return _rootNodes.Select(node => node.RangeFieldItem.Toggle2.isOn).ToList();
    //}

    public CutParameters GetAverageCutParamsFromLeafNodes(List<BaseItem> leafNodes)
    {
        var cutParams = new CutParameters();

        foreach (var leafNode in leafNodes)
        {
            var index = SceneManager.Instance.NodeToProteinLookup[leafNode.Id];
            if(index < 0) throw new Exception("Node to protein lookup error");

            foreach (var cutObject in SceneManager.Instance.GetSelectedCutObjects())
            {
                var cutParam = cutObject.GetCutParametersFor(index);
                cutParams.value1 += cutParam.value1;
                cutParams.value2 += cutParam.value2;
            }
        }

        var averageTotalCount = (float)(leafNodes.Count * SceneManager.Instance.GetSelectedCutObjects().Count);
        cutParams.value1 /= averageTotalCount;
        cutParams.value2 /= averageTotalCount;
        return cutParams;
    }
    
    public void OnRangeSliderDrag(BaseItem targetNode, int rangeIndex, float dragDelta)
    {
        var selectedLeafNodes = new List<BaseItem>();

        if (targetNode.IsLeafNode()) selectedLeafNodes.Add(targetNode);
        else selectedLeafNodes.AddRange(targetNode.GetAllLeafChildren());

        // Init current drag value
        if (targetNode.RangeFieldItem.CustomRangeSliderUi.StartedDragging)
        {
            var averageCutParams = GetAverageCutParamsFromLeafNodes(selectedLeafNodes);
            _currentDragValue = rangeIndex == 0 ? averageCutParams.value2 : averageCutParams.value1;
        }
        
        _currentDragValue += dragDelta / 200;
        _currentDragValue = Mathf.Min(1.0f, Mathf.Max(0.0f, _currentDragValue));

        // Set new cut params values
        foreach (var child in selectedLeafNodes)
        {
            var index = SceneManager.Instance.NodeToProteinLookup[child.Id];
            if (index < 0) throw new Exception("Node to protein lookup error");
            
            foreach (var cutObject in SceneManager.Instance.GetSelectedCutObjects())
            {
                if (rangeIndex == 0)
                {
                    cutObject.SetValue2For(index, _currentDragValue);
                    PersistantSettings.Instance.AdjustVisible = _currentDragValue;
                }

                if (rangeIndex == 1)
                {
                    cutObject.SetValue1For(index, _currentDragValue);
                }
            }
        }

        _setHistogramsDirty = true;
    }

    public void FixedUpdate()
    {
        UpdateNodeItems();
        UpdateHistograms();
    }

    public void UpdateHistograms()
    {
        if (_rootNodes == null || SceneManager.Instance.histograms == null) return;

        foreach (var node in _rootNodes)
        {
            HistStruct hist = SceneManager.Instance.histograms[node.Id];
            List<float> oldRangeValues = node.RangeFieldItem.GetRangeValues();
            List<float> newRangeValues = new List<float>();

            var newRange0 = (float) hist.visible/(float) hist.all;
            var newRange0Smooth = oldRangeValues[0] + ((newRange0 - oldRangeValues[0])*0.1f);

            newRangeValues.Add(newRange0Smooth);
            newRangeValues.Add(1.0f - (float) hist.cutaway/(float) hist.all - newRangeValues[0]);
            newRangeValues.Add(1.0f - newRangeValues[0] - newRangeValues[1]);
            node.RangeFieldItem.SetRangeValues(newRangeValues);

            
        }
    }

    //public void OnRangeSliderDrag(BaseItem node, int rangeIndex, float dragDelta)
    //{
    //    //Debug.Log(node.Id + " " + rangeIndex + " " + dragDelta);


    //    RangeFieldItem range = node.FieldObject.GetComponent<RangeFieldItem>();




    //    //initialize the dragging - average values for all dragged sliders and all selected cut objects
    //    if (range.CustomRangeSliderUi.StartedDragging)
    //    {
    //        range.CustomRangeSliderUi.StartedDragging = false;

    //        BaseItem baseItem = FindBaseItem(node.Path);
    //        List<BaseItem> children = baseItem.GetAllChildren();

    //        children.Add(baseItem);

    //        selectedIngredients.Clear();

    //        foreach (var child in children)
    //        {
    //            if (child.Children.Count == 0)
    //            {
    //                Debug.Log("|~!!~ " + SceneManager.Instance.HistogramsReverseLookup[child.Id]);
    //                selectedIngredients.Add(SceneManager.Instance.HistogramsReverseLookup[child.Id]);
    //            }
    //        }


    //        //calculate occlusion queries where selectedIngredients (those whose histograms are we dragging) are ocludees
    //        Debug.Log("DO TOGGLE");
    //        foreach (var cut in SceneManager.Instance.GetSelectedCutObjects())
    //        {
    //            cut.ToggleAllCutItem(true);
    //            foreach (var si in selectedIngredients)
    //            {
    //                cut.ToggleCutItem(SceneManager.Instance.ProteinNames[si], false);
    //            }
    //        }


    //        float fc = 0;

    //        float averageValue = 0.0f;

    //        foreach (var cut in SceneManager.Instance.GetSelectedCutObjects())
    //        {
    //            for (int i = 0; i < selectedIngredients.Count; i++)
    //            {
    //                CutParameters param = cut.GetCutParametersFor(selectedIngredients[i]);

    //                averageValue += (rangeIndex == 0 ? param.value2 : param.value1);
    //            }
    //        }

    //        fc = (float)(selectedIngredients.Count * SceneManager.Instance.GetSelectedCutObjects().Count);

    //        averageValue /= fc;

    //        value = averageValue;

    //    } //end of initialization

    //    value += dragDelta / 200;
    //    value = Mathf.Min(1.0f, Mathf.Max(0.0f, value));

    //    //now we set these values to every cut object's record for the manipulated protein types
    //    foreach (var cut in SceneManager.Instance.GetSelectedCutObjects())
    //    {
    //        for (int i = 0; i < selectedIngredients.Count; i++)
    //        {
    //            if (rangeIndex == 0)
    //            {
    //                cut.SetValue2For(selectedIngredients[i], value);
    //                PersistantSettings.Instance.AdjustVisible = value;
    //            }

    //            if (rangeIndex == 1)
    //            {
    //                cut.SetValue1For(selectedIngredients[i], value);
    //            }

    //        }
    //    }
    //}

    //public void UpdateRangeValues()
    //{

    //    if (RootNodes != null)
    //        foreach (var Node in RootNodes)
    //        {
    //            HistStruct hist = SceneManager.Instance.histograms[Node.Id];

    //            List<float> rangeValues = new List<float>();
    //            rangeValues.Clear();

    //            rangeValues.Add((float)hist.visible / (float)hist.all);
    //            rangeValues.Add(1.0f - (float)hist.cutaway / (float)hist.all - rangeValues[0]);
    //            rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);

    //            Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);
    //        }

    //    return;

    //    if(RootNodes != null)
    //    foreach (var Node in RootNodes)
    //    {
    //        RangeFieldItem range = Node.FieldObject.GetComponent<RangeFieldItem>();






    //        //on mouse up
    //        if (range.CustomRangeSliderUi.StoppedDragging)
    //        {
    //            range.CustomRangeSliderUi.StoppedDragging = false;
    //            Debug.Log("mouse up up up");

    //            foreach (var cut in SceneManager.Instance.CutObjects)
    //            {
    //                cut.ToggleAllCutItem(true);
    //            }
    //        }





    //        //not dragging it
    //        //else
    //        if (!range.CustomRangeSliderUi.DragState || range.CustomRangeSliderUi.recalcOnce)
    //        {
    //            HistStruct hist = SceneManager.Instance.histograms[Node.Id];

    //            List<float> rangeValues = new List<float>();
    //            rangeValues.Clear();

    //            rangeValues.Add((float)hist.visible / (float)hist.all);
    //            rangeValues.Add(1.0f - (float)hist.cutaway / (float)hist.all - rangeValues[0]);
    //            rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);

    //            Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);



    //            range.CustomRangeSliderUi.recalcOnce = false;
    //            range.CustomRangeSliderUi.disableDragging = false;
    //            range.CustomRangeSliderUi.StartedDragging = true;
    //        }






    //        //if dragging the RangeSlider
    //        if (range.CustomRangeSliderUi.DragState)
    //        {
    //            //initialize the dragging - average all the parameters for all dragged sliders and all selected cut objects
    //            if (range.CustomRangeSliderUi.StartedDragging)
    //            {
    //                range.CustomRangeSliderUi.StartedDragging = false;

    //                BaseItem baseItem = FindBaseItem(Node.Path);
    //                List<BaseItem> children = baseItem.GetAllChildren();

    //                children.Add(baseItem);

    //                selectedIngredients.Clear();

    //                foreach (var child in children)
    //                {
    //                    if (child.Children.Count == 0)
    //                    {
    //                        Debug.Log("|~!!~ " + SceneManager.Instance.HistogramsReverseLookup[child.Id]);
    //                        selectedIngredients.Add(SceneManager.Instance.HistogramsReverseLookup[child.Id]);
    //                    }
    //                }




    //                //calculate occlusion queries where selectedIngredients (those whose histograms are we dragging) are ocludees
    //                Debug.Log("DO TOGGLE");
    //                foreach (var cut in SceneManager.Instance.CutObjects)
    //                {
    //                    cut.ToggleAllCutItem(true);
    //                    foreach (var si in selectedIngredients)
    //                    {
    //                        cut.ToggleCutItem(SceneManager.Instance.ProteinNames[si], false);
    //                    }
    //                }







    //                currentParameters = new CutParameters()
    //                {
    //                    range0 = 0.0f,
    //                    range1 = 0.0f,

    //                    countAll = 0,
    //                    count0 = 0,
    //                    count1 = 0,

    //                    value1 = 0.0f,
    //                    value2 = 0.0f,
    //                    fuzziness = 0.0f,
    //                    fuzzinessDistance = 0.0f,
    //                    fuzzinessCurve = 0.0f
    //                };

    //                CutParameters param = null;

    //                float fc = 0;

    //                foreach (var cut in SceneManager.Instance.CutObjects)
    //                {
    //                    for (int i = 0; i < selectedIngredients.Count; i++)
    //                    {
    //                        param = cut.GetCutParametersFor(selectedIngredients[i]);

    //                        currentParameters.range0 += param.range0;
    //                        currentParameters.range1 += param.range1;

    //                        currentParameters.countAll += param.countAll;
    //                        currentParameters.count0 += param.count0;
    //                        currentParameters.count1 += param.count1;

    //                        currentParameters.value1 += param.value1;
    //                        currentParameters.value2 += param.value2;
    //                        currentParameters.fuzziness += param.fuzziness;
    //                        currentParameters.fuzzinessDistance += param.fuzzinessDistance;
    //                        currentParameters.fuzzinessCurve += param.fuzzinessCurve;
    //                    }
    //                }

    //                fc = (float)(selectedIngredients.Count * SceneManager.Instance.CutObjects.Count);

    //                currentParameters.range0 /= fc;
    //                currentParameters.range1 /= fc;

    //                currentParameters.value1 /= fc;
    //                currentParameters.value2 /= fc;
    //                currentParameters.fuzziness /= fc;
    //                currentParameters.fuzzinessDistance /= fc;
    //                currentParameters.fuzzinessCurve /= fc;

    //                Debug.Log("value1 = " + currentParameters.value1);
    //                Debug.Log("value2 = " + currentParameters.value2);
    //                Debug.Log("fuzziness = " + currentParameters.fuzziness);
    //                Debug.Log("distance = " + currentParameters.fuzzinessDistance);
    //                Debug.Log("curve = " + currentParameters.fuzzinessCurve);


    //                /* lastValueChange0 = Mathf.Abs(lastValue1 - currentParameters.value1);
    //                 lastValueChange1 = Mathf.Abs(lastValue2 - currentParameters.value2);

    //                 lastRangeChange0 = Mathf.Abs(lastRange0 - rangeValues[0]);
    //                 lastRangeChange1 = Mathf.Abs(lastRange1 - rangeValues[1]);*/

    //                rangeValues = Node.RangeFieldItem.GetRangeValues();

    //                lastRange0 = rangeValues[0];
    //                lastRange1 = rangeValues[1];

    //                lastValue1 = currentParameters.value1;
    //                lastValue2 = currentParameters.value2;
    //            } //end of initialization


    //            //rangeValues = Node.RangeFieldItem.GetRangeValues();

    //            /*rangeValues.Clear();
    //            rangeValues.Add(currentParameters.range0);
    //            rangeValues.Add(currentParameters.range1);
    //            rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);*/




    //            //this happens on every frame while dragging




    //            //HistStruct hist = SceneManager.Instance.histograms[Node.Id];

    //            /*rangeValues.Clear();

    //            rangeValues.Add((float)hist.occluding / (float)hist.all);
    //            rangeValues.Add(1.0f - (float)hist.cutaway / (float)hist.all - rangeValues[0]);
    //            rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);*/

    //            //Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);

    //            float d0 = 0.0f;
    //            float d1 = 0.0f;

    //            if (lastRange0 > rangeValues[0])
    //            {
    //                d0 = -1.0f + rangeValues[0] / lastRange0;
    //            }
    //            else if (lastRange0 < rangeValues[0])
    //            {
    //                d0 = (rangeValues[0] - lastRange0) / (lastRange1 - lastRange0);
    //            }

    //            if (lastRange1 > rangeValues[1])
    //            {
    //                d1 = -1.0f + rangeValues[1] / lastRange1;
    //            }
    //            else if (lastRange1 < rangeValues[1])
    //            {
    //                d1 = (rangeValues[1] - lastRange1) / (1 - lastRange1 - lastRange0);
    //            }

    //            if (d0 < -1.0f)
    //                d0 = -1.0f;
    //            if (d0 > 1.0f)
    //                d0 = 1.0f;
    //            if (d1 < -1.0f)
    //                d1 = -1.0f;
    //            if (d1 > 1.0f)
    //                d1 = 1.0f;

    //            if (Mathf.Abs(d0) > 0.001f)
    //                d1 = 0.0f;

    //            float adjust0 = 1.0f;
    //            float adjust1 = 1.0f;

    //            float v1 = lastValue1 + d1 * adjust1;
    //            float v2 = lastValue2 + d0 * adjust0;

    //            if (v1 < 0.0f)
    //                v1 = 0.0f;
    //            if (v1 > 1.0f)
    //                v1 = 1.0f;
    //            if (v2 < 0.0f)
    //                v2 = 0.0f;
    //            if (v2 > 1.0f)
    //                v2 = 1.0f;


    //            currentParameters.value1 = v1;
    //            currentParameters.value2 = v2;




    //            HistStruct hist = SceneManager.Instance.histograms[Node.Id];

    //            currentParameters.countAll = hist.all;
    //            currentParameters.count0 = hist.cutaway;
    //            currentParameters.count1 = hist.visible;

    //            float st_cutaway = (float)currentParameters.count0;
    //            float st_all = (float)currentParameters.countAll;
    //            float st_occluding = (float)currentParameters.count1;


    //            List<float> rv = new List<float>();
    //            rv.Clear();

    //            rv.Add((float)hist.visible / (float)hist.all);
    //            rv.Add(1.0f - (float)hist.cutaway / (float)hist.all - rv[0]);
    //            rv.Add(1.0f - rv[0] - rv[1]);





    //            currentParameters.range0 = rangeValues[0];
    //            currentParameters.range1 = rangeValues[1];


    //            //now we set these values to every cut object's record for the manipulated protein types
    //            foreach (var cut in SceneManager.Instance.CutObjects)
    //            {
    //                for (int i = 0; i < selectedIngredients.Count; i++)
    //                {
    //                    cut.SetCutParametersFor(selectedIngredients[i], currentParameters);
    //                }
    //            }


    //            //Debug.Log("is " + rangeValues[0] + "; want " + rv[0]);
    //            //Debug.Log("is " + rangeValues[1] + "; want " + rv[1]);
    //            Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);
    //            Node.FieldObject.GetComponent<RangeFieldItem>().SetFakeRangeValues(rv);

    //            PersistantSettings.Instance.AdjustVisible = v2;

    //            //nextRangeValues = rv;

    //            //range.CustomRangeSliderUi.disableDragging = true;
    //            //range.CustomRangeSliderUi.recalcOnce = true;
    //        }
    //    }        
    //}

    public void LogRangeValues()
    {
        foreach (var Node in _rootNodes)
        {
            Node.FieldObject.GetComponent<RangeFieldItem>().GetRangeValues();
            //Node.Name
        }
    }

    public BaseItem FindBaseItem(string path)
    {
        return _rootNodes.FirstOrDefault(n => n.Path == path);
    }

    // Add a new object to the tree
	public void AddNodeObject(string fullPath, object[] args, string type)
	{
        var name = TreeUtility.GetNodeName(fullPath);
	    var parentPath = TreeUtility.GetNodeParentPath(fullPath);

        // If the node is a root node
        if (string.IsNullOrEmpty(parentPath))
	    {
            var node = CreateNodeObject(name, fullPath, args, type);
            _rootNodes.Add(node);
        }
        // If the node is a child node
	    else
        {
            var parentNode = FindBaseItem(parentPath);

            if (parentNode != null)
            {
                var node = CreateNodeObject(name, fullPath, args, type);
                _rootNodes.Add(node);
                parentNode.AddChild(node);
            }
            else
            {
                throw new Exception("System error");
            }
        }
    }
    
    public BaseItem CreateNodeObject(string name, string fullPath, object[] args, string type)
    {
        // Instantiate prefac
        var go = GameObject.Instantiate(BaseItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        go.transform.SetParent(this.transform, false);

        var node = go.GetComponent<BaseItem>();
        node.Initialize(name, fullPath, type, args, false, this, _rootNodes.Count);
        
        return node;
    }

    public void InitNodeItems()
    {
        initState = true;
        UpdateLayout();
    }

    // Reorder the tree elements
    public void UpdateLayout()
    {
        // TODO: Use the real base node height here
        float currentYPos = - (maxDistanceY + 10); 

        foreach (var node in _rootNodes)
        {
            var treeLevel = Mathf.Max(node.GetTreeLevel() - 1, 0);

            var rt = node.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(treeLevel * Indent + LeftPadding, currentYPos);
            //rt.localPosition = new Vector3(Indent + LeftPadding, currentYPos);
            node.SaveInitPositionY();
            
            if (node.gameObject.activeInHierarchy)
            {
                currentYPos -= rt.rect.height + Spacing;
                //Debug.Log(rt.rect.height);
            }
        }

        GetComponent<RectTransform>().sizeDelta = new Vector2(300, Mathf.Abs(currentYPos - maxDistanceY));

        UpdateNodeItems();
    }

    private float maxDistanceX = 300;
    private float maxDistanceY = 25;
    private float acc;

    private Vector3 currentMousePos;

    private bool GetLockState()
    {
        bool lockState = false;
        foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
        {
            var l = node.FieldObject.GetComponent<IItemInterface>().GetLockState();
            lockState |= l;
        }

        return lockState;
    }

    private bool GetSlowDownState()
    {
        bool slowDown = false;
        foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
        {
            var l = node.FieldObject.GetComponent<IItemInterface>().GetSlowDownState();
            slowDown |= l;
        }

        return slowDown;
    }

    private bool initState = false;
    private bool _treeIsActive = true;
    
    void UpdateNodeItems()
    {
        // Do list scrolling when hovering the items
        if (Input.mousePosition.x < maxDistanceX && Input.mousePosition.x != 0)
        {
            if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - 50 && transform.localPosition.y > 0)
            {
                transform.localPosition -= new Vector3(0, acc, 0);
                acc *= 1.01f;
            }
            else if (Input.mousePosition.y > 0 && Input.mousePosition.y < 50 && transform.localPosition.y < GetComponent<RectTransform>().sizeDelta.y - Screen.height)
            {
                transform.localPosition += new Vector3(0, acc, 0);
                acc *= 1.01f;
            }
            else
            {
                acc = 8;
            }
        }

        if (!EnableLensEffect)
        {
            currentMousePos = Input.mousePosition;
       
            // Fetch the scroll offset from scroll view content (this)
            var scrollOffset = transform.localPosition.y;

            // Do the apple dock layout list style
            foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
            {
                if (Input.mousePosition.x < 300)
                {
                    // TODO: Use the real base node height here
                    var distanceY = (node.InitGlobalPositionY + scrollOffset + maxDistanceY) - 5 - currentMousePos.y;
                    //var distanceY = node.transform.position.y - node.GetComponent<RectTransform>().sizeDelta.y * 0.5f - mousePos.y;
                    distanceY = Mathf.Clamp(distanceY, -maxDistanceY, maxDistanceY);

                    var x = (Math.Abs(distanceY) / maxDistanceY);
                    var alpha = Mathf.Max(1 - x, 0.2f);
                    node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(alpha);
                }
                else
                {
                    node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0.2f);
                }

                node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                    node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);
            }

            if(_selectedNode != null) _selectedNode.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(1);
        }
        else
        {
            if (_treeIsActive && Input.GetMouseButtonDown(0) && Input.mousePosition.x < 200)
            {
                _treeIsActive = false;
                Camera.main.GetComponent<NavigateCamera>().FreezeState = true;
                currentMousePos = Input.mousePosition;

                foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
                {
                    node.FieldObject.GetComponent<RangeFieldItem>().CustomRangeSliderUi.gameObject.SetActive(true);
                }
            }

            if (initState || Input.GetMouseButtonDown(1))
            {
                _treeIsActive = true;
                Camera.main.GetComponent<NavigateCamera>().FreezeState = false || true;
                Camera.main.GetComponent<NavigateCamera>().FreezeState = false;

                // Do the apple dock layout list style
                foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
                {
                    //node.FieldObject.GetComponent<RangeFieldItem>().RangeSliderUI.gameObject.SetActive(false);
                    node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0.5f);
                    node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                        node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);

                }

                initState = false;
            }

            if (GetLockState() || _treeIsActive) return;

            if (GetSlowDownState())
            {
                currentMousePos += (Input.mousePosition - currentMousePos) * 0.005f;
            }
            else
            {
                currentMousePos += (Input.mousePosition - currentMousePos) * 0.1f;
            }


            // Fetch the scroll offset from scroll view content (this)
            var scrollOffset = transform.localPosition.y;

            // Do the apple dock layout list style
            foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
            {
                //if (mousePos.x > maxDistanceX || mousePos.x <= 0)
                //{
                //    node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                //    node.transform.localPosition = new Vector3(node.transform.localPosition.x, node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);
                //    continue;
                //}

                // TODO: Use the real base node height here
                var distanceY = (node.InitGlobalPositionY + scrollOffset) - 15 - currentMousePos.y;
                //var distanceY = node.transform.position.y - node.GetComponent<RectTransform>().sizeDelta.y * 0.5f - mousePos.y;
                distanceY = Mathf.Clamp(distanceY, -maxDistanceY, maxDistanceY);

                var x = (Math.Abs(distanceY) / maxDistanceY);
                var scale = 1 - (Math.Abs(distanceY) / maxDistanceY);
                scale = 0.5f + (0.25f * scale);

                var alpha = Mathf.Max(1 - x, 0.2f);
                node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(alpha);

                node.transform.localScale = new Vector3(scale, scale, 1);
                node.transform.localPosition = new Vector3(node.transform.localPosition.x, node.InitLocalPositionY + distanceY, node.transform.localPosition.z);
            }
        }
    }

    
}
