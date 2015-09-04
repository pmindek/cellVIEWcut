using System;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionState
{
    Translate = 1,           // Instance will not be displayed
    Rotate = 2,          // Instance will be displayed with normal color
    Scale = 3      // Instance will be displayed with highlighted color
}

public enum ControlType
{
    None = -1,
    TranslateX = 0,        
    TranslateY = 1,         
    TranslateZ = 2,
    RotateX = 3,
    RotateY = 4,
    RotateZ = 5,
    RotateInner = 6,
    RotateOuter = 7,
    ScaleX = 8,
    ScaleY = 9,
    ScaleZ = 10,
    ScaleCenter = 11,
}

[ExecuteInEditMode]
public class TransformHandle : MonoBehaviour
{
    private bool _enabled;
    private float _handleSize;
    private float _nearestDistance;
    private float _customPickDistance = 5f;
    private SelectionState _state = SelectionState.Translate;

    private ControlType _nearestControl;
    private ControlType _currentControl = ControlType.None;

    //*****//

    public void Enable()
    {
        _enabled = true;
    }

    public void Disable()
    {
        _enabled = false;
    }

    public void SetSelectionState(SelectionState state)
    {
        _state = state;
    }

    public ControlType NearestControl
    {
        get
        {
            if ((double)_nearestDistance <= 5.0)
                return _nearestControl;

            return ControlType.None;
        }
        set
        {
            _nearestControl = value;
        }
    }

    public void BeginHandle()
    {
        if (Event.current.type == EventType.Layout)
        {
            _nearestDistance = 5f;
            _nearestControl = ControlType.None;
        }
    }
    
    public void AddControl(ControlType type, float distance)
    {
        if ((double)distance < (double)_customPickDistance && (double)distance > 5.0)
            distance = 5f;

        if ((double)distance > (double)_nearestDistance)
            return;

        _nearestControl = type;
        _nearestDistance = distance;
    }

    //*****//

    void OnGUI()
    {
        if (!_enabled || Camera.current == null) return;

        _handleSize = MyHandleUtility.GetHandleSize(transform.position);

        BeginHandle();

        switch (_state)
        {
            case SelectionState.Scale:
                DoScaleHandle();
                break;

            case SelectionState.Rotate:
                DoRotateHandle();
                break;

            case SelectionState.Translate:
                DoTranslateHandle();
                break;
        }
    }

