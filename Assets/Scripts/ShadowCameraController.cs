using UnityEngine;

[ExecuteInEditMode]
public class ShadowCameraController : MonoBehaviour
{
    public CameraController CameraController;

    public float AngleX;
    public float AngleY;
    public float OffsetZ;

    public bool FollowCamera;
    public Vector3 TargetPosition;

    private float _angleX;
    private float _angleY;
    private float _offsetZ;
    private bool _previousFollowCamera = false;

    void Update()
    {
        if (FollowCamera)
        {
            if (!_previousFollowCamera)
            {
                AngleX = 0;
                AngleY = 0;
                //OffsetZ = 0;
            }

            _angleX = CameraController._currentAngleX + AngleX;
            _angleY = CameraController._currentAngleY + AngleY;
            //_offsetZ = Mathf.Max(0, CameraController.GetRealDistance()) + OffsetZ;
            TargetPosition = CameraController.TargetPosition;

            _previousFollowCamera = true;
        }
        else
        {
            if (_previousFollowCamera)
            {
                AngleX = _angleX;
                AngleY = _angleY;
                //OffsetZ = _offsetZ;
                TargetPosition = Vector3.zero;
            }

            _angleX = AngleX;
            _angleY = AngleY;
            _previousFollowCamera = false;
        }

        _offsetZ = OffsetZ;
        //var distance = CameraController.Distance;

        //var lookAt = CameraController.TargetPosition;

        ////var posY = Mathf.Sin(Y * 0.01f);
        //var posX = Mathf.Sin(X * 0.01f);
        //var posZ = Mathf.Cos(X * 0.01f);

        //var vec = new Vector3(posX, 0, -posZ) * distance;



        //transform.localPosition = vec;
        //transform.LookAt(lookAt);
        //transform.localPosition -= transform.forward*OffsetZ;

        transform.rotation = Quaternion.Euler(_angleX, _angleY, 0.0f);
        transform.position = TargetPosition + transform.rotation * Vector3.back *_offsetZ ;

        
    }
}