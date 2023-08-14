using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GamePanel : BaseScreen
{
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI moveLeftText;
    public TextMeshProUGUI scoreText;

    public override void Init()
    {
        base.Init();

        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.Register(ListenType.UPDATECOUNTMOVE, OnUpdateCountText);
            ListenerManager.Instance.Register(ListenType.UPDATESCORE, OnUpdateScoreText);
        }
    }

    private void Start()
    {
        scoreText.text = ScoreManager.Instance.CurrentScore.ToString();
        moveLeftText.text = GameManager.Instance.movesLeft.ToString();
    }

    private void OnDestroy()
    {
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.Unregister(ListenType.UPDATECOUNTMOVE, OnUpdateCountText);
            ListenerManager.Instance.Unregister(ListenType.UPDATESCORE, OnUpdateScoreText);
        }
    }

    private void OnUpdateScoreText(object value)
    {
        if (value is int score)
        {
            scoreText.text = score.ToString();
        }
    }

    private void OnUpdateCountText(object value)
    {
        if(value is int countMove)
        {
            moveLeftText.text = countMove.ToString();
        }
    }
}