using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PathMover : MonoBehaviour
{
    public NavMeshAgent navmeshagent;
    [SerializeField] NavMeshSurface navMeshSurface;
    Coroutine moveCoroutine;
    [SerializeField] float roadOffSet = 0.7f;
    [SerializeField] float rotationSpeed = 5f;

    void Awake()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, 1.0f, NavMesh.AllAreas))
        {
            navmeshagent.Warp(hit.position);
        }
        // 禁用自动旋转
        navmeshagent.updateRotation = false;
    }

    public void SetPoint(List<Vector3> path, bool isCircle)
    {
        navmeshagent.ResetPath();
        var startPoint = path.First();
        if (navmeshagent.agentTypeID == navMeshSurface.agentTypeID)
            navmeshagent.Warp(startPoint);
        if (path.Count < 2) return;
        transform.LookAt(path[1]);
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToPath(path, isCircle));
    }

    IEnumerator MoveToPath(IList<Vector3> path, bool isCircle)
    {
        var points = path.ToList();
        while (true)
        {
            for (int i = 0; i < points.Count; i++)
            { 
                var nextPoint = points[i];
                navmeshagent.SetDestination(nextPoint);
                yield return new WaitUntil(() => !navmeshagent.pathPending); // 等待路径计算完成
                yield return new WaitUntil(() => navmeshagent.remainingDistance <= 0.5f);
            }
            if (!isCircle) points.Reverse();
        }
        moveCoroutine = null;
    }

    void Update()
    {
        // 手动更新车辆的朝向
        if (navmeshagent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(navmeshagent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmos()
    {
        if (navmeshagent != null && navmeshagent.hasPath)
        {
            Gizmos.color = Color.green;
            var path = navmeshagent.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }
    }
}
