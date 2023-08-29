using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int _row, _col;
    [SerializeField] private Level _level;
    [SerializeField] private Edge _edgePrefab;
    [SerializeField] private Point _pointPrefab;

    private static int spawnId;

    private Dictionary<int, Point> points;
    private Dictionary<Vector2Int, Edge> edges;
    private Point startPoint, endPoint;
    private int currentId;
    private Point startSpawnPoint, endSpawnPoint;

    private void Awake()
    {
        points = new Dictionary<int, Point>();
        edges = new Dictionary<Vector2Int, Edge>();
        currentId = -1;
        CreateLevel();
        SpawnLevel();
    }

    private void CreateLevel()
    {
        if (_level.Row == _row && _level.Col == _col)
        {
            return;
        }

        _level.Row = _row;
        _level.Col = _col;
        _level.Points = new List<Vector4>();
        _level.Edges = new List<Vector2Int>();
        spawnId = 0;
    }


    private void SpawnLevel()
    {
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Col * 0.5f;
        camPos.y = _level.Row * 0.5f;
        Camera.main.transform.position = camPos;
        Camera.main.orthographicSize = Mathf.Max(_level.Col, _level.Row) + 2f;

        for (int i = 0; i < _level.Points.Count; i++)
        {
            Vector4 posData = _level.Points[i];
            Vector3 spawnPos = new Vector3(posData.x, posData.y, posData.z);
            int id = (int)posData.w;
            points[id] = Instantiate(_pointPrefab);
            points[id].Init(spawnPos, id);
            spawnId = id + 1;
        }

        for (int i = 0; i < _level.Edges.Count; i++)
        {
            Vector2Int normal = _level.Edges[i];
            Vector2Int reversed = new Vector2Int(normal.y, normal.x);
            Edge spawnEdge = Instantiate(_edgePrefab);
            edges[normal] = spawnEdge;
            edges[reversed] = spawnEdge;
            spawnEdge.Init(points[normal.x].Position, points[normal.y].Position);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit) return;
            startPoint = hit.collider.gameObject.GetComponent<Point>();
        }
        else if (Input.GetMouseButton(0) && startPoint != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit)
            {
                endPoint = hit.collider.gameObject.GetComponent<Point>();
            }
            if (startPoint == endPoint || endPoint == null) return;
            if (IsStartAdd())
            {
                currentId = endPoint.Id;
                edges[new Vector2Int(startPoint.Id, endPoint.Id)].Add();
                startPoint = endPoint;
            }
            else if (IsEndAdd())
            {
                currentId = endPoint.Id;
                edges[new Vector2Int(startPoint.Id, endPoint.Id)].Add();
                startPoint = endPoint;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            startPoint = null;
            endPoint = null;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit) return;
            int id = spawnId++;
            Vector3 spawnPos = new Vector3(mousePos.x, mousePos.y, 0);
            Vector4 point = new Vector4(mousePos.x, mousePos.y, 0, id);
            _level.Points.Add(point);
            points[id] = Instantiate(_pointPrefab);
            points[id].Init(spawnPos, id);
            EditorUtility.SetDirty(_level);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit) return;
            startSpawnPoint = hit.collider.gameObject.GetComponent<Point>();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit) return;
            endSpawnPoint = hit.collider.gameObject.GetComponent<Point>();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (startSpawnPoint == null || endSpawnPoint == null) return;
            if (startSpawnPoint.Id == endSpawnPoint.Id) return;
            Vector2Int normal = new Vector2Int(startSpawnPoint.Id, endSpawnPoint.Id);
            Vector2Int reversed = new Vector2Int(normal.y, normal.x);
            if (edges.ContainsKey(normal)) return;
            Edge spawnEdge = Instantiate(_edgePrefab);
            edges[normal] = spawnEdge;
            edges[reversed] = spawnEdge;
            spawnEdge.Init(points[normal.x].Position, points[normal.y].Position);
            _level.Edges.Add(normal);
            EditorUtility.SetDirty(_level);
        }


    }

    private bool IsStartAdd()
    {
        if (currentId != -1) return false;
        Vector2Int edge = new Vector2Int(startPoint.Id, endPoint.Id);
        if (!edges.ContainsKey(edge)) return false;
        return true;
    }

    private bool IsEndAdd()
    {
        if (currentId != startPoint.Id)
        {
            return false;
        }

        Vector2Int edge = new Vector2Int(endPoint.Id, startPoint.Id);
        if (edges.TryGetValue(edge, out Edge result))
        {
            if (result == null || result.Filled) return false;
        }
        else
        {
            return false;
        }
        return true;
    }
}
