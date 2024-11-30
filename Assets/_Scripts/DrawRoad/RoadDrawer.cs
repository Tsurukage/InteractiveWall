using System.Collections.Generic;
using System.Linq;
using GMVC.Core;
using UnityEngine;
using UnityEngine.Events;
using Utls;

public class RoadDrawer : MonoBehaviour
{
    public int Index;
    public float pointSpacing = 1f; // 路径点的最小间隔
    List<Vector3> points = new(); // 记录的路径点
    bool isDrawing = false;
    public Material validMat;
    public Material invalidMat;
    public LineRenderer lineRenderer;
    bool invalidInput;
    public event UnityAction<DrawingBoardInfo> OnDrawingEnd;

    public void OnStartDraw(Vector3 pos)
    {
        invalidInput = false;
        isDrawing = true;
        points.Clear();
        lineRenderer.positionCount = 0;
        lineRenderer.material = validMat;
    }
    public void OnDrawing(RaycastHit hit)
    {
        if (!isDrawing) return;
        if (!invalidInput)
        {
            invalidInput = hit.collider == null;
            if (invalidInput && lineRenderer.material != invalidMat) 
                lineRenderer.material = invalidMat;
        }

        if (App.Setting.GenerateCutPathOnError && invalidInput)
        {
            OnGeneratePathPoints();
            return;
        }

        var pos = hit.point;
        if (pos == Vector3.zero) return;
        // 只有当新点与上一个点的距离大于pointSpacing时才添加
        if (points.Count >= 1)
        {
            if (Vector2.Distance(pos.ToXZ(), points.Last().ToXZ()) < pointSpacing) return;
        }

        points.Add(pos);
        var index = lineRenderer.positionCount;
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(index, pos);
    }
    
    public void OnDrawEnd(Vector3 pos) => OnGeneratePathPoints();

    void OnGeneratePathPoints()
    {
        isDrawing = false;
        if (points.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        var path = points.ToList();
        var first = path.First();
        var last = path.Last();
        // 检查路径是否闭合
        var isCycle = Vector3.Distance(first, last) < 1f;
        var line = path.ToList();
        if (isCycle)
        {
            line.Add(first);
            line.Add(path[1]);
        }
        // 更新 LineRenderer
        DrawLineRenderer(lineRenderer, line);
        var info = new DrawingBoardInfo(Index, transform, path, line, isCycle);
        OnDrawingEnd?.Invoke(info);
    }

    void DrawLineRenderer(LineRenderer lr,List<Vector3> line)
    {
        lr.positionCount = line.Count;
        for (var i = 0; i < line.Count; i++)
        {
            var p = line[i];
            lr.SetPosition(i, p);
        }
    }

    List<Vector3> GetFlatPoints(List<Vector3> list,Quaternion rotation,Vector3 position)
    {
        var inversion = Quaternion.Inverse(rotation);
        // Get the drawing board's rotation and position
        var flatPoints = list.Select(p =>
        {
            var localPos = p - position;
            var rotatePoint = inversion * localPos;
            var worldPoint = rotatePoint + position;
            return worldPoint;
        }).ToList();
        return flatPoints;
    }
}

public record DrawingBoardInfo
{
    public int Index;
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
    public IList<Vector3> Path;
    public IList<Vector3> Line;
    public bool IsCycle;

    public DrawingBoardInfo(int index,Transform tran,IList<Vector3> path, IList<Vector3> line, bool isCycle)
    {
        Position = tran.position;
        Scale = tran.lossyScale;
        Rotation = tran.rotation;
        Path = path;
        Line = line;
        IsCycle = isCycle;
        Index = index;
    }

    public List<Vector3> GetCoordinatePath(Transform target) =>
        GetCoordinatePath(target, Path, Position, Rotation, Scale);

    public static List<Vector3> GetCoordinatePath(Transform target, IList<Vector3> path, Vector3 position,
        Quaternion rotation, Vector3 scale)
    {
        var worldMatrix = WorldMatrix(target.position, target.lossyScale, target.rotation, position, rotation, scale);
        return path.Select(p => worldMatrix.MultiplyPoint3x4(p)).ToList();
    }

    // 绘制板到世界地块的变换矩阵
    public static Matrix4x4 WorldMatrix(Vector3 worldTilePosition, Vector3 worldTileScale, Quaternion worldTileRotation,
        Vector3 position, Quaternion rotation, Vector3 scale) =>
        Matrix4x4.TRS(worldTilePosition, worldTileRotation, worldTileScale) *
        Matrix4x4.TRS(position, rotation, scale).inverse;
}