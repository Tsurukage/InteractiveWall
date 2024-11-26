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
    void Start()
    {
        EnhancedTouchSupport.Enable();
    }

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
                    OnDrawing.Invoke(touch.touchId,GetTouchRayHit(touch));
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnDrawEnd.Invoke(touch.touchId);
                    break;
            }
        }
    }
    //void HandleInput()
    //{
    //    // 鼠标左键按下，开始绘制
    //    if (Input.GetMouseButtonDown(0)) OnDrawStart.Invoke(GetMouseRayHit().point);
    //    // 鼠标左键抬起，结束绘制，生成道路
    //    else if (Input.GetMouseButtonUp(0)) OnDrawEnd.Invoke(GetMouseRayHit().point);
    //    // 鼠标移动，记录路径点
    //    else if (Input.GetMouseButton(0)) OnDrawing.Invoke(GetMouseRayHit());
    //}
    RaycastHit GetTouchRayHit(Touch touch)
    {
        var ray = drawingCamera.ScreenPointToRay(touch.screenPosition);
        return Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer) ? hit : default;
    }
    //RaycastHit GetMouseRayHit()
    //{
    //    var ray = drawingCamera.ScreenPointToRay(Input.mousePosition);
    //    return Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer) ? hit : default;
    //}
}