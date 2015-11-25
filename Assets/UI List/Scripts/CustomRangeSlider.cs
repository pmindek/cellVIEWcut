using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Internal;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomRangeSlider : MonoBehaviour 
{
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
        for(int i = 0; i < rangeValues.Count; i++)
        {
            rangeValues[i] = 1.0f / rangeValues.Count; ;
        }
        GetComponent<LayoutElement>().preferredWidth = totalLength + 10;
    }

    private int GetRangeWidth(float value)
    {
        return Mathf.RoundToInt(totalLength * value);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < rangeValues.Count; i++)
        {
            var setValue = GetRangeWidth(rangeValues[i]);
            var layout = ranges[i].GetComponent<LayoutElement>();//
            if (layout.minWidth != setValue) layout.minWidth = setValue;// = Mathf.Max(setValue, MIN_RANGE_WIDTH);

            var textUI = ranges[i].GetChild(0).GetComponent<Text>();
            var newText = (textUI.gameObject.GetComponent<RectTransform>().rect.width > 5) ? Mathf.Round(rangeValues[i] * 100.0f) + " %" : "";
            if (textUI.text != newText)
            {
                textUI.text = newText;
            }
        }

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

        var previousRangeValue = rangeValues[handleIndex];
        var nextRangeValue = rangeValues[handleIndex + 1];
        var total = previousRangeValue + nextRangeValue;

        var ratio = 100.0f * 3;
        previousRangeValue += pointerEvent.delta.x / ratio;
        previousRangeValue = Mathf.Clamp(previousRangeValue, 0.0f, total);
        nextRangeValue = total - previousRangeValue;

        rangeValues[handleIndex] = previousRangeValue;
        rangeValues[handleIndex + 1] = nextRangeValue;
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
