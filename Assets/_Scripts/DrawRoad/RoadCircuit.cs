using System.Collections.Generic;
using System.Linq;
using GMVC.Core;
using GMVC.Utls;
using Unity.AI.Navigation;
using UnityEngine;
using Utls;

public class RoadCircuit : MonoBehaviour
{
    public RoadGenerator RoadGenerator { get; private set; }
    public NavMeshSurface MeshSurface;
    public LineRenderer LineRenderer;
    public PathMover CarCalled;
    public List<Vector3> Path { get; set; } = new();
    public void Init(RoadGenerator generator) => RoadGenerator = generator;

    public void OnDrawEnd(DrawingBoardInfo info)
    {
        var path = info.GetCoordinatePath(transform);
        LineRenderer.positionCount = path.Count;
        LineRenderer.SetPositions(path.Select(p => p.ChangeY(p.y + 0.1f)).ToArray());
        LineRenderer.Simplify(0.3f);
        LineRenderer.Display(!App.Setting.CircuitIsRoad);
        Path.Clear();
        for (var i = 0; i < LineRenderer.positionCount; i++) 
            Path.Add(LineRenderer.GetPosition(i));
        if (App.Setting.CircuitIsRoad) RoadGenerator.GenerateRoad(info.Index, Path);
        CarCalled.transform.SetParent(MeshSurface.transform);
        CarCalled.transform.localPosition = CarCalled.transform.localPosition.ChangeY(0);
        CarCalled.SetPoint(Path,info.IsCycle);
    }
}