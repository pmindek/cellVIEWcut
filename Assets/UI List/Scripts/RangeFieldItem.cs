using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RangeFieldItem : MonoBehaviour, IItemInterface
{
    public Text TextUI;
    public RangeSlider RangeSliderUI;

    /// <summary>
    /// Parameters = new object[]{ string DisplayText }   OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle, Font font } 
    /// </summary>
    /// <value>The parameters.</value>

    public void SetRangeValues(List<float> rangeValues)
    {
        RangeSliderUI.rangeValues = rangeValues;
    }

    public List<float> GetRangeValues()
    {
        return RangeSliderUI.rangeValues;
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
        RangeSliderUI.GetComponent<CanvasGroup>().alpha = alpha;
    }

    public bool GetLockState()
    {
        //if(RangeSliderUI.LockState) Debug.Log("Lock state");
        return RangeSliderUI.LockState;
    }

    public bool GetSlowDownState()
    {
        //if (RangeSliderUI.LockState) Debug.Log("Lock state");
        return RangeSliderUI.SlowDownState;
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
