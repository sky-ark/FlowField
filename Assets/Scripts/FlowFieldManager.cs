using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Gère le flow field coté Unity en gérant les références de la Tilemap et en faisant le relais vers une classe C# plain
/// </summary>
public class FlowFieldManager : MonoBehaviourSingleton<FlowFieldManager>
{
    private int _width;
    private int _height;
    private float _cellSize = 1f;
    private Vector2 _origin = Vector2.zero;
    private Dictionary<Vector2Int, Cell[,]> _flowFields = new();

    [SerializeField] private GameObject _destinationTest;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _redTile;
    
    private readonly Pathfinder _pathfinder = new Pathfinder();

    private void Awake()
    {
        Test();
    }

    [ContextMenu("Create FlowField Test")]
    private void Test()
    {
        // Récupère les limites de la tilemap
        BoundsInt bounds = _tilemap.cellBounds;
        _width = bounds.size.x;
        _height = bounds.size.y;
        
        _pathfinder.CreateGrid(_width, _height, bounds, _tilemap, _redTile);
        
        Vector2Int cellIndexFromWorldPosition = GetCellIndexFromWorldPosition(_destinationTest.transform.position);
        _pathfinder.GenerateBFS(cellIndexFromWorldPosition, _width, _height);
        
        _pathfinder.GenerateFlowField();
    }
    
    private Vector2Int GetCellIndexFromWorldPosition(Vector3 worldPosition)
    {
        Vector3Int cellPos = _tilemap.WorldToCell(worldPosition);
        int x = cellPos.x - _tilemap.cellBounds.xMin;
        int y = cellPos.y - _tilemap.cellBounds.yMin;
        return new Vector2Int(x, y);
    }

    public Vector2Int GetDirectionAtPosition(Vector2Int position, Vector3 destination)
    {
        // Récupérer cellule vers laquel on flow field (target)
        Vector2Int cellIndexFromWorldPosition = GetCellIndexFromWorldPosition(destination);

        // Récupère les limites de la tilemap
        BoundsInt bounds = _tilemap.cellBounds;
        _width = bounds.size.x;
        _height = bounds.size.y;
        // Est ce qu'il existe un flow field pour destination ?
        // S'il n'existe pas alors on lance le BFS et le flow field puis on le met dans le dictionnaire
        if (!_flowFields.ContainsKey(cellIndexFromWorldPosition))
        {
            Pathfinder pathfinder = new Pathfinder();
            pathfinder.CreateGrid(_width, _height, bounds, _tilemap, _redTile);
            pathfinder.GenerateBFS(cellIndexFromWorldPosition, _width, _height);
            pathfinder.GenerateFlowField();

            _flowFields.Add(cellIndexFromWorldPosition, pathfinder.Matrix);
        }

        // S'il existe un flow field pour la destination alors on le récupère dans le dictionnaire
        Cell[,] flowField = _flowFields[cellIndexFromWorldPosition];

        // Ensuite on renvoie la direction du flowfield en fonction de la position
        Cell cell = flowField[position.x, position.y];
        foreach (Cell cell1 in flowField)
        {
            Debug.Log(cell1.Position + " has as direction: " + cell1.Direction);
        }

        Debug.Log("searching: " + cell.Position + " with direction: " + cell.Direction);
        return cell.Direction;
    }
    
    private void OnDrawGizmos()
    {
        Cell[,] matrix = _pathfinder.Matrix;
        
        if (matrix == null) return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3 pos = matrix[x, y].Position;
                Gizmos.color = matrix[x, y].Valid ? Color.black : Color.red;
                Gizmos.DrawWireCube(pos, Vector3.one * _cellSize);

#if UNITY_EDITOR
                if (matrix[x, y].Cost < int.MaxValue)
                    Handles.Label(pos, matrix[x, y].Cost.ToString());
#endif
                if (matrix[x, y].Valid && matrix[x, y].Cost < int.MaxValue)
                {
                    Vector3 dir = (Vector2)matrix[x, y].Direction;
                    Vector3 start = pos;
                    Vector3 end = pos + dir;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}