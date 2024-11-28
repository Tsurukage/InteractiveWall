using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallTargetCar : MonoBehaviour
{
    [SerializeField] private GameObject targetCar;

    public void CallCar(List<Vector3> points)
    {
        var pathMover = targetCar.GetComponent<PathMover>();
        pathMover.SetPoint(points);
    }
}
