using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public class MessagePanel : BaseNotify
{
    public GameObject moveObj;
    public float startValue;
    public float endValue = 0;
    public float timeToMove = 1f;

    public TextMeshProUGUI scoreText;

    private void Start()
    { 
        startValue = moveObj.transform.localPosition.y;
    }

    public void MoveOn(Action onComplete = null)
    {
        moveObj.transform.DOLocalMoveY(endValue, timeToMove).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void MoveOff(Action onComplete = null)
    {
        UIManager.Instance.ShowOverlap<FadePanel>();
        var fadePanel = UIManager.Instance.GetExistOverlap<FadePanel>();
        if (fadePanel != null)
        {
            fadePanel.Fade();
        }
        moveObj.transform.DOLocalMoveY(-startValue, timeToMove).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void ShowMessage(string scoreTxt="")
    {
        if (scoreText)
        {
            scoreText.text = scoreTxt;
        }
    }


    public void StartGame()
    {
        MoveOff(OnCompleteMoveOff);
    }

    private void OnCompleteMoveOff()
    {
        this.Hide();
        GameManager.Instance.StartGame();
    }
}
