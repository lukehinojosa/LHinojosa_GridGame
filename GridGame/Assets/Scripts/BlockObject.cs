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
    private SpriteRenderer _sR;
    private Color _color2 = new Color32(238, 228, 218, 255);
    private Color _color4 = new Color32(236, 224, 202, 255);
    private Color _color8 = new Color32(244,177,122,255);
    private Color _color16 = new Color32(245,149,101,255);
    private Color _color32 = new Color32(245,124,95,255);
    private Color _color64 = new Color32(246,93,59,255);
    private Color _color128 = new Color32(237,206,113,255);
    private Color _color256 = new Color32(234,207,92,255);
    private Color _color512 = new Color32(237,198,81,255);
    private Color _color1024 = new Color32(238,199,68,255);
    private Color _color2048 = new Color32(238,194,46,255);
    private Color _color4096 = new Color32(254, 61, 62, 255);

    public int GetBlockNumber()
    {
        return _blockNumber;
    }

    void Awake()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _blockNumberText = GetComponentInChildren<Canvas>().gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _sR = GetComponent<SpriteRenderer>();
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
        
        UpdateColor();
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

        UpdateColor();
    }
    
    public void SetNumber(int num)
    {
        _blockNumber = num;
        
        _blockNumberText.text = _blockNumber.ToString();
        
        UpdateColor();
    }

    void UpdateColor()
    {
        switch (_blockNumber)
        {
            case 2:
            {
                _sR.color = _color2;
                break;
            }
            case 4:
            {
                _sR.color = _color4;
                break;
            }
            case 8:
            {
                _sR.color = _color8;
                break;
            }
            case 16:
            {
                _sR.color = _color16;
                break;
            }
            case 32:
            {
                _sR.color = _color32;
                break;
            }
            case 64:
            {
                _sR.color = _color64;
                break;
            }
            case 128:
            {
                _sR.color = _color128;
                break;
            }
            case 256:
            {
                _sR.color = _color256;
                break;
            }
            case 512:
            {
                _sR.color = _color512;
                break;
            }
            case 1024:
            {
                _sR.color = _color1024;
                break;
            }
            case 2048:
            {
                _sR.color = _color2048;
                break;
            }
            case 4096:
            {
                _sR.color = _color4096;
                break;
            }
        }
    }
}
