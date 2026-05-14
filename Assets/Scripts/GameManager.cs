using UnityEngine;
using UnityEngine.SceneManagement;
using Script;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Current Level Root")]
    public GameObject currentLevel;

    [Header("Level Settings")]
    public int currentLevelIndex = 1;

    [Header("Result Audio")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private bool gameEnded = false;

    [Header("Start UI")]
    public GameObject startButton;
    public GameObject startText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
           Destroy(gameObject);
            return;
        }

       

    }

    void Start()
    {
        // 🔥 AUTO FIND UI

        if (winPanel == null)
        {
            GameObject passObj =
                GameObject.FindGameObjectWithTag("Pass");

            if (passObj != null)
                winPanel = passObj;
        }

        if (losePanel == null)
        {
            GameObject failObj =
                GameObject.FindGameObjectWithTag("Fail");

            if (failObj != null)
                losePanel = failObj;
        }

        if (currentLevel == null)
        {
            GameObject levelObj =
                GameObject.FindGameObjectWithTag("Level");

            if (levelObj != null)
                currentLevel = levelObj;
        }

        // 🔥 RESET UI

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        Time.timeScale = 0f;
        Pauser.LockPause();

        // 🔥 AUTO FIND START UI

        if (startButton == null)
        {
            startButton =
                GameObject.Find("StartButton");
        }

        if (startText == null)
        {
            startText =
                GameObject.Find("StartText");
        }

      

        // 🔥 AUTO FIND START UI

        if (startButton == null)
        {
            startButton =
                GameObject.Find("StartButton");
        }

        if (startText == null)
        {
            startText =
                GameObject.Find("StartText");
        }

        // 🔥 ASSIGN BUTTON EVENT

        if (startButton != null)
        {
            UnityEngine.UI.Button btn =
                startButton.GetComponent<UnityEngine.UI.Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(StartGame);
            }

            startButton.SetActive(true);
        }

        // 🔥 SHOW TEXT

        if (startText != null)
        {
            startText.SetActive(true);
        }
    }
    public void StartGame()
    {
        Time.timeScale = 1f;

        if (startButton != null)
            startButton.SetActive(false);

        if (startText != null)
            startText.SetActive(false);

        PlayerAutoMove player =
      FindObjectOfType<PlayerAutoMove>();

        if (player != null)
        {
            player.StartGameplay();
        }

        // 🔥 ENABLE PAUSE
        Pauser.UnlockPause();
    }
    // =========================
    // WIN
    // =========================
    public void OnWin()
    {
        if (gameEnded) return;

        gameEnded = true;
        Pauser.LockPause();

        // ❌ NO REWARD IN TUTORIAL
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            CurrecnyManager.instance?.AddCurrency(100);

            Debug.Log("100 Coins Rewarded");
        }
        else
        {
            Debug.Log("Tutorial completed - no coins rewarded");
        }

        UnlockNextLevel();

        StopAllGameAudio();

        PlayResultSound(winClip);

        if (winPanel != null)
            winPanel.SetActive(true);

        DisableLevel();

        Time.timeScale = 0f;
    }
    // =========================
    // LOSE
    // =========================
    public void OnPlayerDied()
    {
        if (gameEnded) return;

        gameEnded = true;
        Pauser.LockPause();

        StopAllGameAudio();

        PlayResultSound(loseClip);

        if (losePanel != null)
            losePanel.SetActive(true);

        DisableLevel();

        Time.timeScale = 0f;
    }

    // =========================
    // STOP ALL GAME AUDIO
    // =========================
    void StopAllGameAudio()
    {
        AudioSource[] allAudio =
            FindObjectsOfType<AudioSource>();

        foreach (AudioSource audioSource in allAudio)
        {
            audioSource.Stop();
        }
    }

    // =========================
    // PLAY RESULT SOUND
    // =========================
    void PlayResultSound(AudioClip clip)
    {
        if (clip == null)
            return;

        GameObject audioObj =
            new GameObject("ResultAudio");

        AudioSource source =
            audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.playOnAwake = false;

        // important while paused
        source.ignoreListenerPause = true;

        source.Play();

        Destroy(audioObj, clip.length);
    }

    // =========================
    // UNLOCK NEXT LEVEL
    // =========================
    void UnlockNextLevel()
    {
        // ❌ DO NOT UNLOCK FROM TUTORIAL
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            Debug.Log("Tutorial completed - next level NOT unlocked");
            return;
        }

        int unlockedLevel =
            PlayerPrefs.GetInt(StringsData.playerLevel, 1);

        if (currentLevelIndex >= unlockedLevel)
        {
            PlayerPrefs.SetInt(
                StringsData.playerLevel,
                currentLevelIndex + 1
            );

            PlayerPrefs.Save();

            Debug.Log("Unlocked Level: " + (currentLevelIndex + 1));
        }
    }

    // =========================
    // DISABLE CURRENT LEVEL
    // =========================
    void DisableLevel()
    {
        if (currentLevel != null)
        {
            currentLevel.SetActive(false);
        }
    }

    // =========================
    // RESTART
    // =========================
    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================
    // HOME
    // =========================
    public void GoHome()
    {
        Time.timeScale = 1f;

        // 🔥 SHOW SUBSCRIPTION AFTER GAMEPLAY
        PlayerPrefs.SetInt("ShowSubscriptionPanel", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(0);
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}