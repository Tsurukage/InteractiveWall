using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public static List<Vector2Int> AdjustPathGridAngles(List<Vector2Int> path)
    {
        var minAngleThreshold = 90f; // 设置最小角度阈值为 90 度
        var pathModified = false;

        do
        {
            pathModified = false;
            for (var i = 1; i < path.Count - 1; i++)
            {
                var prevCell = path[i - 1];
                var currCell = path[i];
                var nextCell = path[i + 1];

                var angle = CalculateAngle(prevCell, currCell, nextCell);

                if (angle < minAngleThreshold)
                {
                    // 寻找替代路径，插入邻近的 GridCell
                    var newCells = GetAdjustedCells(prevCell, currCell, nextCell);
                    if (newCells != null && newCells.Count > 0)
                    {
                        // 移除当前的尖角点
                        path.RemoveAt(i);
                        // 在当前位置插入新的 GridCell
                        path.InsertRange(i, newCells);
                        pathModified = true;
                        break; // 重新开始检测
                    }
                }
            }
        } while (pathModified);

        return path;

        List<Vector2Int> GetAdjustedCells(Vector2Int prevCell, Vector2Int currCell, Vector2Int nextCell)
        {
            // 获取当前 Cell 的邻居
            var neighbors = GetNeighbors(currCell);

            foreach (var neighbor in neighbors)
            {
                if (neighbor == prevCell || neighbor == nextCell)
                    continue;

                // 检查新的路径是否有更大的角度
                var newAngle = CalculateAngle(prevCell, neighbor, nextCell);
                if (newAngle >= 90f)
                {
                    // 检查邻居是否与前一个 Cell 相邻，确保路径连续
                    if (AreCellsAdjacent(prevCell, neighbor) && AreCellsAdjacent(neighbor, nextCell))
                    {
                        // 返回替代的路径段
                        return new List<Vector2Int> { neighbor };
                    }
                }
            }

            // 如果没有找到合适的邻居，可以考虑更复杂的替代路径
            return null;
        }

        bool AreCellsAdjacent(Vector2Int cellA, Vector2Int cellB)
        {
            var deltaX = Mathf.Abs(cellA.x - cellB.x);
            var deltaY = Mathf.Abs(cellA.y - cellB.y);

            return (deltaX <= 1 && deltaY <= 1 && (deltaX + deltaY) > 0);
        }

        List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            // 定义邻居的相对位置（包括斜对角）
            var directions = new Vector2Int[]
            {
                new(-1, 0), // 左
                new(1, 0), // 右
                new(0, -1), // 下
                new(0, 1), // 上
                new(-1, -1), // 左下
                new(-1, 1), // 左上
                new(1, -1), // 右下
                new(1, 1), // 右上
            };
            return directions.Select(dir => pos + dir).ToList();
        }

        float CalculateAngle(Vector2Int prevCell, Vector2Int currentCell, Vector2Int nextCell)
        {
            var prevDirection = new Vector2(
                currentCell.x - prevCell.x,
                currentCell.y - prevCell.y
            );

            var nextDirection = new Vector2(
                nextCell.x - currentCell.x,
                nextCell.y - currentCell.y
            );
            var angle = Vector2.Angle(prevDirection, nextDirection);
            return angle;
        }
    }

    public static List<Vector2> GetSmoothPath(List<Vector2> keyPoints)
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