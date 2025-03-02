using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    private GridManager _gridManager;
    [SerializeField] public Vector2Int _gridPos;
    
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveInput();
        GameObject tile = _gridManager.GetTile(_gridPos.x, _gridPos.y);
        
        //Move worldspace position to match current tile
        transform.position = tile.transform.position;
    }

    void MoveInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && (_gridPos.x > 0))
            _gridPos.x--;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && (_gridPos.x < _gridManager._rows - 1))
            _gridPos.x++;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && (_gridPos.y > 0))
            _gridPos.y--;
        else if (Input.GetKeyDown(KeyCode.UpArrow) && (_gridPos.y < _gridManager._columns - 1))
            _gridPos.y++;
    }
}
