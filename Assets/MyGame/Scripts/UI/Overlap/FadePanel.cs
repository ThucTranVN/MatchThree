using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadePanel : BaseOverlap
{
    public Image fadeImg;

    public override void Show(object data)
    {
        base.Show(data);
    }

    public void Fade(float timeToFade = 0.5f, Action onComplete = null)
    {
        FadeOn(timeToFade,() =>
        {
            FadeOff(timeToFade,onComplete);
        });
    }

    private void FadeOn(float timeToFade, Action onComplete = null)
    {
        fadeImg.DOFade(0.5f, timeToFade).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    private void FadeOff(float timeToFade,Action onComplete = null)
    {
        fadeImg.DOFade(0, timeToFade).OnComplete(() =>
        {
            this.Hide();
            onComplete?.Invoke();
        });
    }

}
