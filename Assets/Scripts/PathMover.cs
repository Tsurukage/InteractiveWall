using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathMover : MonoBehaviour
{
    private NavMeshAgent navmeshagent;
    private Queue<Vector3> pathPoints = new Queue<Vector3>();
    //public Vector3 destination;
    [SerializeField]
    private float roadOffSet = 0.7f;
    private void Awake()
    {
        //navmeshagent = GetComponent<NavMeshAgent>();
        ////FindAnyObjectByType<PathCreator>().OnNewPathCreated += SetPoints;
        //FindAnyObjectByType<RoadDrawer>().OnNewPathGenerated += SetPoints;
    }

    private void SetPoints(IEnumerable<Vector3> points)
    {
        pathPoints = new Queue<Vector3>(points);
    }
    private void Start()
    {
        if (navmeshagent == null)
            Debug.LogError("NavMeshAgent not found.");

        //var pathCreator = FindAnyObjectByType<PathCreator>();
        var pathGenerator = FindAnyObjectByType<RoadDrawer>();
        if (pathGenerator == null)
            Debug.LogError("PathCreator not found.");
        else
            Debug.Log("PathMover successfully subscribed to PathCreator.");
    }


    private void Update()
    {
        //navmeshagent.SetDestination(destination);
        UpdatePathing();
    }

    private void UpdatePathing()
    {
        if (ShouldSetDestination())
        {
            Vector3 targetPoint = pathPoints.Dequeue();

            Vector3 direction = pathPoints.Count > 0
                ? (pathPoints.Peek() - targetPoint).normalized
                : navmeshagent.transform.forward;

            Vector3 offset = Vector3.Cross(direction, Vector3.up) * roadOffSet;

            Vector3 adjustedTagert = targetPoint + offset;

            navmeshagent.SetDestination(adjustedTagert);
        }
            //navmeshagent.SetDestination(pathPoints.Dequeue());
    }

    private bool ShouldSetDestination()
    {
        if (pathPoints.Count == 0)
            return false;

        if(navmeshagent.hasPath == false || navmeshagent.remainingDistance < 0.5f)
            return true;

        return false;
    }
}
