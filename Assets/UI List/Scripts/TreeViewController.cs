using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject BaseItemPrefab;
    private List<BaseItem> RootNodes;


    private CutParameters currentParameters;
    private List<int> selectedIngredients = new List<int>();
    private float lastValueChange0;
    private float lastValueChange1;
    private float lastRangeChange0;
    private float lastRangeChange1;

    private float rangeAdjust0;
    private float rangeAdjust1;

    private float lastRange0;
    private float lastRange1;

    private float lastValue1;
    private float lastValue2;

    private bool once = false;
    private BaseItem onceItem = null;

    private List<float> rangeValues = new List<float>();
    private List<float> nextRangeValues = new List<float>();


    public bool enableAppleEffect = false;

    // Use this for initialization
    void Start()
    {
        if (RootNodes == null) RootNodes = new List<BaseItem>();
        foreach (var node in PersistantSettings.Instance.hierachy)
        {
            AddNodeObject(node.path, new object[] { node.name }, "Text");
        }
        Init();
    }

    public void UpdateRangeValues()
    {
        if(RootNodes != null)
        foreach (var Node in RootNodes)
        {
            RangeFieldItem range = Node.FieldObject.GetComponent<RangeFieldItem>();












            //not dragging it
            //else
            if (!range.CustomRangeSliderUi.DragState || range.CustomRangeSliderUi.recalcOnce)
            {
                HistStruct hist = SceneManager.Instance.histograms[Node.Id];

                List<float> rangeValues = new List<float>();
                rangeValues.Clear();

                rangeValues.Add((float)hist.occluding / (float)hist.all);
                rangeValues.Add(1.0f - (float)hist.cutaway / (float)hist.all - rangeValues[0]);
                rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);

                Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);

                
                
                range.CustomRangeSliderUi.recalcOnce = false;
                range.CustomRangeSliderUi.disableDragging = false;
                range.CustomRangeSliderUi.StartedDragging = true;
            }






            //if dragging the RangeSlider
            if (range.CustomRangeSliderUi.DragState)
            {
                //initialize the dragging - average all the parameters for all dragged sliders and all selected cut objects
                if (range.CustomRangeSliderUi.StartedDragging)
                {
                    range.CustomRangeSliderUi.StartedDragging = false;

                    BaseItem baseItem = FindBaseItem(Node.Path);
                    List<BaseItem> children = baseItem.GetAllChildren();

                    children.Add(baseItem);

                    selectedIngredients.Clear();

                    foreach (var child in children)
                    {
                        if (child.Children.Count == 0)
                        {
                            Debug.Log("|~!!~ " + SceneManager.Instance.HistogramsReverseLookup[child.Id]);
                            selectedIngredients.Add(SceneManager.Instance.HistogramsReverseLookup[child.Id]);
                        }
                    }



                    currentParameters = new CutParameters()
                    {
                        range0 = 0.0f,
                        range1 = 0.0f,

                        countAll = 0,
                        count0 = 0,
                        count1 = 0,

                        value1 = 0.0f,
                        value2 = 0.0f,
                        fuzziness = 0.0f,
                        fuzzinessDistance = 0.0f,
                        fuzzinessCurve = 0.0f
                    };

                    CutParameters param = null;

                    float fc = 0;

                    foreach (var cut in SceneManager.Instance.CutObjects)
                    {
                        for (int i = 0; i < selectedIngredients.Count; i++)
                        {
                            param = cut.GetCutParametersFor(selectedIngredients[i]);

                            currentParameters.range0 += param.range0;
                            currentParameters.range1 += param.range1;

                            currentParameters.countAll += param.countAll;
                            currentParameters.count0 += param.count0;
                            currentParameters.count1 += param.count1;

                            currentParameters.value1 += param.value1;
                            currentParameters.value2 += param.value2;
                            currentParameters.fuzziness += param.fuzziness;
                            currentParameters.fuzzinessDistance += param.fuzzinessDistance;
                            currentParameters.fuzzinessCurve += param.fuzzinessCurve;
                        }
                    }

                    fc = (float)(selectedIngredients.Count * SceneManager.Instance.CutObjects.Count);

                    currentParameters.range0 /= fc;
                    currentParameters.range1 /= fc;

                    currentParameters.value1 /= fc;
                    currentParameters.value2 /= fc;
                    currentParameters.fuzziness /= fc;
                    currentParameters.fuzzinessDistance /= fc;
                    currentParameters.fuzzinessCurve /= fc;

                    Debug.Log("value1 = " + currentParameters.value1);
                    Debug.Log("value2 = " + currentParameters.value2);
                    Debug.Log("fuzziness = " + currentParameters.fuzziness);
                    Debug.Log("distance = " + currentParameters.fuzzinessDistance);
                    Debug.Log("curve = " + currentParameters.fuzzinessCurve);


                    /* lastValueChange0 = Mathf.Abs(lastValue1 - currentParameters.value1);
                     lastValueChange1 = Mathf.Abs(lastValue2 - currentParameters.value2);

                     lastRangeChange0 = Mathf.Abs(lastRange0 - rangeValues[0]);
                     lastRangeChange1 = Mathf.Abs(lastRange1 - rangeValues[1]);*/

                    rangeValues = Node.RangeFieldItem.GetRangeValues();

                    lastRange0 = rangeValues[0];
                    lastRange1 = rangeValues[1];

                    lastValue1 = currentParameters.value1;
                    lastValue2 = currentParameters.value2;
                } //end of initialization


                //rangeValues = Node.RangeFieldItem.GetRangeValues();

                /*rangeValues.Clear();
                rangeValues.Add(currentParameters.range0);
                rangeValues.Add(currentParameters.range1);
                rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);*/




                //this happens on every frame while dragging




                //HistStruct hist = SceneManager.Instance.histograms[Node.Id];

                /*rangeValues.Clear();

                rangeValues.Add((float)hist.occluding / (float)hist.all);
                rangeValues.Add(1.0f - (float)hist.cutaway / (float)hist.all - rangeValues[0]);
                rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);*/

                //Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);











                float d0 = 0.0f;
                float d1 = 0.0f;

                if (lastRange0 > rangeValues[0])
                {
                    d0 = -1.0f + rangeValues[0] / lastRange0;
                }
                else if (lastRange0 < rangeValues[0])
                {
                    d0 = (rangeValues[0] - lastRange0) / (lastRange1 - lastRange0);
                }

                if (lastRange1 > rangeValues[1])
                {
                    d1 = -1.0f + rangeValues[1] / lastRange1;
                }
                else if (lastRange1 < rangeValues[1])
                {
                    d1 = (rangeValues[1] - lastRange1) / (1 - lastRange1 - lastRange0);
                }

                if (d0 < -1.0f)
                    d0 = -1.0f;
                if (d0 > 1.0f)
                    d0 = 1.0f;
                if (d1 < -1.0f)
                    d1 = -1.0f;
                if (d1 > 1.0f)
                    d1 = 1.0f;

                if (Mathf.Abs(d0) > 0.001f)
                    d1 = 0.0f;

                float adjust0 = 1.0f;
                float adjust1 = 1.0f;

                float v1 = lastValue1 + d1 * adjust1;
                float v2 = lastValue2 + d0 * adjust0;

                if (v1 < 0.0f)
                    v1 = 0.0f;
                if (v1 > 1.0f)
                    v1 = 1.0f;
                if (v2 < 0.0f)
                    v2 = 0.0f;
                if (v2 > 1.0f)
                    v2 = 1.0f;


                currentParameters.value1 = v1;
                currentParameters.value2 = v2;




                HistStruct hist = SceneManager.Instance.histograms[Node.Id];

                currentParameters.countAll = hist.all;
                currentParameters.count0 = hist.cutaway;
                currentParameters.count1 = hist.occluding;

                float st_cutaway = (float)currentParameters.count0;
                float st_all = (float)currentParameters.countAll;
                float st_occluding = (float)currentParameters.count1;


                List<float> rv = new List<float>();
                rv.Clear();

                rv.Add((float)hist.occluding / (float)hist.all);
                rv.Add(1.0f - (float)hist.cutaway / (float)hist.all - rv[0]);
                rv.Add(1.0f - rv[0] - rv[1]);





                currentParameters.range0 = rangeValues[0];
                currentParameters.range1 = rangeValues[1];


                //now we set these values to every cut object's record for the manipulated protein types
                foreach (var cut in SceneManager.Instance.CutObjects)
                {
                    for (int i = 0; i < selectedIngredients.Count; i++)
                    {
                        cut.SetCutParametersFor(selectedIngredients[i], currentParameters);
                    }
                }


                //Debug.Log("is " + rangeValues[0] + "; want " + rv[0]);
                //Debug.Log("is " + rangeValues[1] + "; want " + rv[1]);
                Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);
                Node.FieldObject.GetComponent<RangeFieldItem>().SetFakeRangeValues(rv);

                //nextRangeValues = rv;

                //range.CustomRangeSliderUi.disableDragging = true;
                //range.CustomRangeSliderUi.recalcOnce = true;
            }
        }        
    }

    public void OnItemToggle(BaseEventData eventData)
    { 

    }

    public void LogRangeValues()
    {
        foreach (var Node in RootNodes)
        {
            Node.FieldObject.GetComponent<RangeFieldItem>().GetRangeValues();
            //Node.Name
        }
    }

    public BaseItem FindBaseItem(string path)
    {
        return RootNodes.FirstOrDefault(n => n.Path == path);
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
            RootNodes.Add(node);
        }
        // If the node is a child node
	    else
        {
            var parentNode = FindBaseItem(parentPath);

            if (parentNode != null)
            {
                var node = CreateNodeObject(name, fullPath, args, type);
                RootNodes.Add(node);
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
        node.Initialize(name, fullPath, type, args, false, this, SceneManager.Instance.GetProteinId(name));
        
        return node;
    }

    public void Init()
    {
        initState = true;
        UpdateLayout();
    }

    // Reorder the tree elements
    public void UpdateLayout()
    {
        // TODO: Use the real base node height here
        float currentYPos = - (maxDistanceY + 10); 

        foreach (var node in RootNodes)
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

        Update();
    }

    private float maxDistanceX = 300;
    private float maxDistanceY = 25;
    private float acc;

    private Vector3 currentMousePos;

    private bool GetLockState()
    {
        bool lockState = false;
        foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
        {
            var l = node.FieldObject.GetComponent<IItemInterface>().GetLockState();
            lockState |= l;
        }

        return lockState;
    }

    private bool GetSlowDownState()
    {
        bool slowDown = false;
        foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
        {
            var l = node.FieldObject.GetComponent<IItemInterface>().GetSlowDownState();
            slowDown |= l;
        }

        return slowDown;
    }

    private bool initState = false;
    private bool _treeIsActive = true;

    void Update()
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

        if (!enableAppleEffect)
        {
            

            currentMousePos = Input.mousePosition;
       
            // Fetch the scroll offset from scroll view content (this)
            var scrollOffset = transform.localPosition.y;

            // Do the apple dock layout list style
            foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
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
        }
        else
        {
            if (_treeIsActive && Input.GetMouseButtonDown(0) && Input.mousePosition.x < 200)
            {
                _treeIsActive = false;
                Camera.main.GetComponent<NavigateCamera>().FreezeState = true;
                currentMousePos = Input.mousePosition;

                foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
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
                foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
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
            foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
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

    public void OnToggleItem1(BaseItem baseItem)
    {
        var value = baseItem.RangeFieldItem.Toggle1.isOn;

        foreach (var child in baseItem.Children)
        {
            child.RangeFieldItem.SetToggle1(value);
        }
    }

    public void OnToggleItem2(BaseItem baseItem)
    {
        var value = baseItem.RangeFieldItem.Toggle2.isOn;

        foreach (var child in baseItem.Children)
        {
            child.RangeFieldItem.SetToggle2(value);
        }
    }

    public List<bool> GetAllToggleItems1()
    {
        return RootNodes.Select(node => node.RangeFieldItem.Toggle1.isOn).ToList();
    }

    public List<bool> GetAllToggleItems2()
    {
        return RootNodes.Select(node => node.RangeFieldItem.Toggle2.isOn).ToList();
    }
}
