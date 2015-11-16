using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Internal;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RangeSlider : MonoBehaviour 
{
    private const int MIN_RANGE_WIDTH = 2;

    public Texture2D cursor;
    public int totalLength = 200;

    public List<RectTransform> ranges;
    public List<RectTransform> handles;

    public bool LockState = false;
    public bool SlowDownState = false;
    public bool DragState = false;

    [HideInInspector] public List<float> rangeValues = new List<float> {0, 0, 0};

    // Use this for initialization
    private void Start()
    {
        foreach (var range in ranges)
        {
            var initValue = GetRangeWidth(0.33333f);
            range.GetComponent<LayoutElement>().minWidth = initValue;
        }

        GetComponent<LayoutElement>().preferredWidth = totalLength + 10;

        UpdateRangeValues();
        UpdateText(true);
    }

    private int GetRangeWidth(float value)
    {
        return (int)((float)totalLength * value);
    }

    private void UpdateRangeValues()
    {
        var nullRangeCount = 0;
        for (int i = 0; i < ranges.Count; i++)
        {
            if (ranges[i].GetComponent<LayoutElement>().minWidth == MIN_RANGE_WIDTH)
            {
                rangeValues[i] = 0;
                nullRangeCount ++;
            }
            else
            {
                rangeValues[i] = -1;
            }
        }

        for (int i = 0; i < ranges.Count; i++)
        {
            if (rangeValues[i] == -1)
            {
                var rangeWidth = ranges[i].GetComponent<LayoutElement>().minWidth;
                rangeWidth += nullRangeCount*0.5f*MIN_RANGE_WIDTH;
                rangeValues[i] = rangeWidth/(float)totalLength;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<CanvasGroup>().alpha < 0.9f)
        {
            SlowDownState = false;
        }

        if (GetComponent<CanvasGroup>().alpha < 0.8f)
        {
            LockState = false;
        }
        //if(LockState) Debug.Log("LockState");
    }

    public void OnDrag(BaseEventData eventData)
    {
        LockState = true;
        DragState = true;

        Cursor.SetCursor(cursor, new Vector2(14, 14), CursorMode.Auto);

        var pointerEvent = (PointerEventData) eventData;
        var gameObject = pointerEvent.pointerDrag;

        var handleIndex = handles.IndexOf(gameObject.GetComponent<RectTransform>());

        var previousRange = ranges[handleIndex];
        var nextRange = ranges[handleIndex + 1];

        var previousLayoutElement = previousRange.GetComponent<LayoutElement>();
        var nextLayoutElement = nextRange.GetComponent<LayoutElement>();

        if (previousLayoutElement.minWidth + pointerEvent.delta.x >= MIN_RANGE_WIDTH && nextLayoutElement.minWidth - pointerEvent.delta.x > MIN_RANGE_WIDTH)
        {
            previousLayoutElement.minWidth += pointerEvent.delta.x;
            nextLayoutElement.minWidth -= pointerEvent.delta.x;
        }
        else if (previousLayoutElement.minWidth + pointerEvent.delta.x < MIN_RANGE_WIDTH)
        {
            var delta = previousLayoutElement.minWidth - MIN_RANGE_WIDTH;

            previousLayoutElement.minWidth -= delta;
            nextLayoutElement.minWidth += delta;
        }
        else if (nextLayoutElement.minWidth - pointerEvent.delta.x < MIN_RANGE_WIDTH)
        {
            var delta = nextLayoutElement.minWidth - MIN_RANGE_WIDTH;

            previousLayoutElement.minWidth += delta;
            nextLayoutElement.minWidth -= delta;
        }

        UpdateRangeValues();
        UpdateText();
    }

    public void UpdateText(bool forceText = false)
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            var textUI = ranges[i].GetChild(0).GetComponent<Text>();
            textUI.text = Mathf.Round(rangeValues[i] * 100.0f) + " %";
            if (forceText == false && textUI.gameObject.GetComponent<RectTransform>().rect.width < 25)
            {
                textUI.text = "";
            }
        }
    }

    public void OnEnter()
    {
        LockState = true;
        Cursor.SetCursor(cursor, new Vector2(14,14), CursorMode.Auto);
    }

    public void OnExit()
    {
        if (!DragState)
        {
            LockState = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerUp()
    {
        DragState = false;
        LockState = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerDown()
    {
        DragState = true;
        LockState = true;
        Cursor.SetCursor(cursor, new Vector2(14, 14), CursorMode.Auto);
    }

    public void OnDragExit()
    {
        DragState = false;
        LockState = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerEnter()
    {
        SlowDownState = true;
    }

    public void OnPointerExit()
    {
        SlowDownState = false;
    }
}
