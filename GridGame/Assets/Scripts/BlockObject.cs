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

    public int GetBlockNumber()
    {
        return _blockNumber;
    }
    
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        
        _blockNumberText = GetComponentInChildren<Canvas>().gameObject.GetComponentInChildren<TextMeshProUGUI>();
        
        _blockNumberText.text = _blockNumber.ToString();

        UpdatePosition();
    }

    public void UpdatePosition()
    {
        GameObject tile = _gridManager.GetTile(_gridPos.x, _gridPos.y);
        
        //Get worldspace position to match current tile
        transform.position = tile.transform.position;
    }

    public void DoubleNumber()
    {
        _blockNumber *= 2;
        
        _blockNumberText.text = _blockNumber.ToString();
    }
}
