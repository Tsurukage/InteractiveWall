using System.Collections.Generic;
using System.Linq;
using GMVC.Utls;
using GMVC.Views;
using UnityEngine;
using UnityEngine.UI;
using Utls;

public class DrawingBoardManager : MonoBehaviour
{
    public RayInputHandler RayInput;
    public RoadGenerator RoadGenerator;
    [Header("画板设置")]
    public RoadDrawer drawingBoardPrefab; // 画板预制体
    public Camera DrawingCam;
    public Transform DrawingArea;
    public HorizontalLayoutGroup layoutGroup; // 画板的布局组件
    public RectTransform[] uiFrames; // UI框的数组
    public List<RoadDrawer> drawingBoards;
    public RoadCircuit[] Circuits;
    Dictionary<int,RoadDrawer> touchMap = new();
    
    public void Init()
    {
        drawingBoardPrefab.Display(false);
        RayInput.OnDrawStart.AddListener(DrawStart);
        RayInput.OnDrawEnd.AddListener(DrawEnd);
        RayInput.OnDrawing.AddListener(Drawing);
        RoadGenerator.Init(drawingBoards.Count);
        for (var i = 0; i < drawingBoards.Count; i++)
        {
            var board = drawingBoards[i];
            var circuit = Circuits[i];
            circuit.Init(RoadGenerator);
            board.OnDrawingEnd += circuit.OnDrawEnd;
        }
    }

    void Drawing(int id, RaycastHit hit)
    {
        if (!touchMap.TryGetValue(id, out var roadDrawer)) return;
        roadDrawer.OnDrawing(hit);
    }

    void DrawEnd(int id)
    {
        if (!touchMap.TryGetValue(id, out var roadDrawer)) return;
        roadDrawer.OnDrawEnd(Vector3.zero);
        touchMap.Remove(id);
    }

    void DrawStart(int id, RaycastHit hit)
    {
        if (!hit.collider) return;
        // 根据触摸位置进行射线检测，确定被触摸的画板
        var roadDrawer = hit.collider.GetComponentInParent<RoadDrawer>();
        if (!roadDrawer) return;
        var (key, drawer) = touchMap.FirstOrDefault(k => k.Value == roadDrawer);
        if (!drawer) touchMap.Add(id, roadDrawer);
        touchMap[id] = roadDrawer;
        roadDrawer.OnStartDraw(hit.point);
    }

    [Sirenix.OdinInspector.Button]public void GetUiElements()
    {
        var rects = new List<RectTransform>();
        foreach (Transform t in layoutGroup.transform)
        {
            var v = t.GetComponent<View>();
            if(v) rects.Add((RectTransform)v.transform);
        }
        uiFrames = rects.ToArray();
        GenerateDrawingBoards();
        AlignBoardsWithUI();
    }
    // 生成画板
    void GenerateDrawingBoards()
    {
        if (drawingBoards.Any())
        {
            foreach (var db in drawingBoards.ToArray())
            {
                drawingBoards.Remove(db);
                Destroy(db);
            }
        }
        for (var i = 0; i < uiFrames.Length; i++)
        {
            var board = Instantiate(drawingBoardPrefab, transform);
            board.Display(true);
            board.name = "DrawingBoard_" + (i + 1);
            board.Index = i;
            drawingBoards.Add(board);
        }
    }

    // 对齐画板和UI框
    void AlignBoardsWithUI()
    {
        for (var i = 0; i < uiFrames.Length; i++)
        {
            // 获取画板和对应的UI框
            var board = drawingBoards[i];
            var uiFrame = uiFrames[i];

            // 将画板的位置对齐到UI框的位置
            var screenPos = uiFrame.position;
            var worldPos = DrawingCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0)); // 根据需要调整Z值
            board.transform.position = uiFrame.position;
        }
    }
}
