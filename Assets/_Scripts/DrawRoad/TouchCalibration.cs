using System.Collections.Generic;
using GMVC.Utls;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchCalibration : MonoBehaviour
{
    public Camera drawingCamera;
    public RectTransform drawingRect;
    public List<RectTransform> touchPoints;

    void Update() => HandleTouchInput();

    void HandleTouchInput()
    {
        // 确保 touchPoints 列表的大小与当前触摸点数量一致
        for (var i = 0; i < touchPoints.Count; i++)
        {
            var tp = touchPoints[i];
            if (i >= Touch.activeTouches.Count)
            {
                tp.Display(false);
                continue;
            }
            var touch = Touch.activeTouches[i];
            // 获取触摸位置
            var screenPosition = touch.screenPosition;
            // 将屏幕坐标转换为 UI 坐标
            Vector2 uiPosition;
            var isInRect = RectTransformUtility.RectangleContainsScreenPoint(drawingRect, screenPosition, drawingCamera);
            tp.Display(isInRect);
            if (!isInRect) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingRect,
                screenPosition,
                drawingCamera,
                out uiPosition
            );
            // 更新 touchPoint 的位置
            tp.anchoredPosition = uiPosition;
        }
    }
}
