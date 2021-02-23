using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)] //Execute this script 10ms before all others
public class GridManager : MonoBehaviour
{
    public static GridManager gridInstance;

    [Header("Grid Settings")]
    public int width;
    public int height;
    public float cellSize = 1f;

    [Header("Selection")]
    public float rayLength = 80f;
    public LayerMask raycastMask;

    private GameObject _tileHighlight;

    private Tile[,] _grid;

    public bool IsGridCreated => _grid != null;

    [Header("Gizmos")]
    public float gizmoSize = 2f;
    public Material glMaterial;

    private Camera _camera;
    private RaycastHit[] _hits;

    private void Start()
    {
        CreateInternalGrid(remakeGrid: false);
        _camera = Camera.main;
        _hits = new RaycastHit[1];
        GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlight.name = "Tile Highlight";
        highlight.transform.position = Vector3.zero;
        highlight.transform.localScale = new Vector3(1f, 0.01f, 1f) * cellSize;
        highlight.GetComponent<Renderer>().material.color = Color.green;
        highlight.SetActive(false);
        _tileHighlight = highlight;
    }
    public void CreateInternalGrid(bool remakeGrid)
    {
        if (IsGridCreated && !remakeGrid) return;
        if (gridInstance == null) //Create Singleton
            gridInstance = this;
        else
            Destroy(gameObject);
        _grid = new Tile[width, height];
        GeneratePhysicalGrid();
    }

    private void GeneratePhysicalGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = new Tile() {
                    occupied = false,
                    positionOnGrid = new Vector2Int(x, y),
                    worldPosition = transform.position + new Vector3((x * cellSize) + cellSize / 2f, 0f, (y * cellSize) + cellSize / 2f)
                };
                _grid[x, y] = tile;
            }
        }
    }

    public Tile FindTileAtPosition(Vector3 position) {
        int x = (int)transform.InverseTransformPoint(position).x;
        int y = (int)transform.InverseTransformPoint(position).z;
        if (x >= width || y >= height || x < 0 || y < 0) return _grid[0, 0];
        return _grid[x, y];
    }

    private void OnDrawGizmos()
    {
        if (!IsGridCreated) return;
        Vector3 scale = new Vector3(1f, 0f, 1f) * cellSize;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(_grid[x, y].worldPosition, scale);
            }
        }
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.RaycastNonAlloc(ray, _hits, rayLength, raycastMask) > 0)
        {
            Debug.DrawRay(ray.origin, ray.direction * _hits[0].distance, Color.green);
            _tileHighlight.transform.position = FindTileAtPosition(_hits[0].point).worldPosition;
            _tileHighlight.SetActive(true);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);
            _tileHighlight.SetActive(false);
        }
    }
    public static float MapToRange(float value, float start1, float stop1, float start2, float stop2) => start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
}

public struct Tile {
    public bool occupied;
    public Vector2Int positionOnGrid;
    public Vector3 worldPosition;
}