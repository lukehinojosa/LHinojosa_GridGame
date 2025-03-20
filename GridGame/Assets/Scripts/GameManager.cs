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
    [SerializeField] private TextMeshProUGUI _highScoreText;
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
    private float _vertCameraSize = 5;
    private Vector2 _vertScorePosition = new Vector2(0f, -200f);

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

        _objects = new GameObject[_gridManager._rows][];
        for (int i = 0; i < _gridManager._rows; i++)
            _objects[i] = new GameObject[_gridManager._columns];
        
        SpawnBlock();

        _highScoreText.text = "High Score: " + PlayerPrefs.GetFloat("Highscore", 0);
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
        }
        else //Landscape
        {
            _camera.orthographicSize = _initialCameraSize;
            _scoreRT.anchoredPosition = _initialScorePosition;
        }
    }

    private void FadeAnimation()
    {
        if (_fadeTween != null && _fadeTween.IsActive())
            return;

        Sequence seq = DOTween.Sequence();
        seq.Append(_loseScreen.DOFade(0.6f, 0.5f));
        _scoreText.text = "GAME OVER : " + _score;

        if (_score > PlayerPrefs.GetFloat("Highscore", 0f))
        {
            PlayerPrefs.SetFloat("Highscore", _score);
            _highScoreText.text = "High Score: " + _score;
        }
        
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
    
    /// <summary>
    /// Checks if any blocks can move or merge in the given direction without modifying the grid.
    /// This is used to determine if a move is possible before executing it.
    /// </summary>
    /// <param name="direction">The direction to check for possible moves (Left, Right, Up, or Down).</param>
    /// <returns>Returns true if at least one block can move or merge; otherwise, returns false.</returns>
    bool MoveCheck(Direcs direction)
    {
        bool moved = false; // Tracks if any move is possible
        
        // Variables to control movement direction
        int addNumRow = 0;
        int addNumCol = 0;
        int startingPosition = 0; // Determines where iteration begins for inside loop
        int outsideMax = 0; // Maximum index for outer loop
        int insideMax = 0; // Maximum index for inner loop
        bool horizontal = true; // Determines if movement is horizontal or vertical
        
        // Set movement parameters based on direction
        switch (direction)
        {
            case Direcs.Left:
            {
                addNumCol = 1; // Move right (increase column index)
                startingPosition = _gridManager._columns - 2; // Start from the second-last column
                outsideMax = _gridManager._rows; // Iterate over all rows
                insideMax = _gridManager._columns; // Iterate over columns
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
                addNumRow = -1; // Move downward (reduce row index)
                startingPosition = 1; // Start from the second row
                outsideMax = _gridManager._columns; // Iterate over columns
                insideMax = _gridManager._rows; // Iterate over rows
                horizontal = false; // Movement is vertical
                break;
            }
            case Direcs.Up:
            {
                addNumRow = 1; // Move upward (increase row index)
                startingPosition = _gridManager._rows - 2; // Start from the second-last row
                outsideMax = _gridManager._columns; // Iterate over columns
                insideMax = _gridManager._rows; // Iterate over rows
                horizontal = false; // Movement is vertical
                break;
            }
        }
        
        // Loop through each row/column based on movement direction
        for (int outside = 0; outside < outsideMax; outside++)
        {
            // Determine movement direction for inner loop
            int addNum = horizontal ? addNumCol : addNumRow;

            int size = 1;
            for (int inside = startingPosition; size < insideMax; inside -= addNum)
            {
                int row;
                int column;
                
                // Determine row and column indices based on movement direction
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
                
                // Check if the current block is not empty
                if (_objects[row][column] != null)
                {
                    // If the adjacent block in the movement direction is empty, a move is possible
                    if (_objects[row + addNumRow][column + addNumCol] == null)
                        moved = true;
                    // If the adjacent block has the same number and both have not been combined yet, a merge, therefore a move, is possible
                    else if (!_objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>()._combined &&
                             !_objects[row][column].GetComponent<BlockObject>()._combined &&
                             _objects[row + addNumRow][column + addNumCol].GetComponent<BlockObject>().GetBlockNumber() ==
                             _objects[row][column].GetComponent<BlockObject>().GetBlockNumber())
                        moved = true;
                }
            }
        }
        
        return moved; // Return whether a move is possible
    }
    
    /// <summary>
    /// Moves the blocks in the grid in the specified direction.
    /// Blocks slide as far as possible and combine if they have the same value.
    /// </summary>
    /// <param name="direction">The direction in which to move the blocks (Left, Right, Up, or Down).</param>
    /// <returns>Returns true if any blocks moved or merged; otherwise, returns false.</returns>
    bool Move(Direcs direction)
    {
        bool moved = false; // Tracks if any block has moved
        
        // Variables to control movement direction
        int addNumRow = 0;
        int addNumCol = 0;
        int startingPosition = 0; // Determines where iteration begins for inside loop
        int outsideMax = 0; // Maximum index for outer loop
        int insideMax = 0; // Maximum index for inner loop
        bool horizontal = true; // Determines if movement is horizontal or vertical
        
        // Set movement parameters based on direction
        switch (direction)
        {
            case Direcs.Left:
            {
                addNumCol = -1; // Move left (reduce column index)
                startingPosition = 1; // Start from the second column
                outsideMax = _gridManager._rows; // Iterate over all rows
                insideMax = _gridManager._columns; // Iterate over columns
                break;
            }
            case Direcs.Right:
            {
                addNumCol = 1; // Move right (increase column index)
                startingPosition = _gridManager._columns - 2; // Start from the second-to-last column
                outsideMax = _gridManager._rows; // Iterate over all rows
                insideMax = _gridManager._columns; // Iterate over columns
                break;
            }
            case Direcs.Down:
            {
                addNumRow = -1; // Move Downward (reduce row index)
                startingPosition = 1; // Start from the second row
                outsideMax = _gridManager._columns; // Iterate over columns
                insideMax = _gridManager._rows; // Iterate over rows
                horizontal = false; // Movement is vertical
                break;
            }
            case Direcs.Up:
            {
                addNumRow = 1; // Move upward (increase row index)
                startingPosition = _gridManager._rows - 2; // Start from the second-to-last row
                outsideMax = _gridManager._columns; // Iterate over columns
                insideMax = _gridManager._rows; // Iterate over rows
                horizontal = false; // Movement is vertical
                break;
            }
        }
        
        // Loop through each row/column based on movement direction
        for (int outside = 0; outside < outsideMax; outside++)
        {
            bool blocksMoved = true; // Tracks if any block has moved in the current iteration
            
            // Repeat the process while any block in moved
            while (blocksMoved)
            {
                blocksMoved = false;
                
                // Determine movement direction for inner loop
                int addNum = horizontal ? addNumCol : addNumRow;

                int size = 1;
                for (int inside = startingPosition; size < insideMax; inside -= addNum)
                {
                    int row, column;
                    
                    // Determine row and column indices based on movement direction
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
                    
                    // Check if the current block is not empty
                    if (_objects[row][column] != null)
                    {
                        // If the adjacent block in the movement direction is empty, move the block
                        if (_objects[row + addNumRow][column + addNumCol] == null)
                        {
                            blocksMoved = true;
                            moved = true;
                            MoveBlockToBlank(row, column, addNumRow, addNumCol);
                        }
                        // If the adjacent block has the same number and both have not been combined yet, merge them
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
            
            // Reset the "_combined" flag for all blocks after each move
            for (int i = 0; i < _gridManager._rows; i++)
                for (int j = 0; j < _gridManager._columns; j++)
                    if (_objects[i][j] != null)
                        _objects[i][j].GetComponent<BlockObject>()._combined = false;
        }
        
        return moved; // Return whether any block was moved
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