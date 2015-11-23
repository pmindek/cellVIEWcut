using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TreeViewController : MonoBehaviour
{
    public int TextFontSize;

    public float ArrowSize;
    public float TextFieldSize;

    public float Indent;
    public float Spacing;
    public float TopPadding;
    public float LeftPadding;

    public GameObject BaseItemPrefab;
    public List<BaseItem> RootNodes = new List<BaseItem>();



    public void UpdateRangeValues()
    {
        foreach (var Node in RootNodes)
        {
            List<float> rangeValues = new List<float>();

            //Debug.Log("nodeid = " + Node.Name);
            HistStruct hist = SceneManager.Instance.histograms[Node.Id];

            rangeValues.Add((float) hist.occluding / (float) hist.all);
            rangeValues.Add(1.0f - (float) hist.cutaway / (float) hist.all - rangeValues[0]);
            rangeValues.Add(1.0f - rangeValues[0] - rangeValues[1]);

            Node.FieldObject.GetComponent<RangeFieldItem>().SetRangeValues(rangeValues);
        }        
    }


    public void LogRangeValues()
    {
        foreach (var Node in RootNodes)
        {
            Node.FieldObject.GetComponent<RangeFieldItem>().getRangeValues();
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
    private bool lockState = true;

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

        if (lockState && Input.GetMouseButtonDown(0) && Input.mousePosition.x < 200)
        {
            lockState = false;
            Camera.main.GetComponent<NavigateCamera>().FreezeState = true;
            currentMousePos = Input.mousePosition;
        }

        if (initState || Input.GetMouseButtonDown(1))
        {
            lockState = true;
            Camera.main.GetComponent<NavigateCamera>().FreezeState = false;

            // Do the apple dock layout list style
            foreach (var node in RootNodes.Where(node => node.gameObject.activeInHierarchy))
            {
                node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0);
                node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                    node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);

            }

            initState = false;
        }

        if (GetLockState() || lockState) return;


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

            var x = (Math.Abs(distanceY)/maxDistanceY);
            var scale = 1 - (Math.Abs(distanceY) / maxDistanceY);
            scale = 0.5f + (0.25f * scale);

            var alpha = Mathf.Max(1-x, 0.2f);
            node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(alpha);

            node.transform.localScale = new Vector3(scale, scale, 1);
            node.transform.localPosition = new Vector3(node.transform.localPosition.x, node.InitLocalPositionY + distanceY, node.transform.localPosition.z);
        }
    }
}
