using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EasyRoads3Dv3;
using Sirenix.OdinInspector;
using Utls;

public class RoadGenerator : MonoBehaviour
{
    public Material roadMaterial;

    [OnStateUpdate(nameof(RoadTypeUpdate))]
    public float roadWidth = 8f;

    void RoadTypeUpdate()
    {
        if (roadType == null) return;
        roadType.roadWidth = roadWidth;
        roadType.roadMaterial = roadMaterial;
    }

    public int RoadId = -1;
    public static ERRoadNetwork RoadNetwork { get; set; }
    public ERRoadType roadType;
    public ERRoad road;
    public event Action<DrawingBoardInfo> OnRoadGenerated;

    void Start()
    {
        // 定义道路类型
        roadType = new ERRoadType();
        roadType.roadWidth = roadWidth;
        roadType.roadMaterial = roadMaterial;
        RoadNetwork ??= new ERRoadNetwork();
    }

    public void RemoveRoad()
    {
        if (road != null)
        {
            Destroy(road.gameObject);
            road = null;
        }
    }

    public ERRoad GenerateRoad(List<Vector3> path)
    {
        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("路径点数量不足，无法生成道路。");
            return null;
        }
        // 移除现有的道路
        RemoveRoad();
        // 创建新道路
        road = RoadNetwork.CreateRoad("Road_" + RoadId, roadType, path.ToArray());
        return road;
        road.gameObject.transform.localPosition = Vector3.zero;
        // 构建道路网络
        RoadNetwork.BuildRoadNetwork();
    }
}