using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadCircuit : MonoBehaviour
{
    public RoadGenerator RoadGenerator { get; private set; }
    public PathMover PathMover { get; private set; }
    public List<Vector3> Path { get; set; }
    public void Init(RoadGenerator generator) => RoadGenerator = generator;

    void Start()
    {
        PathMover = FindAnyObjectByType<PathMover>();
    }

    public void OnDrawEnd(DrawingBoardInfo info)
    {
        Path = info.GetCoordinatePath(transform);
        RoadGenerator.GenerateRoad(info.Index, Path);
        PathMover.SetPoint(Path);
    }
}