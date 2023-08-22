using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : BaseManager<ScoreManager>
{
    private int curScore = 0;
    public int CurrentScore => curScore;
    int maxScore;

    private void Start()
    {
        maxScore = LevelGoalScore.Instance.GetMaxScore();
        Debug.Log(maxScore);
    }

    public void AddScore(int value)
    {
        curScore += value;
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.BroadCast(ListenType.UPDATESCORE, curScore);
        }

        if (curScore >= maxScore)
        {
            GameManager.Instance.EndGame();
            StartCoroutine(GameManager.Instance.WaitForBoardRoutine(0.5f, () =>
            {
                if (UIManager.HasInstance && GameManager.Instance.IsGameOver)
                {
                    UIManager.Instance.HideAllScreens();
                    UIManager.Instance.ShowPopup<WinPanel>(true);
                }
            })); 
        }
    }

    public void ScorePoint(GamePiece piece, int multiplier = 1, int bonus = 0)
    {
       if(piece != null)
        {
            AddScore(piece.scoreValue * multiplier + bonus);
            LevelGoalScore.Instance.UpdateScoreStarts(curScore);
            if (UIManager.HasInstance && LevelGoalScore.HasInstance)
            {
                GamePanel gamePanel = UIManager.Instance.GetExistScreen<GamePanel>();
                gamePanel.scoreMeter.UpdateScoreMeter(curScore, LevelGoalScore.Instance.scoreStarts);
            }
            piece.OnPlaySFX();
        }
    }
}
