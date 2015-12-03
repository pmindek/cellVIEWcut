using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RangeFieldItem : MonoBehaviour, IItemInterface
{
    public Text TextUI;
    public CustomToggle Toggle;
    public GameObject VisibilityToggle;
    public LockToggle LockToggle;
    public ThreeStateToggle ThreeStateToggle;
    public CustomRangeSlider CustomRangeSliderUi;

    /// <summary>
    /// Parameters = new object[]{ string DisplayText }   OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle, Font font } 
    /// </summary>
    /// <value>The parameters.</value>

    private BaseItem baseItem;
    
    public void Start()
    {
        baseItem = transform.parent.GetComponent<BaseItem>();
        //Toggle.isOn = false;
        //LockToggle.gameObject.SetActive(false);

        LockToggle.LockToggleEvent += OnLockToggleClick;
        Toggle.CustomToggleClickEvent += OnCustomToggleClick;
        ThreeStateToggle.ThreeStateToggleClickEvent += OnThreeStateToggleClick;
    }

    public List<float> GetRangeValues()
    {
        return CustomRangeSliderUi.rangeValues;
    }

    public void SetRangeValues(List<float> rangeValues)
    {
        /*CustomRangeSliderUi.rangeValues.Clear();
        CustomRangeSliderUi.rangeValues.AddRange(rangeValues);*/
        for (int i = 0; i < rangeValues.Count; i++)
        {
            if (CustomRangeSliderUi.rangeValues.Count > i)
                CustomRangeSliderUi.rangeValues[i] = rangeValues[i];
        }

        if (CustomRangeSliderUi.rangeValues.Count >= 3)
            CustomRangeSliderUi.rangeValues[2] = 1.0f - CustomRangeSliderUi.rangeValues[0] - CustomRangeSliderUi.rangeValues[1];
    }

    //public void SetFakeRangeValues(List<float> fakeRangeValues)
    //{
    //    /*CustomRangeSliderUi.rangeValues.Clear();
    //    CustomRangeSliderUi.rangeValues.AddRange(rangeValues);*/
    //    for (int i = 0; i < fakeRangeValues.Count; i++)
    //    {
    //        if (CustomRangeSliderUi.fakeRangeValues.Count > i)
    //            CustomRangeSliderUi.fakeRangeValues[i] = fakeRangeValues[i];
    //    }

    //    if (CustomRangeSliderUi.fakeRangeValues.Count >= 3)
    //        CustomRangeSliderUi.fakeRangeValues[2] = 1.0f - CustomRangeSliderUi.fakeRangeValues[0] - CustomRangeSliderUi.fakeRangeValues[1];

    //    CustomRangeSliderUi.useFakeRangeValues = true;
    //}

    //******* Event Callbacks *********//

    public void OnLockToggleClick(bool value)
    {
        baseItem.ViewController.SetAllLockState(value);
    }

    public void OnCustomToggleClick(bool value)
    {
        baseItem.ViewController.OnFocusToggleClick(baseItem);
    }

    public void OnThreeStateToggleClick(ThreeStateToggleState value)
    {
        baseItem.ViewController.OnThreeStateToggleClick(baseItem);
    }

    public object[] Parameters
    {
        get
        {
            return GetVals();
        }
        set
        {
            SetVals(value);
        }
    }
    
    public void SetTextFontSize(int fontSize)
    {
        TextUI.fontSize = fontSize;
    }

    public void SetContentAlpha(float alpha)
    {
        CustomRangeSliderUi.GetComponent<CanvasGroup>().alpha = alpha;
    }

    public bool GetLockState()
    {
        //if(RangeSliderUI.LockState) Debug.Log("Lock state");
        return CustomRangeSliderUi.LockState;
    }

    public bool GetSlowDownState()
    {
        //if (RangeSliderUI.LockState) Debug.Log("Lock state");
        return CustomRangeSliderUi.SlowDownState;
    }

    private object[] GetVals()
    {
        return new object[] { TextUI.text, TextUI.color, TextUI.fontSize, TextUI.fontStyle, TextUI.font };
    }

    private void SetVals(object[] Vals)
    {
        if (Vals.Length <= 5)
        {
            bool good = true;
            for (int i = 0; i < Vals.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        if (!(Vals[i] is string))
                        {
                            good = false;
                        }
                        break;
                    case 1:
                        if (!((Vals[i] is Color) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    case 2:
                        if (!((Vals[i] is int) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    case 3:
                        if (!((Vals[i] is FontStyle) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    case 4:
                        if (!((Vals[i] is Font) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (good)
            {
                for (int i = 0; i < Vals.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            TextUI.text = (string)Vals[i];
                            break;
                        case 1:
                            if (Vals[i] != null)
                            {
                                TextUI.color = (Color)Vals[i];
                            }
                            break;
                        case 2:
                            if (Vals[i] != null)
                            {
                                TextUI.fontSize = (int)Vals[i];
                            }
                            break;
                        case 3:
                            if (Vals[i] != null)
                            {
                                TextUI.fontStyle = (FontStyle)Vals[i];
                            }
                            break;
                        case 4:
                            if (Vals[i] != null)
                            {
                                TextUI.font = (Font)Vals[i];
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
