using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public Ease easeType;
    public MatchValue matchValue;
    public int scoreValue = 20;

    private bool isMoving = false;
    private Board curBoard;

    public void Init(Board board)
    {
        curBoard = board;
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destX, int destY, float timeToMove)
    {
        if (!isMoving)
        {
            isMoving = true;
            transform.DOMove(new Vector3(destX, destY, 0), timeToMove).SetEase(easeType).OnComplete(() => {
                if (curBoard != null)
                {
                    curBoard.PlaceGamePiece(this, destX, destY);
                }
                isMoving = false;
            });
        } 
    }

    public void ChangeColor(GamePiece pieceToMatch)
    {
        SpriteRenderer rendererToChange = GetComponent<SpriteRenderer>();
        Color colorToMatch = Color.clear;
        if(pieceToMatch != null)
        {
            SpriteRenderer rendererToMatch = pieceToMatch.GetComponent<SpriteRenderer>();
            if (rendererToMatch != null && rendererToChange != null)
            {
                rendererToChange.color = rendererToMatch.color;
            }
            matchValue = pieceToMatch.matchValue;
        }
    }

    public void ScorePoints(int multiplier = 1, int bonus = 0)
    {
        if (ScoreManager.HasInstance)
        {
            ScoreManager.Instance.AddScore(scoreValue * multiplier + bonus);
        }

        OnPlaySFX();
    }

    public virtual void OnPlaySFX()
    {
        if (AudioManager.HasInstance)
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_PLUNGERPOP1);
        }
    }
}
