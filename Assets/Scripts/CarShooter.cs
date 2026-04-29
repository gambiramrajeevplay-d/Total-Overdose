using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarShooter : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Ammo")]
    public int maxAmmo = 10;
    private int currentAmmo;

    [Header("Fire Rate")]
    public float fireRate = 0.2f;
    private float lastShootTime;

    [Header("Auto Shoot Settings")]
    public float shootRange = 20f;
    public LayerMask targetLayers;

    [Header("UI")]
    public TextMeshProUGUI ammoCounterText;

    [Header("Floating Text")]
    public FloatingText ammoFloatingText;

    [Header("References")]
    public Animator anim; // 👈 ADD THIS
    public PlayerAutoMove playerMove; // 👈 LINK PLAYER SCRIPT

    [Header("Sound")]
    public AudioClip shootClip;
    private AudioSource soundSource;

    [Header("Detection")]
    public float detectRadius = 1.5f;

    private List<Transform> targetList = new List<Transform>();
    private Transform currentTarget;
    private Transform lockedTarget;

    float detectTimer = 0f;
    public float detectInterval = 0.15f;

    private bool permanentlyDisabled = false;

    void Start()
    {
        currentAmmo = maxAmmo;

        GameObject soundObj = GameObject.FindGameObjectWithTag("Sound");
        if (soundObj != null)
            soundSource = soundObj.GetComponent<AudioSource>();

        UpdateAmmoUI();
    }

    void Update()
    {
        if (permanentlyDisabled) return;

        detectTimer += Time.deltaTime;

        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectTargets();
        }

        HandleTargeting();
        HandleShootInput();
    }

    // =======================
    // 🎯 TARGET DETECTION
    // =======================
    void DetectTargets()
    {
        Vector3 dir = firePoint.forward;
        Ray ray = new Ray(firePoint.position, dir);

        targetList.Clear();

        RaycastHit[] hits = Physics.SphereCastAll(ray, detectRadius, shootRange, targetLayers);

        foreach (RaycastHit h in hits)
        {
            Transform t = h.collider.transform.root;

            if (t == transform.root) continue;

            if (!targetList.Contains(t))
                targetList.Add(t);
        }

        // sort by distance
        targetList.Sort((a, b) =>
            Vector3.Distance(firePoint.position, a.position)
            .CompareTo(Vector3.Distance(firePoint.position, b.position)));
    }

    void HandleTargeting()
    {
        targetList.RemoveAll(t => t == null);

        if (lockedTarget != null && targetList.Contains(lockedTarget))
        {
            currentTarget = lockedTarget;
            return;
        }

        if (targetList.Count > 0)
        {
            lockedTarget = targetList[0];
            currentTarget = lockedTarget;
        }
        else
        {
            currentTarget = null;
            lockedTarget = null;
        }
    }

    // =======================
    // 🔫 SHOOT INPUT
    // =======================
    void HandleShootInput()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (Time.time < lastShootTime + fireRate) return;

        if (currentAmmo <= 0)
        {
            ShowOutOfAmmo();
            return;
        }

        lastShootTime = Time.time;

        // 🔥 AUTO AIM ROTATION (Total Overdose style)
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 🔫 ANIMATION
        if (anim != null)
            anim.SetTrigger("Shoot");

        SpawnBullet();
        PlayShootSound();

        currentAmmo--;
        UpdateAmmoUI();
    }

    void SpawnBullet()
    {
        if (!bulletPrefab || !firePoint) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null && currentTarget != null)
        {
            bulletScript.SetTarget(currentTarget);
        }
    }

    void PlayShootSound()
    {
        if (soundSource != null && shootClip != null)
        {
            soundSource.pitch = Random.Range(0.9f, 1.1f);
            soundSource.PlayOneShot(shootClip);
        }
    }

    // =======================
    // 💀 CALLED FROM BULLET / ENEMY
    // =======================
    public void OnEnemyKilled(PostKillAction action)
    {
        if (anim == null) return;

        switch (action)
        {
            case PostKillAction.Dodge:
                anim.SetTrigger("Dodge");
                break;

            case PostKillAction.Roll:
                anim.SetTrigger("Roll");
                break;
        }
    }

    // =======================
    // UI
    // =======================
    void UpdateAmmoUI()
    {
        if (ammoCounterText != null)
            ammoCounterText.text = $"{currentAmmo}/{maxAmmo}";
    }

    void ShowOutOfAmmo()
    {
        if (ammoFloatingText != null)
        {
            ammoFloatingText.gameObject.SetActive(true);
            ammoFloatingText.SetText("OUT OF AMMO", Color.red);
        }
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    public void ForceStop()
    {
        permanentlyDisabled = true;
    }
}