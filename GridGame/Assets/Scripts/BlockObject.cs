using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockObject : MonoBehaviour
{
    private GridManager _gridManager;
    [SerializeField] public Vector2Int _gridPos;
    [SerializeField] private int _blockNumber = 2;
    private TextMeshProUGUI _blockNumberText;
    public bool _combined;
    public Vector3 _newPosition;
    private float _moveSpeed = 15f;
    public bool _destroy = false;

    public int GetBlockNumber()
    {
        return _blockNumber;
    }

    void Awake()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _blockNumberText = GetComponentInChildren<Canvas>().gameObject.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    void Start()
    {
        _blockNumberText.text = _blockNumber.ToString();
        
        //Immediately assigns their position to gridpos if they aren't an animation
        if (!_destroy)
        {
            _newPosition = transform.position;
            UpdatePosition();
            transform.position = _newPosition;
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _newPosition, _moveSpeed * Time.deltaTime);
        
        if (_destroy && transform.position == _newPosition)
            Destroy(gameObject);
    }

    public void UpdatePosition()
    {
        _newPosition = ConvertGridToWorld(_gridPos);
    }

    public Vector3 ConvertGridToWorld(Vector2Int grid)
    {
        GameObject tile = _gridManager.GetTile(grid.x, grid.y);
        return tile.transform.position;
    }

    public void DoubleNumber()
    {
        _blockNumber *= 2;
        
        _blockNumberText.text = _blockNumber.ToString();
    }
    
    public void SetNumber(int num)
    {
        _blockNumber = num;
        
        _blockNumberText.text = _blockNumber.ToString();
    }
}
