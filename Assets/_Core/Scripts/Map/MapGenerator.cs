using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : Singleton<MapGenerator>
{

    [SerializeField] private int size = 3;
    [SerializeField] private Transform container;
    [SerializeField] private Cell emptyCellPrefab;
    [SerializeField] private Face planePrefab;
    [SerializeField] private float rotateDuration = 0.5f; // thời gian xoay 90°

    private bool isRotate = false;
    private Transform pivotX, pivotY, pivotZ;



    public Cell[,,] map;
    private Player player;

    void Start()
    {
        // tạo pivot object cho 3 trục
        pivotX = new GameObject("X").transform;
        pivotY = new GameObject("Y").transform;
        pivotZ = new GameObject("Z").transform;

        pivotX.SetParent(container);
        pivotY.SetParent(container);
        pivotZ.SetParent(container);

        pivotX.localPosition = Vector3.zero;
        pivotY.localPosition = Vector3.zero;
        pivotZ.localPosition = Vector3.zero;

        map = new Cell[size, size, size];
        CreateMap();

        // tạo player ở mặt top giữa (1,2,1) nếu size=3
        Vector3Int startCell = new Vector3Int((size - 1) / 2, size - 1, (size - 1) / 2); // (1,2,1)
        Cell cell = map[startCell.x, startCell.y, startCell.z];
        player = new Player(startCell, cell.GetFace(Vector3.up));
        player.Highlight(Color.red);
    }

    public void OnFaceClicked(Face face)
    {
        Cell targetCell = face.GetCell();
        Vector3 targetNormal = face.GetNormal();
        Vector3Int targetPos = WorldToIndex(targetCell.transform.localPosition);

        // 1. Nếu click cùng cell nhưng khác normal -> đổi mặt
        if (targetPos == player.cellPos && targetNormal != player.facingNormal)
        {
            player.ChangeFace(face, targetNormal);
            return;
        }

        // 2. Nếu đi sang cell khác
        Vector3Int diff = targetPos - player.cellPos;
        int nonZero = (diff.x != 0 ? 1 : 0) + (diff.y != 0 ? 1 : 0) + (diff.z != 0 ? 1 : 0);

        if (nonZero == 1) // đúng 1 trục khác
        {
            // hướng di chuyển
            Vector3Int dir = new Vector3Int(
                diff.x == 0 ? 0 : (diff.x > 0 ? 1 : -1),
                diff.y == 0 ? 0 : (diff.y > 0 ? 1 : -1),
                diff.z == 0 ? 0 : (diff.z > 0 ? 1 : -1)
            );

            // cập nhật facingNormal theo hướng mới
            Vector3 newNormal = dir;

            player.MoveTo(face, targetPos, newNormal);
        }
    }




    private Vector3Int WorldToIndex(Vector3 localPos)
    {
        int center = (size - 1) / 2;
        int xi = Mathf.RoundToInt(localPos.x + center);
        int yi = Mathf.RoundToInt(localPos.y + center);
        int zi = Mathf.RoundToInt(localPos.z + center);
        return new Vector3Int(xi, yi, zi);
    }

    private void CreateMap()
    {
        int offset = (size - 1) / 2;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Cell cell = Instantiate(emptyCellPrefab, container);
                    cell.name = $"Cell {x},{y},{z}";
                    cell.transform.localPosition = new Vector3(x - offset, y - offset, z - offset);

                    // spawn plane nếu cell nằm ở biên
                    if (x == 0) cell.AddFace(Vector3.left, CreatePlane(cell, Vector3.left, "Left"));
                    if (x == size - 1) cell.AddFace(Vector3.right, CreatePlane(cell, Vector3.right, "Right"));

                    if (y == 0) cell.AddFace(Vector3.down, CreatePlane(cell, Vector3.down, "Bottom"));
                    if (y == size - 1) cell.AddFace(Vector3.up, CreatePlane(cell, Vector3.up, "Top"));

                    if (z == 0) cell.AddFace(Vector3.back, CreatePlane(cell, Vector3.back, "Back"));
                    if (z == size - 1) cell.AddFace(Vector3.forward, CreatePlane(cell, Vector3.forward, "Front"));

                    map[x, y, z] = cell;
                }
            }
        }
    }

    private Face CreatePlane(Cell parent, Vector3 normal, string name)
    {
        Face plane = Instantiate(planePrefab, parent.transform);
        plane.name = name;
        plane.transform.localPosition = normal * 0.5f;
        plane.transform.rotation = Quaternion.LookRotation(-normal, Vector3.up);
        return plane;
    }

    void Update()
    {
        if (isRotate) return;
        //Take input to rotate map layer
        if (Input.GetKeyDown(KeyCode.X))
        {
            isRotate = true;
            StartCoroutine(RotateMapLayer('X', 1, true));
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            isRotate = true;
            StartCoroutine(RotateMapLayer('Y', 1, true));
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            isRotate = true;
            StartCoroutine(RotateMapLayer('Z', 1, true));
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Face face = hit.collider.GetComponent<Face>();
                if (face != null)
                {
                    OnFaceClicked(face);
                    Debug.Log(face.name);
                }
            }
        }
    }

    private IEnumerator RotateMapLayer(char axis, int layer, bool clockwise = true)
    {
        int n = size;
        int center = (n - 1) / 2;

        Transform pivot = null;
        switch (axis)
        {
            case 'X': case 'x': pivot = pivotX; break;
            case 'Y': case 'y': pivot = pivotY; break;
            case 'Z': case 'z': pivot = pivotZ; break;
        }
        if (pivot == null) yield break;

        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;

        List<Cell> layerCubes = new List<Cell>();

        switch (axis)
        {
            case 'X':
            case 'x':
                for (int y = 0; y < n; y++)
                    for (int z = 0; z < n; z++)
                    {
                        var cube = map[layer, y, z];
                        cube.transform.SetParent(pivot, true);
                        layerCubes.Add(cube);
                    }
                break;

            case 'Y':
            case 'y':
                for (int x = 0; x < n; x++)
                    for (int z = 0; z < n; z++)
                    {
                        var cube = map[x, layer, z];
                        cube.transform.SetParent(pivot, true);
                        layerCubes.Add(cube);
                    }
                break;

            case 'Z':
            case 'z':
                for (int x = 0; x < n; x++)
                    for (int y = 0; y < n; y++)
                    {
                        var cube = map[x, y, layer];
                        cube.transform.SetParent(pivot, true);
                        layerCubes.Add(cube);
                    }
                break;
        }

        // Xoay pivot từ 0 -> 90 độ
        int dir = clockwise ? 90 : -90;
        Vector3 axisVec = Vector3.zero;
        switch (axis)
        {
            case 'X': case 'x': axisVec = Vector3.right; break;
            case 'Y': case 'y': axisVec = Vector3.up; break;
            case 'Z': case 'z': axisVec = Vector3.forward; break;
        }

        Quaternion startRot = pivot.localRotation;
        Quaternion endRot = startRot * Quaternion.AngleAxis(dir, axisVec);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / rotateDuration;
            pivot.localRotation = Quaternion.Slerp(startRot, endRot, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        pivot.localRotation = endRot;

        // Bỏ parent ra lại container + cập nhật map
        foreach (var cube in layerCubes)
        {
            cube.transform.SetParent(container, true);

            Vector3 pos = cube.transform.localPosition;
            int xi = Mathf.RoundToInt(pos.x + center);
            int yi = Mathf.RoundToInt(pos.y + center);
            int zi = Mathf.RoundToInt(pos.z + center);

            // Gán lại localPosition chính xác
            cube.transform.localPosition = new Vector3(xi - center, yi - center, zi - center);

            map[xi, yi, zi] = cube;
        }
        isRotate = false;
    }
}


