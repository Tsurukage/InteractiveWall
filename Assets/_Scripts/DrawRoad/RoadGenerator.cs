using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EasyRoads3Dv3;
using Sirenix.OdinInspector;
using Utls;
using Random = UnityEngine.Random;

public class RoadGenerator : MonoBehaviour
{
    public Material roadMaterial;
    [OnStateUpdate(nameof(RoadTypeUpdate))]public float roadWidth = 8f;
    void RoadTypeUpdate()
    {
        if (roadType == null) return;
        roadType.roadWidth = roadWidth;
        roadType.roadMaterial = roadMaterial;
    }
    ERRoadNetwork roadNetwork;
    ERRoadType roadType;
    ERRoad road;

    void Start()
    {
        // 初始化道路网络
        roadNetwork = new ERRoadNetwork();

        // 定义道路类型
        roadType = new ERRoadType();
        roadType.roadWidth = roadWidth;
        roadType.roadMaterial = roadMaterial;
    }

    public void RemoveRoad()
    {
        if (road != null)
        {
            Destroy(road.gameObject);
            roadNetwork.BuildRoadNetwork();
            road = null;
        }
    }

    public void GenerateRoad(List<Vector3> points)
    {
        if (points == null || points.Count < 2)
        {
            Debug.LogWarning("路径点数量不足，无法生成道路。");
            return;
        }

        var y = points.First().y;
        var smooth = GetSmoothPath(points.Select(p => p.ToXZ()).ToList()).Select(v => new Vector3(v.x, y, v.y)).ToArray();
        points = LineSimplifier.RamerDouglasPeucker(smooth.ToList(), 1);
        // 移除现有的道路
        RemoveRoad();
        // 创建新道路
        road = roadNetwork.CreateRoad("Road_" + Random.Range(0, 1000), roadType,
            points.Select(p=>p.ChangeY(y)).ToArray());
            //GetSmoothPath(points.Select(p => p.ToXZ()).ToList()).Select(v => new Vector3(v.x, y, v.y)).ToArray());
        road.gameObject.transform.localPosition = Vector3.zero;
        // 构建道路网络
        roadNetwork.BuildRoadNetwork();
    }
    List<Vector2> GetSmoothPath(List<Vector2> keyPoints)
    {
        var smoothPath = new List<Vector2>();

        for (var i = 0; i < keyPoints.Count - 1; i++)
        {
            var p0 = i > 0 ? keyPoints[i - 1] : keyPoints[i];
            var p1 = keyPoints[i];
            var p2 = keyPoints[i + 1];
            var p3 = i + 2 < keyPoints.Count ? keyPoints[i + 2] : keyPoints[i + 1];

            for (float t = 0; t < 1; t += 0.1f)
            {
                var point = CatmullRom(p0, p1, p2, p3, t);
                smoothPath.Add(point);
            }
        }

        return smoothPath;
        Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;

            var result = 0.5f * ((2f * p1) +
                                 (-p0 + p2) * t +
                                 (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                                 (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
            return result;
        }
    }
}
public static class LineSimplifier
{
    public static List<Vector3> RamerDouglasPeucker(List<Vector3> points, float tolerance)
    {
        if (points == null || points.Count < 3)
            return points;

        int firstPoint = 0;
        int lastPoint = points.Count - 1;
        List<int> pointIndicesToKeep = new List<int>();

        // Add the first and last indices to the keep list
        pointIndicesToKeep.Add(firstPoint);
        pointIndicesToKeep.Add(lastPoint);

        // Perform the RDP simplification
        SimplifySection(points, firstPoint, lastPoint, tolerance, ref pointIndicesToKeep);

        // Sort the kept indices
        pointIndicesToKeep.Sort();

        // Create the simplified list
        List<Vector3> result = new List<Vector3>();
        foreach (int index in pointIndicesToKeep)
        {
            result.Add(points[index]);
        }

        return result;
    }

    private static void SimplifySection(List<Vector3> points, int first, int last, float tolerance, ref List<int> keep)
    {
        if (last <= first + 1)
            return;

        float maxDistance = 0;
        int indexFarthest = 0;

        for (int i = first + 1; i < last; i++)
        {
            float distance = PerpendicularDistance(points[first], points[last], points[i]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexFarthest = i;
            }
        }

        if (maxDistance > tolerance)
        {
            // Keep the farthest point
            keep.Add(indexFarthest);

            // Recursive call
            SimplifySection(points, first, indexFarthest, tolerance, ref keep);
            SimplifySection(points, indexFarthest, last, tolerance, ref keep);
        }
    }

    private static float PerpendicularDistance(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        float area = Mathf.Abs(Vector3.Cross(lineEnd - lineStart, point - lineStart).y);
        float baseLength = Vector3.Distance(lineStart, lineEnd);
        return area / baseLength;
    }
}