using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BoardDeadLock))]
[RequireComponent(typeof(BoardSuffer))]
public class Board : MonoBehaviour
{
    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] adjacentBombPrefabs;
    public GameObject[] columnBombPrefabs;
    public GameObject[] rowBombPrefabs;
    public GameObject colorBombPrefab;
    public GameObject[] gamePiecePrefabs;
    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;
    public GameObject[] collectiblePrefabs;
    public int width;
    public int height;
    public int borderSize;
    public float swapTime = 0.5f;
    public int fillYOffset = 10;
    public float fillMoveTime = 0.5f;
    public int maxCollectibles = 3;
    public int collectibleCount = 0;
    public bool isRefilling = false;
    [Range(0,1)]
    public float chanceForCollectible = 0.1f;

    private Tile[,] allTiles;
    private GamePiece[,] allGamePices;
    private Tile clikedTile;
    private Tile targetedTile;
    private GameObject clickedTileBomb;
    private GameObject tartgetedTileBomb;
    private bool isPlayerInputEnabled = true;
    private int scoreMultiplier = 0;
    private BoardDeadLock boardDeadLock;
    private BoardSuffer boardSuffer;

    private void Start()
    {
        allTiles = new Tile[width, height];
        allGamePices = new GamePiece[width, height];
        boardDeadLock = GetComponent<BoardDeadLock>();
        boardSuffer = GetComponent<BoardSuffer>();
    }

    public void SetupBoard()
    {
        SetupTile();
        SetupGamePieces();
        List<GamePiece> startingCollectibles = FindAllCollectibles();
        collectibleCount = startingCollectibles.Count;
        SetupCamera();
        FillBoard(fillYOffset, fillMoveTime);
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.Log("Board: Invalid gamepiece");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithInBounds(x, y))
        {
            allGamePices[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x, y);
    }

    public void ClickTile(Tile tile)
    {
        if (clikedTile == null)
        {
            clikedTile = tile;
            //Debug.Log("Clicked Tile: " + tile.name);
        }
    }

    public void DragToTile(Tile tile)
    {
        if (clikedTile != null && IsNextTo(tile, clikedTile))
        {
            targetedTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (clikedTile != null && targetedTile != null)
        {
            SwitchTile(clikedTile, targetedTile);
        }

        clikedTile = null;
        targetedTile = null;
    }

    private void SetupTile()
    {
        foreach (StartingObject sTile in startingTiles)
        {
            if(sTile != null)
            {
                CreateTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allTiles[i,j] == null)
                {
                    CreateTile(tileNormalPrefab, i, j);
                }              
            }
        }
    }

    private void CreateTile(GameObject prefab, int x, int y, int z = 0)
    {
        if(prefab != null && IsWithInBounds(x,y))
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";
            allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = this.transform;
            allTiles[x, y].Init(x, y, this);
        }
    }

    private GameObject CreateBomb(GameObject prefab, int x, int y)
    {
        if(prefab != null && IsWithInBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    private void SetupGamePieces()
    {
        foreach (StartingObject sPiece in startingGamePieces)
        {
            if(sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;
                CreateGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillMoveTime);
            }
        }
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height-1) / 2f, -10f);
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticleSize = (float)height / 2f + (float)borderSize;
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;
        Camera.main.orthographicSize = (verticleSize > horizontalSize) ? verticleSize : horizontalSize;
    }

    private GameObject GetRandomObject(GameObject[] objectArray)
    {
        int randomIdx = Random.Range(0, objectArray.Length);
        if(objectArray[randomIdx] == null)
        {
            Debug.LogWarning("Borad.GetRandomObject at index " + randomIdx + " does not contain a valid GameObject");
        }
        return objectArray[randomIdx];
    }

    private GameObject GetRandomGamePiece()
    {
        return GetRandomObject(gamePiecePrefabs);
    }

    private GameObject GetRandomCollectible()
    {
        return GetRandomObject(collectiblePrefabs);
    }

    private bool IsWithInBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private void FillBoardFromList(List<GamePiece> gamePieces)
    {
        Queue<GamePiece> unusedPieces = new Queue<GamePiece>(gamePieces);

        int maxInterations = 100;
        int iterations = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allGamePices[i, j] == null && allTiles[i, j].tileType != TileType.Obstacle)
                {
                    allGamePices[i, j] = unusedPieces.Dequeue();

                    iterations = 0;
                    while (HasMatchOnFill(i, j))
                    {
                        unusedPieces.Enqueue(allGamePices[i, j]);
                        allGamePices[i, j] = unusedPieces.Dequeue();
                        iterations++;

                        if (iterations >= maxInterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxInterations = 100;
        int iterations = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allGamePices[i,j] == null && allTiles[i,j].tileType != TileType.Obstacle)
                {
                    if(j == height -1 && CanAddCollectible())
                    {
                        FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
                        collectibleCount++;
                    }
                    else
                    {
                        FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                        iterations = 0;

                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAt(i, j);
                            FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                            iterations++;

                            if (iterations >= maxInterations)
                            {
                                break;
                            }
                        }
                    }
                }               
            }
        }
    }

    private GamePiece FillRandomGamePieceAt(int x, int y, int fallYOffset = 0, float moveTime = 0.1f)
    {
        if(IsWithInBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

            CreateGamePiece(randomPiece, x, y, fallYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }
        
        return null;
    }

    private GamePiece FillRandomCollectibleAt(int x, int y, int fallYOffset = 0, float moveTime = 0.1f)
    {
        if (IsWithInBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity) as GameObject;

            CreateGamePiece(randomPiece, x, y, fallYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }

        return null;
    }

    private void CreateGamePiece(GameObject prefab, int x, int y, int fallYOffset = 0, float moveTime = 0.1f)
    {
        if (prefab != null && IsWithInBounds(x,y))
        {
            prefab.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

            if (fallYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + fallYOffset, 0);
                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }

            prefab.transform.parent = this.transform;
        }
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        if(downMatches == null)
        {
            downMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downMatches.Count > 0);
    }

    private void SwitchTile(Tile clickTile, Tile targetTile)
    {
        StartCoroutine(SwitchTileRoutine(clickTile, targetTile));
    }

    private IEnumerator SwitchTileRoutine(Tile clickTile, Tile targetTile)
    {
        if (GameManager.HasInstance)
        {
            if (isPlayerInputEnabled && !GameManager.Instance.IsGameOver)
            {
                GamePiece clickedPiece = allGamePices[clickTile.xIndex, clickTile.yIndex];
                GamePiece targetPiece = allGamePices[targetTile.xIndex, targetTile.yIndex];

                if (targetPiece != null && clickedPiece != null)
                {
                    clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                    targetPiece.Move(clickTile.xIndex, clickTile.yIndex, swapTime);

                    yield return new WaitForSeconds(swapTime);

                    List<GamePiece> clickedPieceMatchs = FindMatchAt(clickTile.xIndex, clickTile.yIndex);
                    List<GamePiece> targetPieceMatchs = FindMatchAt(targetTile.xIndex, targetTile.yIndex);
                    List<GamePiece> colorMatches = new List<GamePiece>();

                    if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
                    {
                        clickedPiece.matchValue = targetPiece.matchValue;
                        colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                    }
                    else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                    {
                        targetPiece.matchValue = clickedPiece.matchValue;
                        colorMatches = FindAllMatchValue(targetPiece.matchValue);
                    }
                    else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                    {
                        foreach (var piece in allGamePices)
                        {
                            if (!colorMatches.Contains(piece))
                            {
                                colorMatches.Add(piece);
                            }
                        }
                    }


                    if (clickedPieceMatchs.Count == 0 && targetPieceMatchs.Count == 0 && colorMatches.Count == 0)
                    {
                        clickedPiece.Move(clickTile.xIndex, clickTile.yIndex, swapTime);
                        targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                    }
                    else
                    {
                        if (GameManager.HasInstance && ListenerManager.HasInstance)
                        {
                            LevelGoalScore.Instance.moveLeft--;
                            ListenerManager.Instance.BroadCast(ListenType.UPDATECOUNTMOVE, LevelGoalScore.Instance.moveLeft);

                            if(LevelGoalScore.Instance.moveLeft == 0)
                            {
                                GameManager.Instance.EndGame();
                                if (UIManager.HasInstance && GameManager.Instance.IsGameOver)
                                {
                                    UIManager.Instance.HideAllScreens();
                                    UIManager.Instance.ShowPopup<LosePanel>(true);
                                    
                                }
                            }
                        }

                        yield return new WaitForSeconds(swapTime);

                        Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickTile.xIndex, targetTile.yIndex - clickTile.yIndex);
                        clickedTileBomb = DropBomb(clickTile.xIndex, clickTile.yIndex, swipeDirection, clickedPieceMatchs);
                        tartgetedTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swipeDirection, targetPieceMatchs);

                        if (clickedTileBomb != null && targetPiece != null)
                        {
                            GamePiece clickedBombPiece = clickedTileBomb.GetComponent<GamePiece>();
                            if (!IsColorBomb(clickedBombPiece))
                            {
                                clickedBombPiece.ChangeColor(targetPiece);
                            }
                        }

                        if (tartgetedTileBomb != null && clickedPiece != null)
                        {
                            GamePiece targetBombPiece = tartgetedTileBomb.GetComponent<GamePiece>();
                            if (!IsColorBomb(targetBombPiece))
                            {
                                targetBombPiece.ChangeColor(clickedPiece);
                            }
                        }

                        ClearAndRefillBoard(clickedPieceMatchs.Union(targetPieceMatchs).ToList().Union(colorMatches).ToList());
                    }
                }
            }
        }
        
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if(Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 direction, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        if(IsWithInBounds(startX, startY))
        {
            startPiece = allGamePices[startX, startY];
        }

        if(startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;
        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(direction.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(direction.y, -1, 1) * i;

            if(!IsWithInBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = allGamePices[nextX, nextY];
            if(nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }

        if(matches.Count >= minLength)
        {
            return matches;
        }

        return null;
    }

    private List<GamePiece> FindVerticlelMatchs(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if(upMatches == null)
        {
            upMatches = new List<GamePiece>();
        }

        if(downMatches == null)
        {
            downMatches = new List<GamePiece>();
        }

        var combinedMatchs = upMatches.Union(downMatches).ToList();

        return (combinedMatchs.Count >= minLength) ? combinedMatchs : null;
    }

    private List<GamePiece> FindHorizontalMatchs(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> leftMatchs = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);

        if (leftMatchs == null)
        {
            leftMatchs = new List<GamePiece>();
        }

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        var combinedMatchs = leftMatchs.Union(rightMatches).ToList();

        return (combinedMatchs.Count >= minLength) ? combinedMatchs : null;
    }

    private List<GamePiece> FindMatchAt(int x, int y, int mintLength = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatchs(x, y, mintLength);
        List<GamePiece> verticleMatches = FindVerticlelMatchs(x, y, mintLength);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<GamePiece>();
        }

        if (verticleMatches == null)
        {
            verticleMatches = new List<GamePiece>();
        }

        var combinedMatches = horizontalMatches.Union(verticleMatches).ToList();
        return combinedMatches;
    }

    private List<GamePiece> FindMatchAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }

        return matches;
    }

    private List<GamePiece> FIndAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    private void HighLightMatchs()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighLightMatchAt(i,j);
            }
        }
    }

    private void HighLightMatchAt(int x, int y)
    {
        HighLightTileOff(x, y);

        List<GamePiece> combinedMatches = FindMatchAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece gamePiece in combinedMatches)
            {
                HighLightTileOn(gamePiece.xIndex, gamePiece.yIndex, gamePiece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighLightTileOn(int x, int y, Color color)
    {
        if (allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;
        }
    }

    private void HighLightTileOff(int x, int y)
    {
        if(allTiles[x,y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    private void HighLightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                HighLightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = allGamePices[x, y];
        if(pieceToClear != null)
        {
            allGamePices[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }

        //HighLightTileOff(x, y);
    }

    private void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = allTiles[x, y];
        if(tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if(EffectManager.HasInstance)
            {
                EffectManager.Instance.BreakTileFxAt(tileToBreak.breakableValue, tileToBreak.xIndex, tileToBreak.yIndex, 0);
            }

            tileToBreak.BreakTile();
        }
    }

    private void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if(piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);

                if (EffectManager.HasInstance)
                {
                    EffectManager.Instance.ClearPieceFxAt(i, j);
                }
            }
        }
    }

    private void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);

                int bonus = 0;

                if(gamePieces.Count >= 4)
                {
                    bonus = 20;
                }
                if (ScoreManager.HasInstance)
                {
                    ScoreManager.Instance.ScorePoint(piece, scoreMultiplier, bonus);
                }
                //piece.ScorePoints(scoreMultiplier, bonus);

                if(EffectManager.HasInstance)
                {
                    if (bombedPieces.Contains(piece))
                    {
                        EffectManager.Instance.BombFxAt(piece.xIndex, piece.yIndex);
                    }
                    else
                    {
                        EffectManager.Instance.ClearPieceFxAt(piece.xIndex, piece.yIndex);
                    }
                }
            }
        }
    }

    private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if(allGamePices[column,i] == null && allTiles[column,i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if(allGamePices[column, j] != null)
                    {
                        allGamePices[column, j].Move(column, i, collapseTime * (j-i));
                        allGamePices[column, i] = allGamePices[column, j];
                        allGamePices[column, i].SetCoord(column, i);

                        if (!movingPieces.Contains(allGamePices[column, i]))
                        {
                            movingPieces.Add(allGamePices[column, i]);
                        }

                        allGamePices[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    private List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> colmunToCollaps = GetColumns(gamePieces);
        foreach (int column in colmunToCollaps)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    private List<GamePiece> CollapseColumn(List<int> columnsToCollapse)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    private List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                if (!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }
        }
        return columns;
    }

    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    private IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        isPlayerInputEnabled = false;
        isRefilling = true;
        List<GamePiece> matches = gamePieces;
        scoreMultiplier = 0;
        do
        {
            scoreMultiplier++;
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;
            yield return StartCoroutine(RefillRoutine());
            matches = FIndAllMatches();
            yield return new WaitForSeconds(0.2f);
        } while (matches.Count != 0);

        if (boardDeadLock.IsDeadLocked(allGamePices, 3))
        {
            yield return new WaitForSeconds(1f);

            StartCoroutine(ShuffleBoardRoutine());

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(RefillRoutine());
        }

        isPlayerInputEnabled = true;
        isRefilling = false;
    }

    private IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, fillMoveTime);
        yield return null;
    }

    private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighLightPieces(gamePieces);

        yield return new WaitForSeconds(0.2f);

        bool isFinised = false;

        while (!isFinised)
        {

            List<GamePiece> bombPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();

            bombPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();

            List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);

            List<GamePiece> allCollectible = FindAllCollectibles();

            List<GamePiece> blockers = gamePieces.Intersect(allCollectible).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();

            collectibleCount -= collectedPieces.Count;
            gamePieces = gamePieces.Union(collectedPieces).ToList();

            List<int> columnsToCollapse = GetColumns(gamePieces);

            ClearPieceAt(gamePieces, bombPieces);
            BreakTileAt(gamePieces);

            if(clickedTileBomb != null)
            {
                ActiveBomb(clickedTileBomb);
                clickedTileBomb = null;
            }

            if (tartgetedTileBomb != null)
            {
                ActiveBomb(tartgetedTileBomb);
                tartgetedTileBomb = null;
            }

            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(columnsToCollapse);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchAt(movingPieces);
            collectedPieces = FindCollectiblesAt(0, true);
            matches = matches.Union(collectedPieces).ToList();

            if(matches.Count == 0)
            {
                isFinised = true;
                break;
            }
            else
            {
                scoreMultiplier++;
                if (AudioManager.HasInstance)
                {
                    AudioManager.Instance.PlaySE(AUDIO.SE_POWERUP);
                }
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    private bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                if(piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }

                if (piece.transform.position.x - (float)piece.xIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if(allGamePices[i,row] != null)
            {
                gamePieces.Add(allGamePices[i, row]);
            }
        }

        return gamePieces;
    }

    private List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            if (allGamePices[column, i] != null)
            {
                gamePieces.Add(allGamePices[column, i]);
            }
        }

        return gamePieces;
    }

    private List<GamePiece> GetAdjacentPieces(int x, int y, int offSet = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offSet; i <= x + offSet; i++)
        {
            for (int j = y - offSet; j <= y + offSet; j++)
            {
                if (IsWithInBounds(i, j))
                {
                    gamePieces.Add(allGamePices[i, j]);
                }
            }
        }

        return gamePieces;
    }

    private List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();
        foreach (var piece in gamePieces)
        {
            if(piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();
                Bomb bomb = piece.GetComponent<Bomb>();
                if(bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            break;
                    }
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                    allPiecesToClear = RemoveCollectibles(allPiecesToClear);
                }
            }
        }
        return allPiecesToClear;
    }

    private bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (var piece in gamePieces)
        {
            if(piece != null)
            {
                if(xStart == -1 && yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }

                if(piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }

                if(piece.xIndex == xStart && piece.yIndex != yStart)
                {
                    vertical = true;
                }
            }
        }

        return (horizontal && vertical);
    }

    private GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;
        MatchValue matchValue = MatchValue.None;

        if(gamePieces != null)
        {
            matchValue = FindMatchValue(gamePieces);
        }

        if (gamePieces.Count >= 5 && matchValue != MatchValue.None)
        {
            if (IsCornerMatch(gamePieces))
            {
                GameObject adjacentBomb = FindGamePieceByMatchvalue(adjacentBombPrefabs, matchValue);

                if (adjacentBomb != null)
                {
                    bomb = CreateBomb(adjacentBomb, x, y);
                }
            }
            else
            {
                if (colorBombPrefab != null)
                {
                    bomb = CreateBomb(colorBombPrefab, x, y);
                }
            }
        }
        else if (gamePieces.Count == 4 && matchValue != MatchValue.None)
        {
            if (swapDirection.x != 0)
            {
                GameObject rowBomb = FindGamePieceByMatchvalue(rowBombPrefabs, matchValue);

                if (rowBomb != null)
                {
                    bomb = CreateBomb(rowBomb, x, y);
                }
            }
            else
            {
                GameObject columnBomb = FindGamePieceByMatchvalue(columnBombPrefabs, matchValue);

                if (columnBomb != null)
                {
                    bomb = CreateBomb(columnBomb, x, y);
                }
            }
        }
        return bomb;   
    }

    private void ActiveBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if (IsWithInBounds(x, y))
        {
            allGamePices[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    private List<GamePiece> FindAllMatchValue(MatchValue malue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allGamePices[i,j] != null)
                {
                    if(allGamePices[i,j].matchValue == malue)
                    {
                        foundPieces.Add(allGamePices[i, j]);
                    }
                }
            }
        }

        return foundPieces;
    }

    private bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if(bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }

        return false;
    }

    private List<GamePiece> FindCollectiblesAt(int row, bool clearAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if(allGamePices[i,row] != null)
            {
                Collectible collectibleComponent = allGamePices[i, row].GetComponent<Collectible>();

                if(collectibleComponent != null)
                {
                    if(!clearAtBottomOnly || (clearAtBottomOnly && collectibleComponent.clearedAtBottom))
                    {
                        foundCollectibles.Add(allGamePices[i, row]);
                    }
                }
            }
        }

        return foundCollectibles;
    }

    private List<GamePiece> FindAllCollectibles()
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }

        return foundCollectibles;
    }

    private bool CanAddCollectible()
    {
        return (Random.Range(0f, 1f) <= chanceForCollectible && collectiblePrefabs.Length > 0 && collectibleCount < maxCollectibles);
    }

    private List<GamePiece> RemoveCollectibles(List<GamePiece> bombPieces)
    {
        List<GamePiece> collectiblePieces = FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach (var piece in collectiblePieces)
        {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();

            if(collectibleComponent != null)
            {
                if (!collectibleComponent.clearedByBomb)
                {
                    piecesToRemove.Add(piece);
                }
            }
        }

        return bombPieces.Except(piecesToRemove).ToList();
    }

    private MatchValue FindMatchValue(List<GamePiece> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if(piece != null)
            {
                return piece.matchValue;
            }
        }

        return MatchValue.None;
    }

    private GameObject FindGamePieceByMatchvalue(GameObject[] gamePiecePrefabs, MatchValue matchValue)
    {
        if(matchValue == MatchValue.None)
        {
            return null;
        }

        foreach (var go in gamePiecePrefabs)
        {
            GamePiece piece = go.GetComponent<GamePiece>();

            if(piece != null)
            {
                if(piece.matchValue == matchValue)
                {
                    return go;
                }
            }
        }

        return null;
    }

    //public void TestDeadLock()
    //{
    //    boardDeadLock.IsDeadLocked(allGamePices, 3);
    //}

    //public void TestShuffleBoard()
    //{
    //    if (isPlayerInputEnabled)
    //    {
    //        StartCoroutine(ShuffleBoardRoutine());
    //    }
    //}

    private IEnumerator ShuffleBoardRoutine()
    {
        List<GamePiece> allPieces = new List<GamePiece>();

        foreach (var piece in allGamePices)
        {
            allPieces.Add(piece);
        }

        while (!IsCollapsed(allPieces))
        {
            yield return null;
        }

        List<GamePiece> normalPieces = boardSuffer.RemoveNormalPieces(allGamePices);
        boardSuffer.ShuffleList(normalPieces);
        FillBoardFromList(normalPieces);
        boardSuffer.MovePieces(allGamePices, swapTime);

        List<GamePiece> matches = FIndAllMatches();
        StartCoroutine(ClearAndRefillBoardRoutine(matches));
    }
}
