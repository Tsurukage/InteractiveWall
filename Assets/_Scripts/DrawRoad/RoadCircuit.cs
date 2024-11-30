using System.Collections.Generic;
using System.Linq;
using GMVC.Core;
using UnityEngine;
using Utls;

public class RoadCircuit : MonoBehaviour
{
    public RoadGenerator RoadGenerator { get; private set; }
    public LineRenderer LineRenderer;
    public CallTargetCar CarCalled;
    public List<Vector3> Path { get; set; }
    public void Init(RoadGenerator generator) => RoadGenerator = generator;

    public void OnDrawEnd(DrawingBoardInfo info)
    {
        Path = info.GetCoordinatePath(transform);
        if (!App.Setting.CircuitIsRoad)
        {
            LineRenderer.positionCount = Path.Count;
            LineRenderer.SetPositions(Path.Select(p => p.ChangeY(p.y + 0.1f)).ToArray());
        }
        else RoadGenerator.GenerateRoad(info.Index, Path);
        CarCalled.CallCar(Path);
    }
}