using UnityEngine;
using UnityEngine.Events;

public class RayInputHandler : MonoBehaviour
{
    public UnityEvent<Vector3> OnDrawStart;
    public UnityEvent<RaycastHit> OnDrawing;
    public UnityEvent<Vector3> OnDrawEnd;
    public LayerMask groundLayer; // 用于射线检测的地面层级

    public bool enableInput = true;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        if (enableInput)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        // 鼠标左键按下，开始绘制
        if (Input.GetMouseButtonDown(0)) OnDrawStart.Invoke(GetMouseRayHit().point);
        // 鼠标左键抬起，结束绘制，生成道路
        else if (Input.GetMouseButtonUp(0)) OnDrawEnd.Invoke(GetMouseRayHit().point);
        // 鼠标移动，记录路径点
        else if (Input.GetMouseButton(0)) OnDrawing.Invoke(GetMouseRayHit());
    }

    RaycastHit GetMouseRayHit()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer) ? hit : default;
    }
}