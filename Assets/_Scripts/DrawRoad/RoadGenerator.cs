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
    public ERRoad[] roads;
    
    public void Init(int roadCount)
    {
        // 定义道路类型
        roadType = new ERRoadType();
        roadType.roadWidth = roadWidth;
        roadType.roadMaterial = roadMaterial;
        roads = new ERRoad[roadCount];
        RoadNetwork ??= new ERRoadNetwork();
    }

    public void RemoveRoad(int index)
    {
        var road = roads[index];
        if (road == null) return;
        Destroy(road.gameObject);
        roads[index] = null;
    }

    public ERRoad GenerateRoad(int index, List<Vector3> path)
    {
        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("路径点数量不足，无法生成道路。");
            return null;
        }
        // 移除现有的道路
        RemoveRoad(index);
        // 创建新道路
        roads[index] = RoadNetwork.CreateRoad("Road_" + RoadId, roadType, path.ToArray());
        return roads[index];
    }
}