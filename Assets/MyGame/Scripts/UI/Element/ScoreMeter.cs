using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMeter : MonoBehaviour
{
    public Slider slider;
    public ScoreStar[] scoreStars = new ScoreStar[3];
    private int maxScore;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetupStars()
    {
        maxScore = LevelGoalScore.Instance.GetMaxScore();
        slider.value = 0;
        slider.maxValue = maxScore;
        float sliderWidth = slider.GetComponent<RectTransform>().rect.width;
        if(maxScore > 0)
        {
            for (int i = 0; i < LevelGoalScore.Instance.scoreGoals.Length; i++)
            {
                if(scoreStars[i] != null)
                {
                    float newX = (sliderWidth * LevelGoalScore.Instance.scoreGoals[i] / maxScore) - (sliderWidth * 0.5f);
                    Debug.Log(newX);
                    RectTransform starRectXform = scoreStars[i].GetComponent<RectTransform>();
                    if(starRectXform != null)
                    {
                        starRectXform.anchoredPosition = new Vector2(newX, starRectXform.anchoredPosition.y);
                    }
                }
            }
        }
    }

    public void UpdateScoreMeter(int score, int starCount)
    {
        if (LevelGoalScore.HasInstance)
        {
            slider.value = (float)score;
        }

        for (int i = 0; i < starCount; i++)
        {
            if(scoreStars[i] != null)
            {
                scoreStars[i].Activate();
            }
        }
    }
}
