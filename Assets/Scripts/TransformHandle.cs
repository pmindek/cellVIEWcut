using System;

using UnityEngine;
using UnityEngine.Experimental.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum HandleSelectionState
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
    private HandleSelectionState _state = HandleSelectionState.Translate;
    
    private static Vector2 _startMousePosition;
    private static Vector2 _currentMousePosition;

    // Position handle
    private static Vector3 _translateStartPosition;
    
    // Scale handle
    private Vector3 _rotateStartAxis;
    private Vector3 _rotateStartPosition;
    private Quaternion _rotateStartRotation;

    // Scale handle
    private float _scaleValueDrag;
    private float _scaleStartScale;

    //*****//

    private bool _freezeObjectPicking = false;
    public bool FreezeObjectPicking
    {
        get { return _freezeObjectPicking; }
        set{ _freezeObjectPicking = value; }
    }

    private ControlType _currentControl = ControlType.None;
    public ControlType CurrentControl
    {
        get { return _currentControl; }
    }

    private ControlType _nearestControl;

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

    public void Enable()
    {
        _enabled = true;
        _state = HandleSelectionState.Translate;
    }

    public void Disable()
    {
        _enabled = false;
    }

    public void SetSelectionState(HandleSelectionState state)
    {
        _state = state;
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

    private Vector3 GetAxis(ControlType control)
    {
        switch (control)
        {
            case ControlType.TranslateX:
                return transform.right;
            case ControlType.TranslateY:
                return transform.up;
            case ControlType.TranslateZ:
                return transform.forward;
            case ControlType.RotateX:
                return transform.right;
            case ControlType.RotateY:
                return transform.up;
            case ControlType.RotateZ:
                return transform.forward;
            case ControlType.RotateInner:
                return Camera.main.transform.forward;
            case ControlType.RotateOuter:
                return Camera.main.transform.forward;
            case ControlType.ScaleX:
                return new Vector3(-1, 0, 0);
            case ControlType.ScaleY:
                return new Vector3(0, 1, 0);
            case ControlType.ScaleZ:
                return new Vector3(0, 0, -1);
            case ControlType.ScaleCenter:
                return Vector3.one;
            case ControlType.None:
                return transform.right;
            default:
                throw new Exception("Control type error");
        }
    }

    private Color GetColor(ControlType control)
    {
        if (_currentControl == control) return MyHandleUtility.selectedColor;
        else
        {
            switch (control)
            {
                case ControlType.TranslateX:
                    return MyHandleUtility.xAxisColor;
                case ControlType.TranslateY:
                    return MyHandleUtility.yAxisColor;
                case ControlType.TranslateZ:
                    return MyHandleUtility.zAxisColor;
                case ControlType.RotateX:
                    return MyHandleUtility.xAxisColor;
                case ControlType.RotateY:
                    return MyHandleUtility.yAxisColor;
                case ControlType.RotateZ:
                    return MyHandleUtility.zAxisColor;
                case ControlType.RotateInner:
                    return MyHandleUtility.centerColor;
                case ControlType.RotateOuter:
                    return MyHandleUtility.centerColor;
                case ControlType.ScaleX:
                    return MyHandleUtility.xAxisColor;
                case ControlType.ScaleY:
                    return MyHandleUtility.yAxisColor;
                case ControlType.ScaleZ:
                    return MyHandleUtility.zAxisColor;
                case ControlType.ScaleCenter:
                    return MyHandleUtility.centerColor;
                case ControlType.None:
                    break;
                default:
                    throw new Exception("Control type error");
            }
        }

        return Color.magenta;
    }

    void OnGUI()
    {
        if (!_enabled) return;
        
        BeginHandle();

        // Do the controls
        if (Event.current.type == EventType.mouseDown && Event.current.button == 0 && !Event.current.alt)
            DoControls(); 

        // Do the handles
        if (_currentControl != ControlType.None && Event.current.type == EventType.MouseDrag && !Event.current.alt)
            DoHandles();

        EndHandle();
    }

    //*****//

    public void BeginHandle()
    {
        _handleSize = MyHandleUtility.GetHandleSize(transform.position);

        if (Event.current.type == EventType.Layout)
        {
            _nearestDistance = 5f;
            _nearestControl = ControlType.None;
        }
    }

    public void EndHandle()
    {
        //if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        //{
        //    if(!_freezeObjectPicking) _currentControl = ControlType.None;
        //}
    }

    //*****//

    private void DoControls()
    {
        switch (_state)
        {
            case HandleSelectionState.Scale:
                DoScaleControls();
                break;

            case HandleSelectionState.Rotate:
                DoRotateControls();
                break;

            case HandleSelectionState.Translate:
                DoTranslateControls();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DoHandles()
    {
        switch (_state)
        {
            case HandleSelectionState.Scale:
                DoScaleHandle();
                break;

            case HandleSelectionState.Rotate:
                DoRotateHandle();
                break;

            case HandleSelectionState.Translate:
                DoTranslateHandle();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //*****//

    private void DoTranslateControls()
    {
        AddControl(ControlType.TranslateX, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.right * _handleSize));
        AddControl(ControlType.TranslateX, MyHandleUtility.DistanceToCircle(transform.position + transform.right * _handleSize, _handleSize * 0.2f));
        AddControl(ControlType.TranslateY, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.up * _handleSize));
        AddControl(ControlType.TranslateY, MyHandleUtility.DistanceToCircle(transform.position + transform.up * _handleSize, _handleSize * 0.2f));
        AddControl(ControlType.TranslateZ, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.forward * _handleSize));
        AddControl(ControlType.TranslateZ, MyHandleUtility.DistanceToCircle(transform.position + transform.forward * _handleSize, _handleSize * 0.2f));

        _currentControl = NearestControl;
        if (_currentControl != ControlType.None)
        {
            _freezeObjectPicking = true;
            _currentControl = NearestControl;
            _translateStartPosition = transform.position;
            _currentMousePosition = _startMousePosition = Event.current.mousePosition;
        }
    }

    private void DoTranslateHandle()
    {
        _currentMousePosition += Event.current.delta;
        var translateAxis = GetAxis(_currentControl);
        var translationDist = MyHandleUtility.CalcLineTranslation(_startMousePosition, _currentMousePosition, _translateStartPosition, translateAxis);
        transform.position = _translateStartPosition + translateAxis * translationDist;
    }

    //*****//

    private void DoRotateControls()
    {
        AddControl(ControlType.RotateInner, MyHandleUtility.DistanceToDisc(transform.position, Camera.main.transform.forward, _handleSize) / 2f);
        AddControl(ControlType.RotateOuter, MyHandleUtility.DistanceToDisc(transform.position, Camera.main.transform.forward, _handleSize * 1.1f) / 2f);
        AddControl(ControlType.RotateX, MyHandleUtility.DistanceToArc(transform.position, transform.right, Vector3.Cross(transform.right, Camera.main.transform.forward).normalized, 180f, _handleSize) / 2f);
        AddControl(ControlType.RotateY, MyHandleUtility.DistanceToArc(transform.position, transform.up, Vector3.Cross(transform.up, Camera.main.transform.forward).normalized, 180f, _handleSize) / 2f);
        AddControl(ControlType.RotateZ, MyHandleUtility.DistanceToArc(transform.position, transform.forward, Vector3.Cross(transform.forward, Camera.main.transform.forward).normalized, 180f, _handleSize) / 2f);

        _currentControl = NearestControl;
        if (_currentControl != ControlType.None)
        {
            _freezeObjectPicking = true;
            _currentControl = NearestControl;
            _rotateStartAxis = GetAxis(_currentControl);
            _rotateStartRotation = transform.rotation;
            _currentMousePosition = _startMousePosition = Event.current.mousePosition;

            _rotateStartPosition = MyHandleUtility.ClosestPointToDisc(transform.position, _rotateStartAxis, _handleSize);
            //var normalized = Vector3.Cross(_rotateStartAxis, Camera.main.transform.forward).normalized;
            //_rotateStartPosition = MyHandleUtility.ClosestPointToArc(transform.position, _rotateStartAxis, normalized, 180f, _handleSize);
        }
    }

    private void DoRotateHandle()
    {
       _currentMousePosition += Event.current.delta;
        var normalized = Vector3.Cross(_rotateStartAxis, transform.position - _rotateStartPosition).normalized;
        var rotationDist = (float)((double)MyHandleUtility.CalcLineTranslation(_startMousePosition, _currentMousePosition, _rotateStartPosition, normalized) / (double)_handleSize * 30.0);
        transform.rotation = Quaternion.AngleAxis(rotationDist * -1f, _rotateStartAxis) * _rotateStartRotation;
    }

    //*****//

    private void DoScaleControls()
    {
        AddControl(ControlType.ScaleCenter, MyHandleUtility.DistanceToCircle(transform.position, _handleSize * 0.15f));
        //AddControl(ControlType.ScaleX, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.right * _handleSize));
        //AddControl(ControlType.ScaleX, MyHandleUtility.DistanceToCircle(transform.position + transform.right * _handleSize, _handleSize * 0.2f));
        //AddControl(ControlType.ScaleY, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.up * _handleSize));
        //AddControl(ControlType.ScaleY, MyHandleUtility.DistanceToCircle(transform.position + transform.up * _handleSize, _handleSize * 0.2f));
        //AddControl(ControlType.ScaleZ, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.forward * _handleSize));
        //AddControl(ControlType.ScaleZ, MyHandleUtility.DistanceToCircle(transform.position + transform.forward * _handleSize, _handleSize * 0.2f));
        
        _currentControl = NearestControl;
        if (_currentControl != ControlType.None)
        {
            _freezeObjectPicking = true;
            _currentMousePosition = _startMousePosition = Event.current.mousePosition;

            if (CurrentControl == ControlType.ScaleCenter)
            {
                _scaleValueDrag = 0.0f;
                _scaleStartScale = transform.localScale.x;
            }
        }
    }
    
    private void DoScaleHandle()
    {
        _currentMousePosition += Event.current.delta;
        var scaleAxis = GetAxis(_currentControl);

        if (CurrentControl == ControlType.ScaleCenter)
        {
            _scaleValueDrag += MyHandleUtility.niceMouseDelta * 0.01f;
            var scaleFactor = (_scaleValueDrag + 1f) * _scaleStartScale;

            var num2 = scaleFactor / transform.localScale.x;
            var newScale = transform.localScale;

            newScale.x = scaleFactor;
            newScale.y *= num2;
            newScale.z *= num2;

            transform.localScale = newScale;
        }
    }

    //*****//

    private void OnRenderObject()
    {
        if (!_enabled || Camera.current != Camera.main) return;

        _handleSize = MyHandleUtility.GetHandleSize(transform.position);

        MyHandleUtility.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh, transform, new Color(0.5f, 0.8f, 0.5f));

        switch (_state)
        {
            case HandleSelectionState.Scale:
                DrawScaleHandle();
                break;

            case HandleSelectionState.Rotate:
                DrawRotateHandle();
                break;

            case HandleSelectionState.Translate:
                DrawTranslateHandle();
                break;
        }
    }

    private void DrawTranslateHandle()
    {
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.right*_handleSize*0.9f, GetColor(ControlType.TranslateX));
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.up * _handleSize * 0.9f, GetColor(ControlType.TranslateY));
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.forward * _handleSize * 0.9f, GetColor(ControlType.TranslateZ));

        MyHandleUtility.DrawConeCap(transform.position + transform.right*_handleSize, Quaternion.LookRotation(transform.right), _handleSize*0.2f, GetColor(ControlType.TranslateX));
        MyHandleUtility.DrawConeCap(transform.position + transform.up*_handleSize, Quaternion.LookRotation(transform.up), _handleSize*0.2f, GetColor(ControlType.TranslateY));
        MyHandleUtility.DrawConeCap(transform.position + transform.forward*_handleSize, Quaternion.LookRotation(transform.forward), _handleSize*0.2f, GetColor(ControlType.TranslateZ));
    }

    private void DrawRotateHandle()
    {
        MyHandleUtility.DrawWireDisc(transform.position, Camera.current.transform.forward, _handleSize, GetColor(ControlType.RotateInner));
        MyHandleUtility.DrawWireDisc(transform.position, Camera.current.transform.forward, _handleSize*1.1f, GetColor(ControlType.RotateOuter));
        MyHandleUtility.DrawWireArc(transform.position, transform.right, Vector3.Cross(transform.right, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateX));
        MyHandleUtility.DrawWireArc(transform.position, transform.up, Vector3.Cross(transform.up, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateY));
        MyHandleUtility.DrawWireArc(transform.position, transform.forward, Vector3.Cross(transform.forward, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateZ));
    }

    private void DrawScaleHandle()
    {
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.right*_handleSize, GetColor(ControlType.ScaleX));
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.up * _handleSize, GetColor(ControlType.ScaleY));
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.forward * _handleSize, GetColor(ControlType.ScaleZ));
        
        MyHandleUtility.DrawCubeCap(transform.position, transform.rotation, _handleSize * 0.15f, GetColor(ControlType.ScaleCenter));
        MyHandleUtility.DrawCubeCap(transform.position + transform.right*_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleX));
        MyHandleUtility.DrawCubeCap(transform.position + transform.up*_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleY));
        MyHandleUtility.DrawCubeCap(transform.position + transform.forward*_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleZ));
    }
}