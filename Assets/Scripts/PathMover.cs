using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PathMover : MonoBehaviour
{
    private NavMeshAgent navmeshagent;
    public Queue<Vector3> pathPoints = new Queue<Vector3>();
    public List<Vector3> originalPath = new List<Vector3>();

    [SerializeField]
    private float roadOffSet = 0.7f;
    private void Awake()
    {
        navmeshagent = GetComponent<NavMeshAgent>();
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            navmeshagent.Warp(hit.position);
        }
        //var pathGenerator = FindAnyObjectByType<RoadDrawer>();
        //pathGenerator.OnDrawingEnd += SetPoints;
        //pathGenerator.OnNewPathGenerated += SetPoints;
    }
    public void SetPoint(List<Vector3> path)
    {
        originalPath.Clear();

        foreach (var point in path)
        {
            originalPath.Add(point);
        }
        SetPathPoint(originalPath);
    }

    private void Update()
    {
        UpdatePathing();
    }

    private void UpdatePathing()
    {
        if(originalPath.Count <2)
            return;
        if (ShouldSetNextCheckPoint())
        {
            ApplyNextCheckpoint();
            return;
        }

        if(pathPoints.Any())
            return;
        // If no more points, loop back to the original path
        ProceedFinalization();
    }

    private void ProceedFinalization()
    {
        var firstPos = originalPath.First();
        var lastPos = originalPath.Last();
        var isCircle = Vector3.Distance(lastPos, firstPos) < 1f;
        if (isCircle)
        {
            SetPathPoint(originalPath);
            return;
        }
        //Stop loop for non-circle path
        StopPathMover();
    }
    private void ApplyNextCheckpoint()
    {
        // Set the next destination
        Vector3 nextPoint = pathPoints.Dequeue();
        Vector3 offsetPoint = ApplyOffset(nextPoint, GetNextDirection());
        navmeshagent.SetDestination(offsetPoint);
    }

    void StopPathMover()
    {
        navmeshagent.ResetPath();
        pathPoints.Clear();
    }

    void SetPathPoint(List<Vector3> points)
    {
        StopPathMover();
        foreach (var point in points)
        {
            pathPoints.Enqueue(point);
        }
        Vector3 startPoint = pathPoints.Peek();
        //transform.position = startPoint;
        navmeshagent.Warp(startPoint);
        transform.rotation = GetForwardDirection();
    }

    private Quaternion GetForwardDirection()
    {
        var firstPos = originalPath.First();
        var secondPos = originalPath[1];
        var direction = (secondPos - firstPos).normalized;
        var targetRotation = Quaternion.LookRotation(direction);
        return targetRotation;
    }

    private Vector3 GetNextDirection()
    {
        if (pathPoints.Count > 1)
        {
            Vector3[] pointsArray = pathPoints.ToArray();
            return (pointsArray[1] - pointsArray[0]).normalized;
        }
        return transform.forward;
    }

    private Vector3 ApplyOffset(Vector3 point, Vector3 direction)
    {
        Vector3 offset = Vector3.Cross(direction, Vector3.up).normalized * roadOffSet;
        return point + offset;
    }

    private bool ShouldSetNextCheckPoint()
    {
        var hasPathPoints = pathPoints.Count > 0;
        var hasPath = navmeshagent.hasPath;
        var isClose = navmeshagent.remainingDistance < 0.5f;
        Debug.Log($"hasPath = {hasPath}, distance = {navmeshagent.remainingDistance}");
        return hasPathPoints && (!hasPath || isClose);
    }

}
