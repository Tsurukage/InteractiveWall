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
    public LineRenderer lineRenderer;
    public Camera DrawingCam;
    public event UnityAction<DrawingBoardInfo> OnDrawingEnd;

    public void OnStartDraw(Vector3 pos)
    {
        isDrawing = true;
        points.Clear();
        lineRenderer.positionCount = 0;
    }
    public void OnDrawing(RaycastHit hit)
    {
        if (!isDrawing) return;
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
        if (points.Count >= 2)
        {
            //var smooth = LineSimplifier.GetSmoothPath(adjust.Select(v => new Vector2(v.x, v.y)).ToList());
            //var path = LineSimplifier.RamerDouglasPeucker(smooth.Select(v => new Vector3(v.x, transform.position.y, v.y)).ToList(), 1);
            //var adjust = LineSimplifier.AdjustPathGridAngles(info.Path.Select(p => p.ToXZInt()).ToList());
            //var flatPoints = GetFlatPoints(points, Quaternion.Inverse(DrawingCam.transform.rotation), transform.position);
            DrawLineRenderer(lineRenderer, points);
            lineRenderer.Simplify(1);
            var info = GenerateInfoFromLineRenderer(lineRenderer);
            OnDrawingEnd?.Invoke(info);
            return;
        }
        Debug.LogWarning("路径点数量不足，无法生成道路。");
        isDrawing = false;
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