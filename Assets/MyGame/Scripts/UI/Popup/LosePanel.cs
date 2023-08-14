using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LosePanel : BasePopup
{
    public TextMeshProUGUI scoreText;

    public override void Init()
    {
        base.Init();

        if (ScoreManager.HasInstance)
        {
            scoreText.text = "Your score: \n" + ScoreManager.Instance.CurrentScore.ToString();
        }
    }

    public override void Show(object data)
    {
        base.Show(data);

        if (ScoreManager.HasInstance)
        {
            scoreText.text = "Your score: \n" + ScoreManager.Instance.CurrentScore.ToString();
        }
    }

    public void OnClickOkButton()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.Restart();
        }
    }
}
