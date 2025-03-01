using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;

    [SerializeField] public int _rows = 4;
    [SerializeField] public int _columns = 4;
    [SerializeField] private Vector2 _tileSize = Vector2.one;
    [SerializeField] private Vector2 _tilePadding = new Vector2(0.1f, 0.1f);
    
    private List<GameObject> _tiles = new List<GameObject>();
    
    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        _tiles.Capacity = _rows * _columns;
        
        for (int row = 0; row < _rows; row++)
        {
            for (int column = 0; column < _columns; column++)
            {
                Vector3 pos = new Vector3(column * (_tileSize.x + _tilePadding.x), row * (_tileSize.y + _tilePadding.y), 0f);
                GameObject tile = Instantiate(_tilePrefab, pos, Quaternion.identity, transform);
                _tiles.Add(tile);
            }
        }
    }

    public GameObject GetTile(int column, int row)
    {
        //Check if coordinate is valid
        if (column < 0 || row < 0 || column >= _columns || row >= _rows)
        {
            Debug.LogError($"Invalid tile coordinate({column}, {row})");
            return null;
        }

        return _tiles[row * _columns + column];
    }
}
