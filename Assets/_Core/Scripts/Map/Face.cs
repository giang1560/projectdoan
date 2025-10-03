using UnityEngine;

public class Face : MonoBehaviour
{
    private Cell currentCell;
    private Vector3 dir;
    public void Init(Cell currentCell, Vector3 dir)
    {
        this.currentCell = currentCell;
        this.dir = dir;
    }
    
    // private void OnMouseDown()
    // {
    //     MapGenerator.Instance.OnFaceClicked(currentCell, dir);
    // }

    public Cell GetCell() => currentCell;
    public Vector3 GetNormal() => dir;
}
