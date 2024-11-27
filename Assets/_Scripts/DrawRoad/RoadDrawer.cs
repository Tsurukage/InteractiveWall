using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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

        var pos = hit.point;
        if (pos == Vector3.zero) return;
        // 只有当新点与上一个点的距离大于pointSpacing时才添加
        if (points.Count == 0 ||
            Vector2.Distance(pos.ToXZ(), points[^1].ToXZ()) >= pointSpacing)
        {
            points.Add(pos);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(points.Count - 1, pos);
        }

    }
    
    public void OnDrawEnd(Vector3 pos)
    {
        isDrawing = false;
        if (invalidInput || points.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        // 使用 Ramer-Douglas-Peucker 算法简化路径
        //var simplifiedPoints = LineSimplifier.RamerDouglasPeucker3D(points, 1f);

        //// 如果您有网格，可以将路径点转换为网格坐标（Vector3Int）
        //var gridPositions = simplifiedPoints.Select(WorldToGridPosition).ToList();
        //
        //// 调整路径中的尖角
        //var adjustedGridPositions = LineSimplifier.AdjustPathGridAngles3D(gridPositions);
        //
        //// 将网格坐标转换回世界坐标
        //var adjustedPoints = adjustedGridPositions.Select(GridToWorldPosition).ToList();
        var path = points.ToList();
        var first = path.First();
        var last = path.Last();
        // 检查路径是否闭合
        var isCycle = Vector3.Distance(first, last) < 1f;
        if (isCycle)
        {
            path.Add(first);
            path.Add(path[1]);
        }
        // 更新 LineRenderer
        DrawLineRenderer(lineRenderer, path);
        //lineRenderer.Simplify(1);
        var info = GenerateInfoFromLineRenderer(lineRenderer);
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

    [Button] public DrawingBoardInfo GenerateInfoFromLineRenderer(LineRenderer lr)
    {
        var path = new List<Vector3>();
        for (var i = 0; i < lr.positionCount; i++)
        {
            var point = lr.GetPosition(i);
            path.Add(point);
        }
        return new (Index, transform, path);
    }
}
public record DrawingBoardInfo
{
    public int Index;
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
    public IList<Vector3> Path;

    public DrawingBoardInfo(int index,Transform tran,IList<Vector3> path)
    {
        Position = tran.position;
        Scale = tran.lossyScale;
        Rotation = tran.rotation;
        Path = path;
        Index = index;
    }
    public List<Vector3> GetCoordinatePath(Transform tran) => GetCoordinatePath(tran,Path);

    public List<Vector3> GetCoordinatePath(Transform tran, IList<Vector3> path)
    {
        var worldMatrix = WorldMatrix(tran.position, tran.lossyScale, tran.rotation);
        return path.Select(p => worldMatrix.MultiplyPoint3x4(p)).ToList();
    }

    // 绘制板到世界地块的变换矩阵
    public Matrix4x4 WorldMatrix(Vector3 worldTilePosition, Vector3 worldTileScale, Quaternion worldTileRotation) =>
        Matrix4x4.TRS(worldTilePosition, worldTileRotation, worldTileScale) *
        Matrix4x4.TRS(Position, Rotation, Scale).inverse;
}