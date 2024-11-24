using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utls;

public class GridManager : MonoBehaviour
{
    public Grid Grid;
    public GridCell cellPrefab;
    public Transform Content;
    List<Vector2Int> gridCells = new();
    List<GridCell> path = new();
    void Start()
    {
        CreateGrid();
    }
    [Button]public void CreateGrid()
    {
        // 获取网格单元的数量
        var cellCountX = Grid.cellSize.x;
        var cellCountZ = Grid.cellSize.z;

        if (cellCountX <= 0 || cellCountZ <= 0)
        {
            Debug.LogError("Grid cell count must be greater than zero.");
            return;
        }

        gridCells.Clear();

        // 生成网格单元
        for (var x = 0; x < cellCountX; x++)
        {
            for (var z = 0; z < cellCountZ; z++)
            {
                var pos = new Vector2Int(x, z);
                gridCells.Add(pos);
            }
        }
        Debug.Log("网格生成完成！");
    }
    [Button]public void GenGrid()
    {
        // Calculate the plane's actual size, considering scaling
        // 获取 Renderer 组件
        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("没有找到 Renderer 组件！");
            return;
        }
        // 获取网格单元的数量
        var cellCountX = Grid.cellSize.x;
        var cellCountZ = Grid.cellSize.z;

        if (cellCountX <= 0 || cellCountZ <= 0)
        {
            Debug.LogError("Grid cell count must be greater than zero.");
            return;
        }
        // 获取世界空间中的平面尺寸
        var planeSize = renderer.bounds.size;

        // 获取平面的中心位置（世界空间）
        var planeCenter = renderer.bounds.center;

        // 计算平面尺寸的一半
        var halfPlaneSize = planeSize * 0.5f;

        // 计算网格原点（左上角）
        var gridOrigin = planeCenter + new Vector3(-halfPlaneSize.x, 0, halfPlaneSize.z);
        // 获取网格单元间的间隙
        var gapX = Grid.cellGap.x;
        var gapZ = Grid.cellGap.z;

        // 计算每个网格单元的实际大小，考虑间隙
        var totalGapX = (cellCountX - 1) * gapX;
        var totalGapZ = (cellCountZ - 1) * gapZ;

        var cellSizeX = (planeSize.x - totalGapX) / cellCountX;
        var cellSizeZ = (planeSize.z - totalGapZ) / cellCountZ;

        var cellSize = new Vector3(cellSizeX, 1, cellSizeZ); // Y轴高度根据需求调整
        foreach (var grid in gridCells)
        {
            // 计算每个单元的位置偏移
            var posX = grid.x * (cellSizeX + gapX);
            var posZ = -grid.y * (cellSizeZ + gapZ); // 负方向沿Z轴
            // 计算单元的世界位置
            var cellPosition = gridOrigin + new Vector3(posX + cellSizeX / 2, 0, posZ - cellSizeZ / 2);
            // 实例化网格单元
            var cellObj = Instantiate(cellPrefab, Content);
            cellObj.transform.position = cellPosition.ChangeY(Content.position.y);
            //cellObj.transform.localScale = new Vector3(1, 1, 1); // 确保单元没有被缩放

            // 初始化单元（假设有 Init 和 SetSize 方法）
            var cell = cellObj.GetComponent<GridCell>();
            if (cell != null)
            {
                cell.Init(grid);
                cell.SetSize(cellSize);
            }
            else
            {
                Debug.LogWarning("GridCell 脚本未找到！");
            }
        }
    }
    [Button]public void RemoveGid()
    {
        // 清除之前的网格单元（如果有）
        foreach (Transform child in Content) Destroy(child.gameObject);
    }
    
    public List<Vector2Int> AdjustPathAngles(List<Vector2Int> path)
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
    }

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
            new(1, 0),  // 右
            new(0, -1), // 下
            new(0, 1),  // 上
            new(-1, -1), // 左下
            new(-1, 1),  // 左上
            new(1, -1),  // 右下
            new(1, 1),   // 右上
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