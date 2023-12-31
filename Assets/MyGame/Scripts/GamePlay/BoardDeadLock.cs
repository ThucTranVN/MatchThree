using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardDeadLock : MonoBehaviour
{
    private List<GamePiece> GetRowOrColumnList(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        int width = allPieces.GetLength(0);
        int heigth = allPieces.GetLength(1);

        List<GamePiece> pieceList = new List<GamePiece>();

        for (int i = 0; i < listLength; i++)
        {
            if (checkRow)
            {
                if(x + i < width && y < heigth && allPieces[x + i, y] != null)
                {
                    pieceList.Add(allPieces[x + i, y]);
                }
            }
            else
            {
                if(x < width && y + i < heigth && allPieces[x, y + i] != null)
                {
                    pieceList.Add(allPieces[x, y + i]);
                }
            }
        }

        return pieceList;
    }

    private List<GamePiece> GetMiniumMatches(List<GamePiece> gamePieces, int minForMatch = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();

        var groups = gamePieces.GroupBy(n => n.matchValue);

        foreach (var group in groups)
        {
            if(group.Count() >= minForMatch && group.Key != MatchValue.None)
            {
                matches = group.ToList();
            }
        }

        return matches;
    }

    private List<GamePiece> GetNeighbors(GamePiece[,] allPieces, int x, int y)
    {
        int width = allPieces.GetLength(0);
        int heigth = allPieces.GetLength(1);
        List<GamePiece> neighbors = new List<GamePiece>();

        Vector2[] searchDirections = new Vector2[4]
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f)
        };

        foreach (var dir in searchDirections)
        {
            if (x + (int)dir.x >= 0 && x+ (int)dir.x < width && y + (int)dir.y >= 0 && y + (int)dir.y < heigth)
            {
                if (allPieces[x + (int)dir.x, y + (int)dir.y] != null)
                {
                    if (!neighbors.Contains(allPieces[x + (int)dir.x, y + (int)dir.y]))
                    {
                        neighbors.Add(allPieces[x + (int)dir.x, y + (int)dir.y]);
                    }
                }
            }
        }

        return neighbors;
    }

    private bool HasMoveAt(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> pieces = GetRowOrColumnList(allPieces, x, y, listLength, checkRow);
        List<GamePiece> matches = GetMiniumMatches(pieces, listLength - 1);
        GamePiece unMatchedPiece = null;

        if(pieces != null && matches != null)
        {
            if(pieces.Count == listLength && matches.Count == listLength - 1)
            {
                unMatchedPiece = pieces.Except(matches).FirstOrDefault();
            }

            if(unMatchedPiece != null)
            {
                List<GamePiece> neighbors = GetNeighbors(allPieces, unMatchedPiece.xIndex, unMatchedPiece.yIndex);
                neighbors = neighbors.Except(matches).ToList();
                neighbors = neighbors.FindAll(n => n.matchValue == matches[0].matchValue);
                matches = matches.Union(neighbors).ToList();
            }

            if(matches.Count >= listLength)
            {
                //string rowColStr = (checkRow) ? "row" : "column";
                //Debug.Log("==========AVAIABLE MOVE==========");
                //Debug.Log("Move " + matches[0].matchValue + " piece to " + unMatchedPiece.xIndex + "," + unMatchedPiece.yIndex + " to form matching " + rowColStr);
                return true;
            }
        }

        return false;
    }

    public bool IsDeadLocked(GamePiece[,] allPieces, int listLength =3)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        bool isDeadLocked = true;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(HasMoveAt(allPieces, i, j, listLength, true) || HasMoveAt(allPieces, i , j , listLength, false))
                {
                    isDeadLocked = false;
                }
            }
        }
        //if (isDeadLocked)
        //{
        //    Debug.Log("=======Board Dedlocked=========");
        //}

        return isDeadLocked;
    }
}
