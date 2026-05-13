using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Enemies")]
    public List<Enemy> enemies = new List<Enemy>();

    [Header("UI")]
    public TextMeshProUGUI killCountText;

    private int aliveCount;
    private int totalEnemies;
    private int killedEnemies;

    public bool AllEnemiesDead => aliveCount <= 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // auto find kill count text
        GameObject textObj = GameObject.FindGameObjectWithTag("KillCount");

        if (textObj != null)
        {
            killCountText = textObj.GetComponent<TextMeshProUGUI>();
        }

        Enemy[] found = FindObjectsOfType<Enemy>();

        enemies.Clear();
        enemies.AddRange(found);

        aliveCount = enemies.Count;

        totalEnemies = aliveCount;

        killedEnemies = 0;

        UpdateKillUI();
    }

    // =========================
    // REGISTER ENEMY
    // =========================
    public void RegisterEnemy(Enemy e)
    {
        if (!enemies.Contains(e))
        {
            enemies.Add(e);

            aliveCount++;
            totalEnemies++;

            UpdateKillUI();
        }
    }

    // =========================
    // ENEMY DIED
    // =========================
    public void RegisterEnemyDeath(Enemy e)
    {
        // prevent double count
        if (!enemies.Contains(e))
            return;

        enemies.Remove(e);

        aliveCount--;

        killedEnemies++;

        UpdateKillUI();

        // optional debug
        // Debug.Log("Enemy died. Remaining: " + aliveCount);
    }

    // =========================
    // UPDATE UI
    // =========================
    void UpdateKillUI()
    {
        if (killCountText != null)
        {
            killCountText.text =
                "Kill Count : " +
                killedEnemies +
                " / " +
                totalEnemies;
        }
    }
}