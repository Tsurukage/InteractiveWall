using UnityEngine;
using Utls;

public class GridCell : MonoBehaviour
{
    public CellCollider cellOn;
    public CellCollider cellOff;
    public Vector2Int GridPosition => transform.position.ToXZInt();

    public void Init(Vector2Int grid)
    {
        gameObject.SetActive(true);
        Activate(false);
        name = grid.ToString();
    }

    public void SetSize(Vector3 size)
    {
        var cellSize = cellOff.Mesh.bounds.size;
        transform.localScale = new Vector3(size.x / cellSize.x, 1, size.z / cellSize.z);
    }
    public void SetPosition(float x,float y)
    {
        transform.position = new Vector3(x, transform.position.y, y);
    }
    public void Activate(bool active)
    {
        cellOn.SetActive(active);
        cellOff.SetActive(!active);
    }
}