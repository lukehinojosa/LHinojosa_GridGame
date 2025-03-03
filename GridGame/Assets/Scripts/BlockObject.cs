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
    private Vector3 _newPosition;
    private float _moveSpeed = 5f;

    public int GetBlockNumber()
    {
        return _blockNumber;
    }
    
    void Start()
    {
        _newPosition = transform.position;
        _gridManager = FindObjectOfType<GridManager>();
        
        _blockNumberText = GetComponentInChildren<Canvas>().gameObject.GetComponentInChildren<TextMeshProUGUI>();
        
        _blockNumberText.text = _blockNumber.ToString();

        UpdatePosition();
        transform.position = _newPosition;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _newPosition, _moveSpeed * Time.deltaTime);
    }

    public void UpdatePosition()
    {
        transform.position = _newPosition;
        GameObject tile = _gridManager.GetTile(_gridPos.x, _gridPos.y);
        
        //Get worldspace position to match current tile
        _newPosition = tile.transform.position;
    }

    public void DoubleNumber()
    {
        _blockNumber *= 2;
        
        _blockNumberText.text = _blockNumber.ToString();
    }
}
