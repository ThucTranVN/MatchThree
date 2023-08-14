using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : BaseManager<GameManager>
{
    public int movesLeft = 30;
    public int scoreGoal = 10000;

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

        StartCoroutine("WaitForBoardRoutine", 0.5f);

        if (UIManager.HasInstance)
        {
            UIManager.Instance.ShowScreen<GamePanel>();
            var gamePanel = UIManager.Instance.GetExistScreen<GamePanel>();
            if (gamePanel != null)
            {
                gamePanel.levelNameText.text = sceneName;
            }
        }
    }

    private IEnumerator WaitForBoardRoutine(float delay = 0f)
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
