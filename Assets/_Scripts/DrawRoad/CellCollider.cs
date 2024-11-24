using UnityEngine;

public class CellCollider : MonoBehaviour
{
    public GridCell Grid;
    public MeshRenderer Mesh;
    public void SetActive(bool active) => gameObject.SetActive(active);
}