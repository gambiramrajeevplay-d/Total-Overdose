using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject winPanel;
    public GameObject losePanel;

    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // 🟢 WIN
    public void OnWin()
    {
        if (gameEnded) return;

        gameEnded = true;

        Time.timeScale = 0f;

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    // 🔴 LOSE (UPDATED FLOW)
    public void OnPlayerDied()
    {
        if (gameEnded) return;

        gameEnded = true;

        // show UI first
        if (losePanel != null)
            losePanel.SetActive(true);

        // then freeze game
        Time.timeScale = 0f;
    }

    // 🔄 RESTART
    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}