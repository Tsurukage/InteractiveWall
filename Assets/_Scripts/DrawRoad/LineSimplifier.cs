using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LineSimplifier
{
    public static List<Vector3> RamerDouglasPeucker3D(List<Vector3> points, float tolerance)
    {
        if (points == null || points.Count < 3)
            return points;

        var firstPoint = 0;
        var lastPoint = points.Count - 1;
        var pointIndicesToKeep = new List<int>();

        // Add the first and last indices to the keep list
        pointIndicesToKeep.Add(firstPoint);
        pointIndicesToKeep.Add(lastPoint);

        // Perform the RDP simplification
        SimplifySection(points, firstPoint, lastPoint, tolerance, ref pointIndicesToKeep);

        // Sort the kept indices
        pointIndicesToKeep.Sort();

        // Create the simplified list
        var result = new List<Vector3>();
        foreach (var index in pointIndicesToKeep)
        {
            result.Add(points[index]);
        }

        return result;
    }

    static void SimplifySection(List<Vector3> points, int first, int last, float tolerance, ref List<int> keep)
    {
        if (last <= first + 1)
            return;

        float maxDistance = 0;
        var indexFarthest = 0;

        for (var i = first + 1; i < last; i++)
        {
            var distance = PerpendicularDistance3D(points[first], points[last], points[i]);
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

    static float PerpendicularDistance3D(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        // Compute the area of the parallelogram formed by the line and the point
        var line = lineEnd - lineStart;
        var toPoint = point - lineStart;
        var area = Vector3.Cross(line, toPoint).magnitude;

        // Compute the length of the base (line segment)
        var baseLength = line.magnitude;

        // The height of the parallelogram is the perpendicular distance
        return area / baseLength;
    }


    public static List<Vector3Int> AdjustPathGridAngles3D(List<Vector3Int> path)
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

                var angle = CalculateAngle3D(prevCell, currCell, nextCell);

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

        List<Vector3Int> GetAdjustedCells(Vector3Int prevCell, Vector3Int currCell, Vector3Int nextCell)
        {
            // 获取当前 Cell 的邻居
            var neighbors = GetNeighbors(currCell);

            foreach (var neighbor in neighbors)
            {
                if (neighbor == prevCell || neighbor == nextCell)
                    continue;

                // 检查新的路径是否有更大的角度
                var newAngle = CalculateAngle3D(prevCell, neighbor, nextCell);
                if (newAngle >= minAngleThreshold)
                {
                    // 检查邻居是否与前一个 Cell 相邻，确保路径连续
                    if (AreCellsAdjacent(prevCell, neighbor) && AreCellsAdjacent(neighbor, nextCell))
                    {
                        // 返回替代的路径段
                        return new List<Vector3Int> { neighbor };
                    }
                }
            }

            // 如果没有找到合适的邻居，可以考虑更复杂的替代路径
            return null;
        }

        bool AreCellsAdjacent(Vector3Int cellA, Vector3Int cellB)
        {
            var deltaX = Mathf.Abs(cellA.x - cellB.x);
            var deltaY = Mathf.Abs(cellA.y - cellB.y);
            var deltaZ = Mathf.Abs(cellA.z - cellB.z);

            return (deltaX <= 1 && deltaY <= 1 && deltaZ <= 1 && (deltaX + deltaY + deltaZ) > 0);
        }

        List<Vector3Int> GetNeighbors(Vector3Int pos)
        {
            // 定义邻居的相对位置（包括斜对角）
            var directions = new Vector3Int[]
            {
                new(-1, 0, 0), // 左
                new(1, 0, 0), // 右
                new(0, -1, 0), // 下
                new(0, 1, 0), // 上
                new(0, 0, -1), // 后
                new(0, 0, 1), // 前
                // 斜对角方向
                new(-1, -1, 0), new(-1, 1, 0), new(1, -1, 0), new(1, 1, 0),
                new(-1, 0, -1), new(-1, 0, 1), new(1, 0, -1), new(1, 0, 1),
                new(0, -1, -1), new(0, -1, 1), new(0, 1, -1), new(0, 1, 1),
                new(-1, -1, -1), new(-1, -1, 1), new(-1, 1, -1), new(-1, 1, 1),
                new(1, -1, -1), new(1, -1, 1), new(1, 1, -1), new(1, 1, 1),
            };
            return directions.Select(dir => pos + dir).ToList();
        }

        float CalculateAngle3D(Vector3Int prevCell, Vector3Int currentCell, Vector3Int nextCell)
        {
            var prevDirection = new Vector3(
                currentCell.x - prevCell.x,
                currentCell.y - prevCell.y,
                currentCell.z - prevCell.z
            );

            var nextDirection = new Vector3(
                nextCell.x - currentCell.x,
                nextCell.y - currentCell.y,
                nextCell.z - currentCell.z
            );
            var angle = Vector3.Angle(prevDirection, nextDirection);
            return angle;
        }
    }
}