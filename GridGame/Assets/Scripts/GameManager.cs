using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject _objectTwo;
    [SerializeField] private GameObject _objectFour;
    private GameObject[][] _objects;
    private int _objectCount = 0;
    private Vector3 _instantiatePosition = new Vector3(50f, 20f, 0f);
    private int _score = 0;
    [SerializeField] private TextMeshProUGUI _scoreText;
    private float _delayTimer = 0f;
    private float _delayTime = 0.15f;
    private bool _moved = true;
    private bool _movesLeft = true;
    [SerializeField] private SpriteRenderer _loseScreen;
    private Tween _fadeTween;
    private Vector2 _startTouchPosition;
    private Vector2 _endTouchPosition;

    void Start()
    {
        _objects = new GameObject[_gridManager._rows][];
        for (int i = 0; i < _gridManager._rows; i++)
            _objects[i] = new GameObject[_gridManager._columns];
        
        SpawnBlock();
    }
    
    private void OnDestroy()
    {
        _fadeTween.Kill();
    }

    void Update()
    {
        if (_movesLeft)
        {
            MoveInput();
            CheckForMoves();
        }
        else
            FadeAnimation();
    }

    private void FadeAnimation()
    {
        if (_fadeTween != null && _fadeTween.IsActive())
            return;

        Sequence seq = DOTween.Sequence();
        seq.Append(_loseScreen.DOFade(0.6f, 0.5f));
        _scoreText.text = "GAME OVER : " + _score;
        _fadeTween = seq;
    }

    public void SpawnBlock()
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
            bo.transform.position = bo.ConvertGridToWorld(bo._gridPos);
            _objects[randomRow][randomColumn] = block;
            _objectCount++;
        }
    }

    private void CheckForMoves()
    {
        _movesLeft = MoveCheck(KeyCode.LeftArrow) || MoveCheck(KeyCode.RightArrow) || MoveCheck(KeyCode.UpArrow) || MoveCheck(KeyCode.DownArrow);
    }
    
    void MoveInput()
    {
        if (_delayTimer <= 0f)
        {
            if (_moved)
            {
                SpawnBlock();
                _moved = false;
            }

            KeyCode direction = TouchControls();

            if (direction != KeyCode.None)
                _moved = Move(direction);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                _moved = Move(KeyCode.LeftArrow);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                _moved = Move(KeyCode.RightArrow);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                _moved = Move(KeyCode.DownArrow);
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                _moved = Move(KeyCode.UpArrow);
            
            if (_moved)
                _delayTimer = _delayTime;
        }
        else
            _delayTimer -= Time.deltaTime;
    }

    private KeyCode TouchControls()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            _startTouchPosition = Input.GetTouch(0).position;
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            _endTouchPosition = Input.GetTouch(0).position;

            if (Mathf.Abs(_endTouchPosition.x - _startTouchPosition.x) >
                Mathf.Abs(_endTouchPosition.y - _startTouchPosition.y))
            {
                if (_endTouchPosition.x < _startTouchPosition.x)
                    return KeyCode.LeftArrow;
                
                if (_endTouchPosition.x > _startTouchPosition.x)
                    return KeyCode.RightArrow;
            }
            
            if (Mathf.Abs(_endTouchPosition.x - _startTouchPosition.x) <
                Mathf.Abs(_endTouchPosition.y - _startTouchPosition.y))
            {
                if (_endTouchPosition.y < _startTouchPosition.y)
                    return KeyCode.DownArrow;
                
                if (_endTouchPosition.y > _startTouchPosition.y)
                    return KeyCode.UpArrow;
            }
        }

        return KeyCode.None;
    }
    
    bool MoveCheck(KeyCode direction)
    {
        bool moved = false;
        
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
                        moved = true;
                    else if (!_objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._combined &&
                             !_objects[row][column].GetComponent<BlockObject>()._combined &&
                             _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().GetBlockNumber() ==
                             _objects[row][column].GetComponent<BlockObject>().GetBlockNumber())
                        moved = true;
                }
            }
        }
        
        return moved;
    }

    bool Move(KeyCode direction)
    {
        bool moved = false;
        
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
                            moved = true;
                            MoveBlockToBlank(row, column, addNumRow, addNumCol);
                        }
                        else if (!_objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._combined &&
                                 !_objects[row][column].GetComponent<BlockObject>()._combined &&
                                 _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().GetBlockNumber() ==
                                 _objects[row][column].GetComponent<BlockObject>().GetBlockNumber())
                        {
                            blocksMoved = true;
                            moved = true;
                            _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._combined = true;
                            Combine(row, column, addNumRow, addNumCol);
                        }
                    }
                }
            }
            
            for (int i = 0; i < _gridManager._rows; i++)
                for (int j = 0; j < _gridManager._columns; j++)
                    if (_objects[i][j] != null)
                        _objects[i][j].GetComponent<BlockObject>()._combined = false;
        }
        
        return moved;
    }
    
    void MoveBlockToBlank(int row, int column, int addNumRow, int addNumCol)
    {
        _objects[row + addNumRow][column + addNumCol] = _objects[row][column];
        
        BlockObject bo = _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>();
        
        if (addNumRow != 0)
            bo._gridPos.y += addNumRow;
        else
            bo._gridPos.x += addNumCol;

        bo._newPosition = bo.ConvertGridToWorld(bo._gridPos);
        _objects[row][column] = null;
    }
    
    void Combine(int row, int column, int addNumRow, int addNumCol)
    {
        MoveAnimation(_objects[row][column].transform.position, _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._gridPos, _objects[row][column].GetComponent<BlockObject>().GetBlockNumber());
        _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().DoubleNumber();
        _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().ScaleAnimation();
        AddScore(_objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().GetBlockNumber());
        Destroy(_objects[row][column]);
        _objects[row][column] = null; //Destroy is not quick enough
        _objectCount--;
    }

    void AddScore(int num)
    {
        _score += num;
        _scoreText.text = _score.ToString();
    }

    private void MoveAnimation(Vector3 startPos, Vector2Int endPos, int blockNumber)
    {
        GameObject block = Instantiate(_objectTwo, startPos, Quaternion.identity);
        BlockObject bo = block.GetComponent<BlockObject>();
        bo.SetNumber(blockNumber);
        bo._newPosition = bo.ConvertGridToWorld(endPos);
        bo._destroy = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}