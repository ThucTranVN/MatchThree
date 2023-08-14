using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadePanel : BaseOverlap
{
    public Image fadeImg;
    public float solidAlpha = 1f;
    public float clearAlpha = 0f;
    public float delay = 0f;
    public float timeToFade = 1f;

    public void FadeOn(Action onComplete = null)
    {
        fadeImg.DOFade(solidAlpha, timeToFade).OnComplete(() =>
        {
            this.Hide();
            onComplete?.Invoke();
        });
    }

    public void FadeOff(Action onComplete = null)
    {
        fadeImg.DOFade(clearAlpha, timeToFade).OnComplete(() =>
        {
            this.Hide();
            onComplete?.Invoke();
        });
    }

}
