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
    private int _score = 0;

    void Start()
    {
        _objects = new GameObject[_gridManager._rows][];
        for (int i = 0; i < _gridManager._rows; i++)
            _objects[i] = new GameObject[_gridManager._columns];
    }

    void Update()
    {
        MoveInput();
        
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

            BlockObject bo = block.GetComponent<BlockObject>();
            bo._gridPos = new Vector2Int(randomColumn, randomRow);
            _objects[randomRow][randomColumn] = block;
            _objectCount++;
        }
    }
    
    void MoveInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(KeyCode.LeftArrow);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(KeyCode.RightArrow);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            Move(KeyCode.DownArrow);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            Move(KeyCode.UpArrow);
    }

    void Move(KeyCode direction)
    {
        switch (direction)
        {
            case KeyCode.LeftArrow:
            {
                CaseLeft();

                break;
            }
        }
    }

    void CaseLeft()
    {
        for (int row = 0; row < _gridManager._rows; row++)
        {
            bool blocksMoved = true;

            while (blocksMoved)
            {
                blocksMoved = false;
                
                for (int column = 1; column < _gridManager._columns; column++)
                {
                    if (_objects[row][column] != null)
                    {
                        if (_objects[row][column - 1] == null)
                        {
                            blocksMoved = true;
                            MoveBlockToLeftBlank(row, column);
                        }
                        else if (!_objects[row][column - 1].GetComponent<BlockObject>()._combined &&
                                 _objects[row][column - 1].GetComponent<BlockObject>().GetBlockNumber() ==
                                 _objects[row][column].GetComponent<BlockObject>().GetBlockNumber())
                        {
                            blocksMoved = true;
                            _objects[row][column - 1].GetComponent<BlockObject>()._combined = true;
                            CombineRightWithLeft(row, column);
                        }
                    }
                }
            }
            for (int column = 0; column < _gridManager._columns - 1; column++)
                if (_objects[row][column] != null)
                    _objects[row][column].GetComponent<BlockObject>()._combined = false;
        }
    }
    
    void MoveBlockToLeftBlank(int row, int column)
    {
        _objects[row][column - 1] = _objects[row][column];
        _objects[row][column - 1].GetComponent<BlockObject>()._gridPos.x--;

        _objects[row][column - 1].GetComponent<BlockObject>().UpdatePosition();
        _objects[row][column] = null;
    }
    
    void CombineRightWithLeft(int row, int column)
    {
        _objects[row][column - 1].GetComponent<BlockObject>().DoubleNumber();
        _score += _objects[row][column - 1].GetComponent<BlockObject>().GetBlockNumber();
        Destroy(_objects[row][column]);
        _objects[row][column] = null; //Destroy is not quick enough
        _objectCount--;
    }
}