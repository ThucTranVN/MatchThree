using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinPanel : BasePopup
{
    public TextMeshProUGUI scoreText;

    public override void Init()
    {
        base.Init();

        if (ScoreManager.HasInstance)
        {
            scoreText.text = "Your score: \n" + ScoreManager.Instance.CurrentScore.ToString();
        }
        if (AudioManager.HasInstance)
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_WIN1);
        }
    }

    public override void Show(object data)
    {
        base.Show(data);

        if (ScoreManager.HasInstance)
        {
            scoreText.text = "Your score: \n" +  ScoreManager.Instance.CurrentScore.ToString();
        }
        if (AudioManager.HasInstance)
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_WIN1);
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
