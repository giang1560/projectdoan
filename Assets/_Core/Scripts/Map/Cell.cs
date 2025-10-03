using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private Dictionary<Vector3, Face> faces = new Dictionary<Vector3, Face>();

    public void AddFace(Vector3 dir, Face face)
    {
        faces.Add(dir, face);
        face.Init(this, dir);
    }

    public Face GetFace(Vector3 dir) => faces.TryGetValue(dir, out var face) ? face : null;
}

public class PlayerOnCube
{
    public Vector3Int cellPos;       // tọa độ trong map
    public Vector3 planeNormal;      // hướng mặt (normal)

    public Cell GetCell(Cell[,,] map) => map[cellPos.x, cellPos.y, cellPos.z];
    public Face GetFace(Cell[,,] map)
        => GetCell(map).GetFace(planeNormal);
}

public class Player
{
    public Vector3Int cellPos;
    private Face currentFace;
    public Vector3 facingNormal; // hướng mặt hiện tại

    public Player(Vector3Int startCell, Face startFace)
    {
        cellPos = startCell;
        currentFace = startFace;
        Highlight(Color.red);
    }

    public void Highlight(Color color)
    {
        if (currentFace != null)
        {
            currentFace.GetComponent<Renderer>().material.color = color;
        }
    }

    public void ChangeFace(Face newFace, Vector3 newNormal)
{
    if (currentFace != null)
        currentFace.GetComponent<Renderer>().material.color = Color.white;

    currentFace = newFace;
    facingNormal = newNormal;

    Highlight(Color.red);
}

public void MoveTo(Face newFace, Vector3Int newPos, Vector3 newNormal)
{
    if (currentFace != null)
        currentFace.GetComponent<Renderer>().material.color = Color.white;

    currentFace = newFace;
    cellPos = newPos;
    facingNormal = newNormal;

    Highlight(Color.red);
}
}