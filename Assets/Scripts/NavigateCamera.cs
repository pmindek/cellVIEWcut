using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NavigateCamera : MonoBehaviour
{
    private HandleSelectionState _currentState = HandleSelectionState.Translate;
    private TransformHandle _selectedTransformHandle;

    public float DefaultDistance = 5.0f;

    public float AcrBallRotationSpeed = 0.25f;
    public float FpsRotationSpeed = 0.25f;
    public float TranslationSpeed = 20.0f;
    public float ScrollingSpeed = 2.0f;
    public float PannigSpeed = 0.25f;

    public Vector3 TargetPosition;
    private GameObject TargetGameObject;

    /*****/

    private bool forward;
    private bool backward;
    private bool right;
    private bool left;

    [HideInInspector]
    public float Distance;

    [HideInInspector]
    public float EulerAngleX;

    [HideInInspector]
    public float EulerAngleY;

    private float deltaTime = 0;
    private float lastUpdateTime = 0;
    private float leftClickTimeStart = 0;

    /*****/

    void OnEnable()
    {
        #if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update += Update;
        }
        #endif
    }

    void Update()
    {
        deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
        lastUpdateTime = Time.realtimeSinceStartup;
        
        if (forward)
        {
            TargetPosition += gameObject.transform.forward * TranslationSpeed * deltaTime; 
            transform.position += gameObject.transform.forward * TranslationSpeed * deltaTime; 
        }

        if (backward)
        {
            TargetPosition -= gameObject.transform.forward * TranslationSpeed * deltaTime;
            transform.position -= gameObject.transform.forward * TranslationSpeed * deltaTime; 
        }

        if (right)
        {
            TargetPosition += gameObject.transform.right * TranslationSpeed * deltaTime;
            transform.position += gameObject.transform.right * TranslationSpeed * deltaTime; 
        }

        if (left)
        {
            TargetPosition -= gameObject.transform.right * TranslationSpeed * deltaTime;
            transform.position -= gameObject.transform.right * TranslationSpeed * deltaTime; 
        }
    }

    void DoArcBallRotation()
    {
        EulerAngleX += Event.current.delta.x * AcrBallRotationSpeed;
        EulerAngleY += Event.current.delta.y * AcrBallRotationSpeed;

        var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);
        var position = TargetPosition + rotation * Vector3.back * Distance;

        transform.rotation = rotation;
        transform.position = position;
    }

    void DoFpsRotation()
    {
        EulerAngleX += Event.current.delta.x * FpsRotationSpeed;
        EulerAngleY += Event.current.delta.y * FpsRotationSpeed;

        var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);

        transform.rotation = rotation;
        TargetPosition = transform.position + transform.forward * Distance;
    }

    void DoPanning()
    {
        TargetPosition += transform.up * Event.current.delta.y * PannigSpeed;
        transform.position += transform.up * Event.current.delta.y * PannigSpeed;

        TargetPosition -= transform.right * Event.current.delta.x * PannigSpeed;
        transform.position -= transform.right * Event.current.delta.x * PannigSpeed;
    }

    
    void DoScrolling()
    {
        Distance += Event.current.delta.y* ScrollingSpeed;
        transform.position = TargetPosition - transform.forward* Distance;

        if (Distance< 0)
        {
            TargetPosition = transform.position + transform.forward * DefaultDistance;
            Distance = Vector3.Distance(TargetPosition, transform.position);
        }
    }


    //bool TestLeftMouseClick()
    //{
    //    var leftClick = false;

    //    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
    //    {
    //        leftClickTimeStart = Time.realtimeSinceStartup;
    //    }

    //    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
    //    {
    //        leftClickTimeStart = 0;
    //    }

    //    if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
    //    {
    //        var delta = Time.realtimeSinceStartup - leftClickTimeStart;
    //        if (delta < 0.5f)
    //        {
    //            leftClick = true;
    //        }
    //    }

    //    return leftClick;
    //}

    void OnGUI()
    {
        
#if UNITY_EDITOR
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
        {
            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
        }
#endif
        
        if (Event.current.alt && Event.current.type == EventType.mouseDrag && Event.current.button == 0)
        {
            DoArcBallRotation();
        }
       
        if (Event.current.type == EventType.mouseDrag && Event.current.button == 1)
        {
            DoFpsRotation();   
        }

        if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)
        {
            DoPanning();
        }

        if (Event.current.type == EventType.ScrollWheel)
        {
            DoScrolling();
        }

        if (Event.current.keyCode == KeyCode.F)
        {
            if (TargetGameObject != null)
            {
                TargetPosition = TargetGameObject.transform.position;
            }

            Distance = DefaultDistance;
            transform.position = TargetPosition - transform.forward * Distance;
        }

        if (Event.current.keyCode == KeyCode.R)
        {
            Distance = DefaultDistance;
            TargetPosition = Vector3.zero;
            transform.position = TargetPosition - transform.forward * Distance;
        }

        if (Event.current.keyCode == KeyCode.W)
        {
            forward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.S)
        {
            backward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.A)
        {
            left = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.D)
        {
            right = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.Alpha1)
        {
            _currentState = HandleSelectionState.Translate;
            if (_selectedTransformHandle) { _selectedTransformHandle.SetSelectionState(_currentState); }
        }

        if (Event.current.keyCode == KeyCode.Alpha2)
        {
            _currentState = HandleSelectionState.Rotate;
            if (_selectedTransformHandle) { _selectedTransformHandle.SetSelectionState(_currentState); }
        }

        if (Event.current.keyCode == KeyCode.Alpha3)
        {
            _currentState = HandleSelectionState.Scale;
            if (_selectedTransformHandle) { _selectedTransformHandle.SetSelectionState(_currentState); }
        }
        
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Event.current.alt)
        {
            if (_selectedTransformHandle && _selectedTransformHandle.FreezeObjectPicking)
            {
                _selectedTransformHandle.FreezeObjectPicking = false;
            }
            else
            {
                DoObjectPicking();
            }
        }
    }

    void DoObjectPicking()
    {
        var mousePos = Event.current.mousePosition;
        Ray CameraRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, Screen.height - mousePos.y, 0));
        RaycastHit hit;

        // If we hit an object
        if (Physics.Raycast(CameraRay, out hit, 1000))
        {
            var transformHandle = hit.collider.gameObject.GetComponent<TransformHandle>();

            // If we hit a new selectable object
            if (transformHandle != null && transformHandle != _selectedTransformHandle)
            {
                if (_selectedTransformHandle != null)
                {
                    Debug.Log("Reset");
                    _selectedTransformHandle.Disable();
                }

                Debug.Log("Selected transform: " + transformHandle.gameObject.name);

                transformHandle.Enable();
                transformHandle.SetSelectionState(_currentState);
                _selectedTransformHandle = transformHandle;
                TargetGameObject = hit.collider.gameObject;
            }
            // If we hit a non-selectable object
            else if (transformHandle == null && _selectedTransformHandle != null)
            {
                Debug.Log("Reset");
                _selectedTransformHandle.Disable();
                _selectedTransformHandle = null;
            }
        }
        // If we miss a hit
        else if (_selectedTransformHandle != null)
        {
            Debug.Log("Reset2");
            _selectedTransformHandle.Disable();
            _selectedTransformHandle = null;
        }
    }
}

