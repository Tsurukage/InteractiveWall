using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Utls;
using System;

public class RoadDrawer : MonoBehaviour
{
    public float pointSpacing = 1f; // 路径点的最小间隔
    public RoadGenerator roadGenerator; // 用于生成道路的脚本
    public RayInputHandler rayHandler; // 用于处理输入的脚本
    List<Vector3> points = new (); // 记录的路径点
    bool isDrawing = false;

    public LineRenderer lineRenderer;
    public GridManager GridManager;
    public Action<IEnumerable<Vector3>> OnNewPathGenerated = delegate { };

    void Start()
    {
        rayHandler.OnDrawStart.AddListener(OnStartDraw);
        rayHandler.OnDrawEnd.AddListener(OnDrawEnd);
        rayHandler.OnDrawing.AddListener(OnDrawing);
    }

    void OnDrawing(RaycastHit hit)
    {
        if (!isDrawing) return;
        var pos = hit.point;
        if (pos == Vector3.zero) return;
        // 只有当新点与上一个点的距离大于pointSpacing时才添加
        if (points.Count == 0 ||
            Vector3.Distance(pos, points[^1]) >= pointSpacing)
        {
            points.Add(pos);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(points.Count - 1, pos);
        }
    }

    void OnDrawEnd(Vector3 pos)
    {
        isDrawing = false;
        if (points.Count >= 2)
        {
            OnNewPathGenerated(points);
            var path = GridManager.AdjustPathAngles(points.Select(p => p.ToXZInt()).ToList());
            lineRenderer.positionCount = path.Count;
            for (var i = 0; i < path.Count; i++)
            {
                var p = path[i];
                lineRenderer.SetPosition(i, new Vector3(p.x, 0, p.y));
            }
            lineRenderer.Simplify(1);
            GenerateRoad();
        }
        else
        {
            Debug.LogWarning("路径点数量不足，无法生成道路。");
            isDrawing = false;
        }
    }
    [Button]void GenerateRoad()
    {
        roadGenerator?.GenerateRoad(points);
        lineRenderer.positionCount = 0;
    }

    void OnStartDraw(Vector3 pos)
    {
        roadGenerator?.RemoveRoad();
        isDrawing = true;
        points.Clear();
        lineRenderer.positionCount = 0;
    }
}