using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTime : BaseManager<LevelGoalTime>
{
    public int timeLeft = 60;
    private int maxTime;
    public int MaxTime => maxTime;

    private void Start()
    {
        maxTime = timeLeft;
    }

    public void AddTime(int timeValue)
    {
        timeLeft += timeValue;
        timeLeft = Mathf.Clamp(timeLeft, 0, maxTime);

        if (UIManager.HasInstance)
        {
            UIManager.Instance.GetExistScreen<GamePanel>().UpdateTime(timeLeft);
        }
    }
}
