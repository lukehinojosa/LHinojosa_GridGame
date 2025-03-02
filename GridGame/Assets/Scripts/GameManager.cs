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
        int addNumRow = 0;
        int addNumCol = 0;
        int startingPosition = 0;
        int outsideMax = 0;
        int insideMax = 0;
        bool horizontal = true;

        switch (direction)
        {
            case KeyCode.LeftArrow:
            {
                addNumCol = -1;
                startingPosition = 1;
                outsideMax = _gridManager._rows;
                insideMax = _gridManager._columns;
                break;
            }
            case KeyCode.RightArrow:
            {
                addNumCol = 1;
                startingPosition = _gridManager._columns - 2;
                outsideMax = _gridManager._rows;
                insideMax = _gridManager._columns;
                break;
            }
            case KeyCode.DownArrow:
            {
                addNumRow = -1;
                startingPosition = 1;
                outsideMax = _gridManager._columns;
                insideMax = _gridManager._rows;
                horizontal = false;
                break;
            }
            case KeyCode.UpArrow:
            {
                addNumRow = 1;
                startingPosition = _gridManager._rows - 2;
                outsideMax = _gridManager._columns;
                insideMax = _gridManager._rows;
                horizontal = false;
                break;
            }
        }
        
        for (int outside = 0; outside < outsideMax; outside++)
        {
            bool blocksMoved = true;

            while (blocksMoved)
            {
                blocksMoved = false;

                int addNum;
                if (horizontal)
                    addNum = addNumCol;
                else
                    addNum = addNumRow;

                int size = 1;
                for (int inside = startingPosition; size < insideMax; inside -= addNum)
                {
                    int row;
                    int column;

                    if (horizontal)
                    {
                        row = outside;
                        column = inside;
                    }
                    else
                    {
                        row = inside;
                        column = outside;
                    }
                    
                    size++;
                    
                    if (_objects[row][column] != null)
                    {
                        if (_objects[row + addNumRow][column + addNumCol] == null)
                        {
                            blocksMoved = true;
                            MoveBlockToBlank(row, column, addNumRow, addNumCol);
                        }
                        else if (!_objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._combined &&
                                 _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().GetBlockNumber() ==
                                 _objects[row][column].GetComponent<BlockObject>().GetBlockNumber())
                        {
                            blocksMoved = true;
                            _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._combined = true;
                            Combine(row, column, addNumRow, addNumCol);
                        }
                    }

                    if (size >= insideMax)
                    {
                        for (int i = 0; i < _gridManager._rows; i++)
                            for (int j = 0; j < _gridManager._columns; j++)
                                if (_objects[i][j] != null)
                                    _objects[i][j].GetComponent<BlockObject>()._combined = false;
                    }
                }
            }
        }
    }
    
    void MoveBlockToBlank(int row, int column, int addNumRow, int addNumCol)
    {
        _objects[row + addNumRow][column + addNumCol] = _objects[row][column];
        
        if (addNumRow != 0)
            _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._gridPos.y += addNumRow;
        else
            _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._gridPos.x += addNumCol;

        _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().UpdatePosition();
        _objects[row][column] = null;
    }
    
    void Combine(int row, int column, int addNumRow, int addNumCol)
    {
        _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().DoubleNumber();
        _score += _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().GetBlockNumber();
        Destroy(_objects[row][column]);
        _objects[row][column] = null; //Destroy is not quick enough
        _objectCount--;
    }
}