using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
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

    private Camera _camera;
    private RectTransform _scoreRT;
    private float _initialCameraSize;
    private Vector2 _initialScorePosition;
    [SerializeField] private Transform _wasd;
    [SerializeField] private Transform _udlr;
    private Vector3 _initialWASDPosition;
    private Vector3 _initialUDLRPosition;
    private float _vertCameraSize = 5;
    private Vector2 _vertScorePosition = new Vector2(0f, -200f);
    private Vector3 _vertWASDPosition = new Vector3(-1f, -3.5f, 0f);
    private Vector3 _vertUDLRPosition = new Vector3(1f, -3.5f, 0f);

    private enum Direcs
    {
        Up, Down, Left, Right, None
    }

    void Start()
    {
        _camera = Camera.main;
        _initialCameraSize = _camera.orthographicSize;
        _scoreRT = _scoreText.gameObject.GetComponent<RectTransform>();
        _initialScorePosition = _scoreRT.anchoredPosition;
        _initialWASDPosition = _wasd.position;
        _initialUDLRPosition = _udlr.position;

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
        CheckScreenRotation();

        if (_movesLeft)
        {
            MoveInput();
            CheckForMoves();
        }
        else
            FadeAnimation();
    }

    private void CheckScreenRotation()
    {
        //Portrait
        if (_camera.scaledPixelHeight > _camera.scaledPixelWidth)
        {
            _camera.orthographicSize = _vertCameraSize;
            _scoreRT.anchoredPosition = _vertScorePosition;
            _wasd.position = _vertWASDPosition;
            _udlr.position = _vertUDLRPosition;
        }
        else //Landscape
        {
            _camera.orthographicSize = _initialCameraSize;
            _scoreRT.anchoredPosition = _initialScorePosition;
            _wasd.position = _initialWASDPosition;
            _udlr.position= _initialUDLRPosition;
        }
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
        _movesLeft = MoveCheck(Direcs.Left) || MoveCheck(Direcs.Right) || MoveCheck(Direcs.Up) || MoveCheck(Direcs.Down);
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

            Direcs direction = TouchControls();

            if (direction != Direcs.None)
                _moved = Move(direction);
            else if (Input.GetButtonDown("Left"))
                _moved = Move(Direcs.Left);
            else if (Input.GetButtonDown("Right"))
                _moved = Move(Direcs.Right);
            else if (Input.GetButtonDown("Down"))
                _moved = Move(Direcs.Down);
            else if (Input.GetButtonDown("Up"))
                _moved = Move(Direcs.Up);
            
            if (_moved)
                _delayTimer = _delayTime;
        }
        else
            _delayTimer -= Time.deltaTime;
    }

    private Direcs TouchControls()
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
                    return Direcs.Left;
                
                if (_endTouchPosition.x > _startTouchPosition.x)
                    return Direcs.Right;
            }
            
            if (Mathf.Abs(_endTouchPosition.x - _startTouchPosition.x) <
                Mathf.Abs(_endTouchPosition.y - _startTouchPosition.y))
            {
                if (_endTouchPosition.y < _startTouchPosition.y)
                    return Direcs.Down;
                
                if (_endTouchPosition.y > _startTouchPosition.y)
                    return Direcs.Up;
            }
        }

        return Direcs.None;
    }
    
    bool MoveCheck(Direcs direction)
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
            case Direcs.Left:
            {
                addNumCol = -1;
                startingPosition = 1;
                outsideMax = _gridManager._rows;
                insideMax = _gridManager._columns;
                break;
            }
            case Direcs.Right:
            {
                addNumCol = 1;
                startingPosition = _gridManager._columns - 2;
                outsideMax = _gridManager._rows;
                insideMax = _gridManager._columns;
                break;
            }
            case Direcs.Down:
            {
                addNumRow = -1;
                startingPosition = 1;
                outsideMax = _gridManager._columns;
                insideMax = _gridManager._rows;
                horizontal = false;
                break;
            }
            case Direcs.Up:
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

    bool Move(Direcs direction)
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
            case Direcs.Left:
            {
                addNumCol = -1;
                startingPosition = 1;
                outsideMax = _gridManager._rows;
                insideMax = _gridManager._columns;
                break;
            }
            case Direcs.Right:
            {
                addNumCol = 1;
                startingPosition = _gridManager._columns - 2;
                outsideMax = _gridManager._rows;
                insideMax = _gridManager._columns;
                break;
            }
            case Direcs.Down:
            {
                addNumRow = -1;
                startingPosition = 1;
                outsideMax = _gridManager._columns;
                insideMax = _gridManager._rows;
                horizontal = false;
                break;
            }
            case Direcs.Up:
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