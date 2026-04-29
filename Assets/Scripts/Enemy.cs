using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Settings")]
    public PostKillAction postKillAction;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Detection")]
    public float detectionRange = 10f;

    [Header("Shooting Timing")]
    public float firstShotDelay = 1f;
    public float shootInterval = 2f;

    private float nextShootTime;
    private bool hasStartedShooting = false;

    private PlayerAutoMove player;
    private PlayerHitBox playerHitBox;

    public Animator animator;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private bool isDead = false;
    private bool deathTriggered = false; // 🔥 NEW (prevents double trigger)

    public SpriteHealthBar healthBar;

    // 🔥 Track bullets fired by this enemy
    private List<Bullet> spawnedBullets = new List<Bullet>();

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.UpdateHealth(1f);

            if (healthBar.fill != null)
                healthBar.fill.localScale = Vector3.one;
        }

        player = FindObjectOfType<PlayerAutoMove>();

        if (player != null)
            playerHitBox = player.GetComponentInChildren<PlayerHitBox>();
    }

    void Update()
    {
        if (isDead || playerHitBox == null) return;

        float distance = Vector3.Distance(transform.position, playerHitBox.transform.position);

        if (distance <= detectionRange)
        {
            LookAtPlayer();

            if (animator != null)
                animator.SetBool("IsAiming", true);

            HandleShooting();
        }
    }

    void HandleShooting()
    {
        if (isDead) return;

        if (!hasStartedShooting)
        {
            hasStartedShooting = true;
            nextShootTime = Time.time + firstShotDelay;
            return;
        }

        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }
    }

    void LookAtPlayer()
    {
        Vector3 dir = playerHitBox.transform.position - transform.position;
        dir.y = 0;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
    }

    void Shoot()
    {
        if (isDead) return;

        if (animator != null)
            animator.SetTrigger("Shoot");

        Invoke(nameof(ShootBullet), 0.2f);
    }

    void ShootBullet()
    {
        if (isDead) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector3 dir = (playerHitBox.transform.position - firePoint.position).normalized;
        bullet.transform.forward = dir;

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.SetTarget(playerHitBox.transform);
            b.owner = Bullet.BulletOwner.Enemy;

            spawnedBullets.Add(b);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        float percent = (float)currentHealth / maxHealth;

        if (healthBar != null)
            healthBar.UpdateHealth(percent);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathTriggered) return; // 🔥 prevent duplicate calls
        deathTriggered = true;

        isDead = true;

        CancelInvoke();

        if (animator != null)
            animator.SetTrigger("Die");

        // 🔥 destroy all bullets fired by this enemy
        foreach (Bullet b in spawnedBullets)
        {
            if (b != null)
                Destroy(b.gameObject);
        }

        spawnedBullets.Clear();

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemyDeath(this);

        // 🔥 THIS is where your flow starts
        if (player != null)
            player.OnEnemyKilled(postKillAction);

        Destroy(gameObject, 2f);
    }
}