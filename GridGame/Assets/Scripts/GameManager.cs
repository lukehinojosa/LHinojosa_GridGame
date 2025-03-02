using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject _objectTwo;
    [SerializeField] private GameObject _objectFour;
    private GameObject[][] _objects;
    private int _objectCount = 0;
    private Vector3 _instantiatePosition = new Vector3(50f, 20f, 0f);

    void Start()
    {
        _objects = new GameObject[_gridManager._rows][];
        for (int i = 0; i < _gridManager._rows; i++)
            _objects[i] = new GameObject[_gridManager._columns];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnTile();
        }
    }

    public void SpawnTile()
    {
        if (_objectCount < _gridManager._rows * _gridManager._columns)
        {
            GameObject block;
            if (Random.Range(0f, 100f) > 10f)
                block = Instantiate(_objectTwo, _instantiatePosition, Quaternion.identity);
            else
                block = Instantiate(_objectFour, _instantiatePosition, Quaternion.identity);

            int randomRow = Random.Range(0, _gridManager._rows);
            int randomColumn = Random.Range(0, _gridManager._columns);
            while (_objects[randomRow][randomColumn] != null)
            {
                randomRow = Random.Range(0, _gridManager._rows);
                randomColumn = Random.Range(0, _gridManager._columns);
            }

            GridMovement gm = block.GetComponent<GridMovement>();
            gm._gridPos = new Vector2Int(randomRow, randomColumn);
            _objects[randomRow][randomColumn] = block;
            _objectCount++;
        }
    }
}
