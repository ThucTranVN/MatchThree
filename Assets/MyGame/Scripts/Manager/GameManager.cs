using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : BaseManager<GameManager>
{
    [SerializeField]
    private Board curBoard;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    private string sceneName = "";
    public string CurrentSceneName => sceneName;

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        sceneName = scene.name;

        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowScreen<MenuPanel>();
        }

    }

    public void StartGame()
    {
        if (curBoard != null)
        {
            curBoard.SetupBoard();
        }

        StartCoroutine(WaitForBoardRoutine(0.2f, () =>
        {
            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowScreen<GamePanel>();
                var gamePanel = UIManager.Instance.GetExistScreen<GamePanel>();
                if (gamePanel != null)
                {
                    gamePanel.levelNameText.text = sceneName;
                }
            }
        })); 
    }

    public IEnumerator WaitForBoardRoutine(float delay = 0f, Action onComplete = null)
    {
        if(curBoard != null)
        {
            yield return new WaitForSeconds(curBoard.swapTime);
            while (curBoard.isRefilling)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGame()
    {
        isGameOver = true;
    }
}
