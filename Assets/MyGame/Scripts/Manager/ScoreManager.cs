using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : BaseManager<ScoreManager>
{
    private int curScore = 0;
    public int CurrentScore => curScore;

    public void AddScore(int value)
    {
        curScore += value;
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.BroadCast(ListenType.UPDATESCORE, curScore);
        }

        if(curScore >= GameManager.Instance.scoreGoal)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySE(AUDIO.SE_WIN1);
            }

            GameManager.Instance.EndGame();
            if (UIManager.HasInstance && GameManager.Instance.IsGameOver)
            {
                UIManager.Instance.HideAllScreens();
                UIManager.Instance.ShowPopup<WinPanel>(true);
            }
        }
    }
}
