using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePanel : BaseScreen
{
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI moveLeftText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public Image clockImg;
    public ScoreMeter scoreMeter;
    public bool isPause = false;
    public int flashTImeLimit = 10;
    public float flashInterval = 1f;
    public Color fllashColor = Color.red;
    private int maxTime;
    private IEnumerator countdown;

    public override void Init()
    {
        base.Init();

        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.Register(ListenType.UPDATECOUNTMOVE, OnUpdateCountText);
            ListenerManager.Instance.Register(ListenType.UPDATESCORE, OnUpdateScoreText);
        }
    }

    public override void Show(object data)
    {
        base.Show(data);
    }

    private void Start()
    {
        scoreText.text = ScoreManager.Instance.CurrentScore.ToString();
        moveLeftText.text = LevelGoalScore.Instance.moveLeft.ToString();
        InitTimer(LevelGoalScore.Instance.timeLeft);
        scoreMeter.SetupStars();
        countdown = CountdownRoutine();
        StartCoroutine(countdown);
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

    private IEnumerator CountdownRoutine()
    {
        while (LevelGoalScore.Instance.timeLeft > 0)
        {
            isPause = GameManager.Instance.IsGameOver;
            if (isPause) StopCoroutine(countdown);
            yield return new WaitForSeconds(1f);
            LevelGoalScore.Instance.timeLeft--;
            UpdateTime(LevelGoalScore.Instance.timeLeft);
            timeText.text = LevelGoalScore.Instance.timeLeft.ToString();
            if (LevelGoalScore.Instance.timeLeft <= 0)
            {
                GameManager.Instance.EndGame();
                StartCoroutine(GameManager.Instance.WaitForBoardRoutine(0.5f, () =>
                {
                    if (UIManager.HasInstance && GameManager.Instance.IsGameOver)
                    {
                        UIManager.Instance.HideAllScreens();
                        UIManager.Instance.ShowOverlap<FadePanel>();
                        var fadePanel = UIManager.Instance.GetExistOverlap<FadePanel>();
                        if (fadePanel != null)
                        {
                            fadePanel.Fade();
                        }
                        UIManager.Instance.ShowPopup<LosePanel>(true);
                    }
                }));
            }
        }
    }

    private void InitTimer(int maxLevelTime = 60)
    {
        maxTime = maxLevelTime;
        timeText.text = maxTime.ToString();
    }

    private void UpdateTime(int curTime)
    {
        clockImg.fillAmount = (float)curTime / (float)maxTime;
        if(curTime <= flashTImeLimit)
        {
            StartCoroutine(FlashRoutine(clockImg, fllashColor, flashInterval));
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySE(AUDIO.SE_BEEPLOWERSLOWER);
            }
        }
        timeText.text = curTime.ToString();
    }

    private IEnumerator FlashRoutine(Image image, Color targetCor, float interval)
    {
        if(image != null)
        {
            Color originalColor = image.color;
            image.CrossFadeColor(targetCor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);
            image.CrossFadeColor(originalColor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);
        }
    }
}