    void DoTranslateHandle()
    {
        if (Event.current.type == EventType.Layout)
        {
            AddControl(ControlType.TranslateX, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.right * _handleSize));
            AddControl(ControlType.TranslateX, MyHandleUtility.DistanceToCircle(transform.position + transform.right * _handleSize, _handleSize * 0.2f));
            AddControl(ControlType.TranslateY, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.up * _handleSize));
            AddControl(ControlType.TranslateY, MyHandleUtility.DistanceToCircle(transform.position + transform.up * _handleSize, _handleSize * 0.2f));
            AddControl(ControlType.TranslateZ, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.forward * _handleSize));
            AddControl(ControlType.TranslateZ, MyHandleUtility.DistanceToCircle(transform.position + transform.forward * _handleSize, _handleSize * 0.2f));
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            _currentControl = NearestControl;
        }
    }

    void DoRotateHandle()
    {
        if (Event.current.type == EventType.Layout)
        {
            AddControl(ControlType.RotateInner, MyHandleUtility.DistanceToDisc(transform.position, Camera.current.transform.forward, _handleSize) / 2f);
            AddControl(ControlType.RotateOuter, MyHandleUtility.DistanceToDisc(transform.position, Camera.current.transform.forward, _handleSize * 1.1f) / 2f);
            AddControl(ControlType.RotateX, MyHandleUtility.DistanceToArc(transform.position, transform.right, Vector3.Cross(transform.right, Camera.current.transform.forward).normalized, 180f, _handleSize) / 2f);
            AddControl(ControlType.RotateY, MyHandleUtility.DistanceToArc(transform.position, transform.up, Vector3.Cross(transform.up, Camera.current.transform.forward).normalized, 180f, _handleSize) / 2f);
            AddControl(ControlType.RotateZ, MyHandleUtility.DistanceToArc(transform.position, transform.forward, Vector3.Cross(transform.forward, Camera.current.transform.forward).normalized, 180f, _handleSize) / 2f);
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            _currentControl = NearestControl;
        }
    }

    void DoScaleHandle()
    {
        if (Event.current.type == EventType.Layout)
        {
            var up = transform.rotation * Vector3.up;
            var right = transform.rotation * Vector3.right;
            var forward = transform.rotation * Vector3.forward;

            AddControl(ControlType.ScaleCenter, MyHandleUtility.DistanceToCircle(transform.position, _handleSize * 0.15f));
            AddControl(ControlType.ScaleX, MyHandleUtility.DistanceToLine(transform.position, transform.position + right * _handleSize));
            AddControl(ControlType.ScaleX, MyHandleUtility.DistanceToCircle(transform.position + right * _handleSize, _handleSize * 0.2f));
            AddControl(ControlType.ScaleY, MyHandleUtility.DistanceToLine(transform.position, transform.position + up * _handleSize));
            AddControl(ControlType.ScaleY, MyHandleUtility.DistanceToCircle(transform.position + up * _handleSize, _handleSize * 0.2f));
            AddControl(ControlType.ScaleZ, MyHandleUtility.DistanceToLine(transform.position, transform.position + forward * _handleSize));
            AddControl(ControlType.ScaleZ, MyHandleUtility.DistanceToCircle(transform.position + forward * _handleSize, _handleSize * 0.2f));
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            _currentControl = NearestControl;
        }
    }

    private Color GetColor(ControlType control)
    {
        if (_currentControl == control) return MyHandleUtility.selectedColor;
        else
        {
            switch (control)
            {
                case ControlType.TranslateX: return MyHandleUtility.xAxisColor;
                case ControlType.TranslateY: return MyHandleUtility.yAxisColor;
                case ControlType.TranslateZ: return MyHandleUtility.zAxisColor;
                case ControlType.RotateX: return MyHandleUtility.xAxisColor;
                case ControlType.RotateY: return MyHandleUtility.yAxisColor;
                case ControlType.RotateZ: return MyHandleUtility.zAxisColor;
                case ControlType.RotateInner: return MyHandleUtility.centerColor;
                case ControlType.RotateOuter: return MyHandleUtility.centerColor;
                case ControlType.ScaleX: return MyHandleUtility.xAxisColor;
                case ControlType.ScaleY: return MyHandleUtility.yAxisColor;
                case ControlType.ScaleZ: return MyHandleUtility.zAxisColor;
                case ControlType.ScaleCenter: return MyHandleUtility.centerColor;
                case ControlType.None:
                    break;
                default:
                    throw new Exception("Control type error");
            }
        }

        return Color.magenta;
    }

    private void OnRenderObject()
    {
        if (!_enabled) return;

        _handleSize = MyHandleUtility.GetHandleSize(transform.position);

        switch (_state)
        {
            case SelectionState.Scale:
                DrawScaleHandle();
                break;

            case SelectionState.Rotate:
                DrawRotationHandle();
                break;

            case SelectionState.Translate:
                DrawTranslateHandle();
                break;
        }

        DrawOutline();
    }

    private void DrawTranslateHandle()
    {
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.right * _handleSize * 0.9f, GetColor(ControlType.TranslateX));
        MyHandleUtility.DrawConeCap(transform.position + transform.right * _handleSize, Quaternion.LookRotation(transform.right), _handleSize*0.2f, GetColor(ControlType.TranslateX));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.up * _handleSize*0.9f, GetColor(ControlType.TranslateY));
        MyHandleUtility.DrawConeCap(transform.position + transform.up * _handleSize, Quaternion.LookRotation(transform.up), _handleSize*0.2f, GetColor(ControlType.TranslateY));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.forward * _handleSize*0.9f, GetColor(ControlType.TranslateZ));
        MyHandleUtility.DrawConeCap(transform.position + transform.forward * _handleSize, Quaternion.LookRotation(transform.forward), _handleSize * 0.2f, GetColor(ControlType.TranslateZ));
    }

    private void DrawRotationHandle()
    {
        MyHandleUtility.DrawWireDisc(transform.position, Camera.current.transform.forward, _handleSize, GetColor(ControlType.RotateInner));
        MyHandleUtility.DrawWireDisc(transform.position, Camera.current.transform.forward, _handleSize*1.1f, GetColor(ControlType.RotateOuter));
        MyHandleUtility.DrawWireArc(transform.position, transform.right, Vector3.Cross(transform.right, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateX));
        MyHandleUtility.DrawWireArc(transform.position, transform.up, Vector3.Cross(transform.up, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateY));
        MyHandleUtility.DrawWireArc(transform.position, transform.forward, Vector3.Cross(transform.forward, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateZ));
    }

    private void DrawScaleHandle()
    {
        MyHandleUtility.DrawCubeCap(transform.position, transform.rotation, _handleSize*0.15f, GetColor(ControlType.ScaleCenter));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.right *_handleSize, GetColor(ControlType.ScaleX));
        MyHandleUtility.DrawCubeCap(transform.position + transform.right *_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleX));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.up *_handleSize, GetColor(ControlType.ScaleY));
        MyHandleUtility.DrawCubeCap(transform.position + transform.up *_handleSize, transform.rotation, _handleSize * 0.1f, GetColor(ControlType.ScaleY));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.forward *_handleSize, GetColor(ControlType.ScaleZ));
        MyHandleUtility.DrawCubeCap(transform.position + transform.forward *_handleSize, transform.rotation, _handleSize * 0.1f, GetColor(ControlType.ScaleZ));
    }

    private void DrawOutline()
    {
        //MyHandleUtility.DrawWireCube(transform, GetComponent<Collider>().bounds, Color.green);
    }
}