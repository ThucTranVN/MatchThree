using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : BaseScreen
{
    public void PlayGameBtnClick()
    {
        if (UIManager.HasInstance)
        {
            this.Hide();
            UIManager.Instance.ShowOverlap<FadePanel>();
            var fadePanel = UIManager.Instance.GetExistOverlap<FadePanel>();
            if (fadePanel != null)
            {
                fadePanel.Fade();
            }
            OnCompleteBtnClick();
        }
    }

    private void OnCompleteBtnClick()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowNotify<MessagePanel>();
            var messagePanel = UIManager.Instance.GetExistNotify<MessagePanel>();
            if (messagePanel != null)
            {
                messagePanel.ShowMessage("Score Goal \n" + LevelGoalScore.Instance.scoreGoals[0].ToString());
                messagePanel.MoveOn();
            }
        }
    }
}   
