using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public List<Enemy> enemies = new List<Enemy>();
    private int aliveCount;

    public bool AllEnemiesDead => aliveCount <= 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Enemy[] found = FindObjectsOfType<Enemy>();

        enemies.Clear();
        enemies.AddRange(found);

        aliveCount = enemies.Count;
    }

    // 🔥 OPTIONAL: call this when spawning enemies dynamically
    public void RegisterEnemy(Enemy e)
    {
        if (!enemies.Contains(e))
        {
            enemies.Add(e);
            aliveCount++;
        }
    }

    public void RegisterEnemyDeath(Enemy e)
    {
        // 🔥 IMPORTANT: prevent double counting
        if (!enemies.Contains(e)) return;

        enemies.Remove(e); // 🔥 remove from list
        aliveCount--;

        // 🔥 DEBUG (optional)
        // Debug.Log("Enemy died. Remaining: " + aliveCount);
    }
}