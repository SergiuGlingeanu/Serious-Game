using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 愛 = Tom.Utility.Utilities;

[RequireComponent(typeof(Camera))]
public class UniversalCameraController : MonoBehaviour
{
    [Header("Main Settings")]
    public Renderer target;
    public Vector2 zoomLimits;
    public float rotationAngle = 45f;

    [Header("Desktop Settings")]
    [Range(0.01f, 100f)] public float desktopPanSensitivity = 2f;
    [Range(0.01f, 100f)] public float desktopZoomSensitivity = 5f;

    [Header("Mobile Settings")]
    [Range(0.01f, 100f)] public float mobilePanSensitivity = 2f;
    [Range(0.01f, 100f)] public float mobileZoomSensitivity = 0.1f;

    private float _internalZoomValue = 16;
    private Touch _initTouch;
    private Touch _lastTouch;
    private Camera _camera;
    private Vector2 _axes;

    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        if (!_camera.orthographic)
        {
            愛.Print($"Specified Camera is not marked as Ortographic. This setting will be forced!", 愛.LogLevel.Warning);
            _camera.orthographic = true;
        }
        Debug.Log(GameSettings.kInitialOrtographicSize);
        _camera.orthographicSize = GameSettings.kInitialOrtographicSize;
        _internalZoomValue = _camera.orthographicSize;
    }
    void Update()
    {
        _axes = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(2) && !GameSettings.panBlocked) //Desktop Pan
        {
            //transform.position -= new Vector3(_axes.y, 0f, _axes.x) * desktopPanSensitivity * Time.deltaTime;
            transform.position -= transform.right * _axes.x * desktopPanSensitivity * Time.deltaTime;
            transform.position -= transform.up * _axes.y * desktopPanSensitivity * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.H))
            Rotate(true);

        if (Input.mouseScrollDelta.magnitude != 0) //Desktop Mouse Zoom
            _internalZoomValue -= Input.mouseScrollDelta.y * desktopZoomSensitivity;

        if (GameSettings.enableMobileMode)
            UpdateMobileGestures();

        _internalZoomValue = Mathf.Clamp(_internalZoomValue, zoomLimits.x, zoomLimits.y);
        _camera.orthographicSize = _internalZoomValue;
    }

    private Vector2 _lastPanTouchPosition;
    private int _lastFingerId;

    private void UpdateMobileGestures() {
        //if (!GameSettings.enableMobileMode) return;

        if (Input.touchCount == 1 && !GameSettings.panBlocked) { //Mobile Pan
            _initTouch = Input.GetTouch(0);
            if (_initTouch.phase == TouchPhase.Began)
            {
                _lastPanTouchPosition = _initTouch.position;
                _lastFingerId = _initTouch.fingerId;
            }
            else if (_initTouch.fingerId == _lastFingerId && _initTouch.phase == TouchPhase.Moved) {
                Vector3 movementDelta = _camera.ScreenToViewportPoint(_lastPanTouchPosition - _initTouch.position);
                transform.Translate(new Vector3(movementDelta.x * mobilePanSensitivity, movementDelta.y * mobilePanSensitivity, 0f));

                _lastPanTouchPosition = _initTouch.position;
            }
            //Vector2 delta = _initTouch.deltaPosition;
            //transform.position -= transform.right * delta.x * mobilePanSensitivity * Time.deltaTime;
            //transform.position -= transform.up * delta.y * mobilePanSensitivity * Time.deltaTime;
        }
        else if (Input.touchCount >= 2) //Mobile Zoom
        {
            _initTouch = Input.GetTouch(0);
            _lastTouch = Input.GetTouch(1);

            Vector2 lastFrameInitDelta = _initTouch.position - _initTouch.deltaPosition;
            Vector2 lastFrameLastDelta = _lastTouch.position - _lastTouch.deltaPosition;
            float lastFrameDelta = (lastFrameInitDelta - lastFrameLastDelta).magnitude;
            float currentFrameDelta = (_initTouch.position - _lastTouch.position).magnitude;

            _internalZoomValue += (lastFrameDelta - currentFrameDelta) * mobileZoomSensitivity;
        }
    }

    public void Rotate(bool direction) {
        if (!target) return;
        transform.RotateAround(target.bounds.center, Vector3.up, rotationAngle * (direction ? 1f : -1f));
    }
}
