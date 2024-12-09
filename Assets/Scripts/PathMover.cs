using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utls;

public class PathMover : MonoBehaviour
{
    Coroutine moveCoroutine;
    [SerializeField] float moveSpeed = 3.5f;
    [SerializeField] float rotationSpeed = 5f;
    List<Vector3> currentPath;
    bool isMoving;
    bool isCircle;

    public void SetPoint(List<Vector3> path, bool isCircle)
    {
        if (path == null || path.Count < 2)
        {
            "drawPoint not enough, unable to move".Log(this, LogType.Warning);
            return;
        }

        this.isCircle = isCircle;
        currentPath = path;
        transform.position = currentPath[0]; // start point
        transform.LookAt(currentPath[1]); // look at next point

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveAlongPath());
    }

    IEnumerator MoveAlongPath()
    {
        isMoving = true;
        var index = 1; // start from second point
        var direction = 1; // 1: forward，-1: backward
        var iterationCount = 0; // limit on iteration
        while (isMoving)
        {
            var targetPoint = currentPath[index];
            while (Vector3.Distance(transform.position, targetPoint) > 0.1f)
            {
                // cal direction
                var directionToTarget = (targetPoint - transform.position).normalized;

                // move
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;

                // rotate
                var targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                yield return null;
            }

            // update index
            index += direction;
            if (index >= currentPath.Count || index < 0)
            {
                if (isCircle)
                {
                    index = (index + currentPath.Count) % currentPath.Count;
                }
                else
                {
                    direction *= -1; // backward
                    index += direction * 2; // adjust index
                }
            }
            
            iterationCount++;
            if (iterationCount >= 9999)
            {
                // stop after 9999 iterations
                isMoving = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.green;
            for (var i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }
}

//public class PathMover : MonoBehaviour
//{
//    public NavMeshAgent navmeshagent;
//    [SerializeField] NavMeshSurface navMeshSurface;
//    Coroutine moveCoroutine;
//    [SerializeField] float roadOffSet = 0.7f;
//    [SerializeField] float rotationSpeed = 5f;

//    void Awake()
//    {
//        if (NavMesh.SamplePosition(transform.position, out var hit, 1.0f, NavMesh.AllAreas))
//        {
//            navmeshagent.Warp(hit.position);
//        }
//        // 禁用自动旋转
//        navmeshagent.updateRotation = false;
//    }

//    //public void SetPoint(List<Vector3> path, bool isCircle)
//    //{
//    //    navmeshagent.ResetPath();
//    //    var startPoint = path.First();
//    //    //if (navmeshagent.agentTypeID == navMeshSurface.agentTypeID)
//    //    navmeshagent.Warp(startPoint);
//    //    if (path.Count < 2) return;
//    //    transform.LookAt(path[1]);
//    //    if (moveCoroutine != null) StopCoroutine(moveCoroutine);
//    //    moveCoroutine = StartCoroutine(MoveToPath(path, isCircle));
//    //}
//    public void SetPoint(List<Vector3> path, bool isCircle)
//    {
//        if (!navmeshagent.enabled)
//        {
//            "NavMeshAgent is not set!".Log(this, LogType.Warning);
//            return;
//        }

//        navmeshagent.ResetPath();
//        var startPoint = path.First();

//        NavMeshHit hit;
//        if (NavMesh.SamplePosition(startPoint, out hit, 10.0f, NavMesh.AllAreas))
//        {
//            navmeshagent.Warp(hit.position);
//        }
//        else
//        {
//            "unable to warp: start point isn't on NavMesh.".Log(this, LogType.Warning);
//            navmeshagent.enabled = false;
//            return;
//        }

//        if (path.Count < 2) return;
//        transform.LookAt(path[1]);
//        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
//        moveCoroutine = StartCoroutine(MoveToPath(path, isCircle));
//    }

//    IEnumerator MoveToPath(IList<Vector3> path, bool isCircle)
//    {
//        var points = path.ToList();
//        while (true)
//        {
//            for (int i = 0; i < points.Count; i++)
//            { 
//                var nextPoint = points[i];
//                navmeshagent.SetDestination(nextPoint);
//                yield return new WaitUntil(() => !navmeshagent.pathPending); // 等待路径计算完成
//                yield return new WaitUntil(() => navmeshagent.remainingDistance <= 0.5f);
//            }
//            if (!isCircle) points.Reverse();
//        }
//        moveCoroutine = null;
//    }

//    void Update()
//    {
//        // 手动更新车辆的朝向
//        if (navmeshagent.velocity.sqrMagnitude > 0.1f)
//        {
//            Quaternion targetRotation = Quaternion.LookRotation(navmeshagent.velocity.normalized);
//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
//        }
//    }

//    void OnDrawGizmos()
//    {
//        if (navmeshagent != null && navmeshagent.hasPath)
//        {
//            Gizmos.color = Color.green;
//            var path = navmeshagent.path;
//            for (int i = 0; i < path.corners.Length - 1; i++)
//            {
//                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
//            }
//        }
//    }
//}
