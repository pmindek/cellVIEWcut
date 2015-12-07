using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CameraState
{
    Normal = 0,
    Fps = 1,
    ArcBall = 2,
    Focus = 3
}

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    public Camera Camera;

    public float DefaultDistance = 5.0f;

    public float AcrBallRotationSpeed = 0.1f;
    public float FpsRotationSpeed = 0.1f;
    public float TranslationSpeed = 10.0f;
    public float ScrollingSpeed = 1.0f;
    public float PannigSpeed = 0.1f;

    public float Distance = 25;
    public float AngleY = 0;
    public float AngleX = 0;

    [Range(0.01f,1)]
    public float Smoothing = 0.25f;

    public Vector3 TargetPosition;

    public bool FollowTarget;
    public Transform TargetTransform;

    /*****/

    private CameraState _cameraState = CameraState.Normal;

    private float _deltaTime = 0;
    private float _lastUpdateTime = 0;

    public float _currentAngleX;
    public float _currentAngleY;

    private bool _forward;
    private bool _backward;
    private bool _right;
    private bool _left;

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

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update = null;
        }
#endif
    }

    public float GetRealDistance()
    {
        return Vector3.Distance(Camera.main.transform.position, TargetPosition);
    }

    /*****/

    void Update()
    {
        if (Camera.main.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>())
        {
            Camera.main.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>().startDistance = Vector3.Distance(Camera.main.transform.position, TargetPosition) - 100;
        }

        if (Camera == null && Camera.main == null) return;
        if (Camera == null && Camera.main != null) Camera = Camera.main;
        
        _deltaTime = Time.realtimeSinceStartup - _lastUpdateTime;
        _lastUpdateTime = Time.realtimeSinceStartup;

        if (FollowTarget && TargetTransform != null)
        {
            TargetPosition = TargetTransform.position;
        }

        if (_forward)
        {
            TargetPosition += transform.forward * TranslationSpeed * _deltaTime;
            transform.position += transform.forward * TranslationSpeed * _deltaTime;
        }

        if (_backward)
        {
            TargetPosition -= transform.forward * TranslationSpeed * _deltaTime;
            transform.position -= transform.forward * TranslationSpeed * _deltaTime;
        }

        if (_right)
        {
            TargetPosition += transform.right * TranslationSpeed * _deltaTime;
            transform.position += transform.right * TranslationSpeed * _deltaTime;
        }

        if (_left)
        {
            TargetPosition -= transform.right * TranslationSpeed * _deltaTime;
            transform.position -= transform.right * TranslationSpeed * _deltaTime;
        }
        
        if (_cameraState == CameraState.ArcBall)
        {
            _currentAngleX = Mathf.Lerp(_currentAngleX, AngleX, Smoothing);
            _currentAngleY = Mathf.Lerp(_currentAngleY, AngleY, Smoothing);

            transform.rotation = Quaternion.Euler(_currentAngleX, _currentAngleY, 0.0f);
            transform.position = TargetPosition + transform.rotation*Vector3.back*Distance;

            Camera.transform.position = transform.position;
            Camera.transform.rotation = transform.rotation;
        }
        else if (_cameraState == CameraState.Focus)
        {
            Camera.transform.position = Vector3.Lerp(Camera.transform.position, transform.position, 0.1f);
            Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation, transform.rotation, 0.1f);

            var d = Vector3.Distance(Camera.main.transform.position, TargetPosition);

            if (d  <= DefaultDistance + 0.15f)
            {
                _cameraState = CameraState.Normal;
                Camera.transform.position = Camera.transform.position;
                Camera.transform.rotation = Camera.transform.rotation;
            }

            //_cameraState = CameraState.Normal;
        }
        else
        {
            _currentAngleX = AngleX;
            _currentAngleY = AngleY;

            transform.position = TargetPosition - transform.forward * Distance;
            transform.rotation = Quaternion.Euler(AngleX, AngleY, 0.0f);
            TargetPosition = transform.position + transform.forward * Distance;

            Camera.transform.position = Vector3.Lerp(Camera.transform.position, transform.position, Smoothing);
            Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation, transform.rotation, Smoothing);
        }
    }
    
    //void DoArcBallRotation(float angleX, float angleY)
    //{
    //    currentAngleX = Mathf.Lerp(currentAngleX, AngleX, Smoothing);
    //    currentAngleY = Mathf.Lerp(currentAngleY, AngleY, Smoothing);

    //    transform.rotation = Quaternion.Euler(currentAngleX, angleY, 0.0f);
    //    transform.position = TargetPosition + transform.rotation * Vector3.back * Distance;

    //    Camera.transform.position = transform.position;
    //    Camera.transform.rotation = transform.rotation;
    //}

    //void DoFpsRotation(float angleX, float angleY)
    //{
    //    transform.position = TargetPosition - transform.forward * Distance;
    //    transform.rotation = Quaternion.Euler(angleX, angleY, 0.0f);
    //    TargetPosition = transform.position + transform.forward * Distance;
    //}

    void DoPanning(float DeltaX, float DeltaY)
    {
        TargetPosition += transform.up * DeltaY * PannigSpeed;
        transform.position += transform.up * DeltaY * PannigSpeed;

        TargetPosition -= transform.right * DeltaX * PannigSpeed;
        transform.position -= transform.right * DeltaX * PannigSpeed;
    }
    
    void DoScrolling(float DeltaY)
    {
        Distance += Event.current.delta.y * ScrollingSpeed;
        transform.position = TargetPosition - transform.forward * Distance;

        if (Distance < 0)
        {
            TargetPosition = transform.position + transform.forward * DefaultDistance;
            Distance = Vector3.Distance(TargetPosition, transform.position);
        }
    }
    
    private void OnGUI()
    {

#if UNITY_EDITOR
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
        {
            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
        }
#endif

        if (_cameraState == CameraState.Focus) return;

        if (Event.current.alt)
        {
            _cameraState = CameraState.ArcBall;
        }
        else
        {
            if (_cameraState == CameraState.ArcBall)
            {
                AngleX = _currentAngleX;
                AngleY = _currentAngleY;

                transform.rotation = Quaternion.Euler(_currentAngleX, _currentAngleY, 0.0f);
                transform.position = TargetPosition + transform.rotation * Vector3.back * Distance;

                Camera.transform.position = transform.position;
                Camera.transform.rotation = transform.rotation;

                _cameraState = CameraState.Normal;
            }
        }
        
        if (Event.current.alt && Event.current.type == EventType.mouseDrag && Event.current.button == 0)
        {
            AngleY += Event.current.delta.x * AcrBallRotationSpeed;
            AngleX += Event.current.delta.y * AcrBallRotationSpeed;
        }
        else if (Event.current.type == EventType.mouseDrag && Event.current.button == 1)
        {
            AngleY += Event.current.delta.x * FpsRotationSpeed;
            AngleX += Event.current.delta.y * FpsRotationSpeed;
        }
        else if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)
        {
            DoPanning(Event.current.delta.x * PannigSpeed, Event.current.delta.y * PannigSpeed);
        }
        else if (Event.current.type == EventType.ScrollWheel)
        {
            DoScrolling(Event.current.delta.y * ScrollingSpeed);
        }

        if (Event.current.keyCode == KeyCode.F)
        {
            if (TargetTransform != null)
            {
                TargetPosition = TargetTransform.position;
            }

            Distance = DefaultDistance;
            transform.position = TargetPosition - transform.forward * Distance;

            _cameraState = CameraState.Focus;
        }

        if (Event.current.keyCode == KeyCode.R)
        {
            Distance = DefaultDistance;
            TargetPosition = Vector3.zero;
            transform.position = TargetPosition - transform.forward * Distance;
        }

        if (Event.current.keyCode == KeyCode.W)
        {
            _forward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.S)
        {
            _backward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.A)
        {
            _left = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.D)
        {
            _right = Event.current.type == EventType.KeyDown;
        }
    }
}
