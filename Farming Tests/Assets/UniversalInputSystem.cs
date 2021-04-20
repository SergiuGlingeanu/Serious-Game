using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tom.Utility;

[DefaultExecutionOrder(-100)]
public class UniversalInputSystem : MonoBehaviour
{
    private RaycastHit[] _hits;
    private Camera _camera;
    public float inputRaycastLength;
    public LayerMask inputRaycastLayerMask;

    private HashSet<IUniversalInputHandler> _handlers;
    private IUniversalInputHandler _lastHandler;

    private void Awake()
    {
        _camera = Camera.main;
        _handlers = new HashSet<IUniversalInputHandler>();
        _hits = new RaycastHit[1]; //Buffer size of 1
        try
        {
            GameSettings.deviceType = Tom.Utility.Utilities.GetDeviceType();
        }
        catch (System.EntryPointNotFoundException)
        { //When this call fails, the Device will be set to Desktop.
            Debug.LogWarning("WebGL Device Fetch failed. This call will always fail outside of a WebGL build.");
            GameSettings.deviceType = DeviceType.Desktop;
        }
    }

    void Update() //No Multi-Touch support
    {
        Vector3 screenPosition;
        if (GameSettings.deviceType == DeviceType.Handheld)
        {
            if (Input.touchCount == 0) return;
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) ReleaseLastHandler();
            if (t.phase != TouchPhase.Began && t.phase != TouchPhase.Moved) return;
            screenPosition = t.position;
        }
        else
        {
            if (Input.GetMouseButtonUp(0)) ReleaseLastHandler();
            if (!Input.GetMouseButton(0)) return;
            screenPosition = Input.mousePosition;
        }

        if (Physics.RaycastNonAlloc(_camera.ScreenPointToRay(screenPosition), _hits, inputRaycastLength, inputRaycastLayerMask) != 0)
        {
            IUniversalInputHandler handler = _hits[0].transform.GetComponent<IUniversalInputHandler>();
            if (handler != null)
            {
                if (!_handlers.Contains(handler))
                    handler.OnUniversalButtonDown();
                _handlers.Add(handler);
                handler.OnUniversalButton();
                _lastHandler = handler;
            }
        }
        else
            ReleaseLastHandler();
    }

    private void ReleaseLastHandler() {
        if (_lastHandler != null)
        {
            _handlers.Remove(_lastHandler);
            _lastHandler.OnUniversalButtonUp();
            _lastHandler = null;
        }
    }
}
