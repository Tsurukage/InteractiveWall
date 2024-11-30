using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class RayInputHandler : MonoBehaviour
{
    public Camera drawingCamera;
    public UnityEvent<int,RaycastHit> OnDrawStart;
    public UnityEvent<int,RaycastHit> OnDrawing;
    public UnityEvent<int> OnDrawEnd;
    public LayerMask groundLayer; // 用于射线检测的地面层级

    public bool enableInput = true;
    
    void Update()
    {
        if (enableInput) HandleTouchInput();
    }
    void HandleTouchInput()
    {
        foreach (var touch in Touch.activeTouches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnDrawStart.Invoke(touch.touchId,GetTouchRayHit(touch));
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    try
                    {
                        OnDrawing.Invoke(touch.touchId, GetTouchRayHit(touch));
                    }
                    catch (Exception e)
                    {
                        OnDrawEnd.Invoke(touch.touchId);
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnDrawEnd.Invoke(touch.touchId);
                    break;
            }
        }
    }
    RaycastHit GetTouchRayHit(Touch touch)
    {
        var ray = drawingCamera.ScreenPointToRay(touch.screenPosition);
        return Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer) ? hit : default;
    }
}