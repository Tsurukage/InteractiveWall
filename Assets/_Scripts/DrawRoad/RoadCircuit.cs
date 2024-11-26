using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;

public class RoadCircuit : MonoBehaviour
{
    public RoadGenerator RoadGenerator;
    public List<Vector3> Path { get; set; }
    public void OnDrawEnd(DrawingBoardInfo info)
    {
        Path = info.GetCoordinatePath(transform);
        RoadGenerator.GenerateRoad(Path);
    }
}