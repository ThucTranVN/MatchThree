using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalScore : BaseManager<LevelGoalScore>
{
    public int scoreStarts = 0;
    public int[] scoreGoals = new int[3] { 1000, 2000, 3000 };
    public int moveLeft = 30;


    void Start()
    {
        Init();
    }

    private void Init()
    {
        scoreStarts = 0;
        for (int i = 1; i < scoreGoals.Length; i++)
        {
            if(scoreGoals[i] < scoreGoals[i - 1])
            {
                Debug.LogWarning("Levelgoal setup score goals in increasing order");
            }
        }
    }

    private int UpdateScore(int score)
    {
        for (int i = 0; i < scoreGoals.Length; i++)
        {
            if(score < scoreGoals[i])
            {
                return i;
            }
        }
        return scoreGoals.Length;
    }


    public void UpdateScoreStarts(int score)
    {
        scoreStarts = UpdateScore(score);
    }

    public int GetMaxScore()
    {
        return scoreGoals[scoreGoals.Length - 1];
    }
}
