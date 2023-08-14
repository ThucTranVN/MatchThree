using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public TileType tileType = TileType.Normal;
    private Board _board;
    private SpriteRenderer spriteRenderer;

    public int breakableValue = 0;
    public Sprite[] breakbableSprites;
    public Color normalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if(_board != null)
        {
            _board.ClickTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (_board != null)
        {
            _board.DragToTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (_board != null)
        {
            _board.ReleaseTile();
        }
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        _board = board;
        if(tileType == TileType.Breakable)
        {
            if (breakbableSprites[breakableValue] != null)
            {
                spriteRenderer.sprite = breakbableSprites[breakableValue];
            }
        }
    }

    public void BreakTile()
    {
        if(tileType != TileType.Breakable)
        {
            return;
        }

        StartCoroutine(BreakTileRoutine());
    }

    private IEnumerator BreakTileRoutine()
    {
        breakableValue--;
        breakableValue = Mathf.Clamp(breakableValue, 0, breakableValue);

        yield return new WaitForSeconds(0.25f);

        if(breakbableSprites[breakableValue] != null)
        {
            spriteRenderer.sprite = breakbableSprites[breakableValue];
        }

        if(breakableValue <= 0)
        {
            tileType = TileType.Normal;
            spriteRenderer.color = normalColor;
        }
    }
}
