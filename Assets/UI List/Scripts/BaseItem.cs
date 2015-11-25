﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public class BaseItem : MonoBehaviour
{
    public bool IsFolded = true;
    public bool IsVisible = true;
    
    public string Name;
    public int Id;
    public string Path;
    public string Type;

    [SerializeField]
    public object[] Args;

    public RangeFieldItem RangeFieldItem;

    // Children ui elements
    public GameObject ArrowObject;
    public GameObject FieldObject;
    
    public TreeViewController ViewController;
    public List<BaseItem> Children = new List<BaseItem>();


    void Start()
    {
        RangeFieldItem = GetComponentInChildren<RangeFieldItem>();
    }

    private void DropDownToggleDelegate(bool value)
    {
        IsFolded = value;
        SetChildrenVisibility(!value);
        ViewController.UpdateLayout();
    }

    public void Initialize(string name, string path, string type, object[] args, bool isFolded, TreeViewController controller, int id)
	{
        Name = name;
        Path = path;
        Type = type;
        Args = args;
        IsFolded = isFolded;
        ViewController = controller;
        Id = id;

        var rt = GetComponent<RectTransform>();
        //rt.sizeDelta = new Vector2(rt.sizeDelta.x, 15);

        SetArrowSize(ViewController.ArrowSize);
        SetArrowAlpha(0.5f);
        SetArrowState(IsFolded);
        SetArrowVisibility(false);
        SetChildrenVisibility(!IsFolded);

        ArrowObject.GetComponent<ArrowScript>().DropDownToggle += DropDownToggleDelegate;

        //FieldObject = TreeUtility.InstantiateNodeField(Type);
        FieldObject = Instantiate(Resources.Load("RangeItem", typeof(GameObject))) as GameObject;
        FieldObject.GetComponent<IItemInterface>().Parameters = Args;
        FieldObject.GetComponent<IItemInterface>().SetTextFontSize(ViewController.TextFontSize);
        FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0.1f);
        FieldObject.transform.SetParent(this.transform, false);
        SetFieldObjectSize(ViewController.TextFieldSize);
    }

    //void Awake()
    //{
    //    Debug.Log("Hevlo");
    //    var arrowScript = ArrowObject.GetComponent<ArrowScript>();
    //    if (arrowScript != null)
    //    {
    //        arrowScript.DropDownToggle += DropDownToggleDelegate;
    //    }
    //}

    //private void UpdateFieldObjectSize()
    //{
    //    var arrowSize = ArrowObject.GetComponent<ArrowScript>().Size;

    //    var rt = FieldObject.GetComponent<RectTransform>();
    //    rt.pivot = new Vector2(0, 1);
    //    //rt.sizeDelta = new Vector2(arrowSize, arrowSize);
    //    //rt.sizeDelta = ArrowObject.GetComponent<RectTransform>().sizeDelta;
    //    rt.localPosition = new Vector3(arrowSize, 0, 0);

    //}

    public void AddChild(BaseItem item)
    {
        if (this.Name.Contains("capsid"))
        {
            int a = 0;
        }

        Children.Add(item);
        SetArrowVisibility(true);
    }

    public float InitGlobalPositionY;
    public float InitLocalPositionY;

    public void SaveInitPositionY()
    {
        InitGlobalPositionY = transform.position.y;
        InitLocalPositionY = transform.localPosition.y;
    }

    public float GetPositionY()
    {
        return GetComponent<RectTransform>().position.y;
    }

    public int GetTreeLevel()
    {
        return Path.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).ToList().Count;
    }

    private void SetChildrenVisibility(bool value)
    {
        foreach (var child in Children)
        {
            child.gameObject.SetActive(value);
            child.SetChildrenVisibility(value);
        }
    }

    /*****/
    
    private void SetArrowAlpha(float value)
    {
        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        arrowScript.SetAlpha(value);
    }

    public void SetArrowVisibility(bool value)
    {
        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        arrowScript.SetEnabled(value);
    }

    private void SetArrowState(bool value)
    {
        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        arrowScript.SetState(value);
    }

    private void SetArrowSize(float value)
    {
        var rt = GetComponent<RectTransform>();
        var offsetY = -rt.rect.height / 2;

        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        var arrowRt = ArrowObject.GetComponent<RectTransform>();
        arrowRt.pivot = new Vector2(.5f, .5f);
        arrowRt.localPosition = new Vector3(value / 2, offsetY, 0);
        arrowRt.sizeDelta = new Vector2(value, value);
        arrowScript.Size = value;
    }

    private void SetFieldObjectSize(float value)
    {
        var rt = GetComponent<RectTransform>();
        var offsetY = -rt.rect.height / 2;

        var arrowSize = ArrowObject.GetComponent<ArrowScript>().Size;
        var fieldRt = FieldObject.GetComponent<RectTransform>();
        offsetY += value / 2;

        fieldRt.pivot = new Vector2(0, 1);
        fieldRt.localPosition = new Vector3(arrowSize + 3, offsetY, 0);
        fieldRt.sizeDelta = new Vector2(fieldRt.sizeDelta.x, value);
    }

    public void Reset()
    {
        //throw new NotImplementedException();
    }
}
