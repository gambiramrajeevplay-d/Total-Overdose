using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAutoMove : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> points;
    private int currentIndex = 0;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.2f;

    [Header("References")]
    public Rigidbody rb;
    public Animator anim;

    [Header("Combat")]
    public float detectionRadius = 10f;
    public LayerMask enemyLayer;

    private HitBox currentHitBox;

    private bool isInCombat = false;
    private bool isShooting = false;
    private bool isPerformingAction = false;
    private bool isDead = false;

    // 🔥 NEW INPUT LOCK
   


    [Header("Aim UI")]
    public GameObject aimSprite;
    public float aimHeightOffset = 1.6f;

    [Header("Aim Animation")]
    public float aimScale = 1.1f;
    public float aimRotateSpeed = 200f;
    private bool aimInitialized = false;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private List<Bullet> spawnedBullets = new List<Bullet>();

    [HideInInspector] public float lastActionTime;

    [Header("Slow Motion")]
    public float actionSlowScale = 0.08f;
    public float actionDuration = 0.4f;
    public float returnSpeed = 1.5f;

    private bool isSlowMotionActive = false;
    private bool useRootMotion = false;

    [Header("Post Kill Timing")]
    public float postKillDuration = 1.5f;

    // 🔥 NEW VARIABLES
    private bool moveWithAction = false;
    private Transform actionTarget;
    private bool isMovingAfterKill = false;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        anim.updateMode = AnimatorUpdateMode.Normal;
        anim.speed = 1f;

        lastActionTime = Time.time;

        if (aimSprite != null)
            aimSprite.SetActive(false);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isInCombat && !isShooting)
        {
            HandleMovement();
        }
    }

    void Update()
    {
        if (isDead) return;

        DetectEnemy();
        HandleShoot();

        HandleActionMovement();

        if (isInCombat && currentHitBox != null)
        {
            LookAtEnemy();

            if (aimSprite != null)
            {
                if (!aimSprite.activeSelf)
                {
                    aimSprite.SetActive(true);
                    aimInitialized = false;
                }

                UpdateAimPosition();
                AnimateAim();
            }
        }
        else
        {
            if (aimSprite != null && aimSprite.activeSelf)
                aimSprite.SetActive(false);
        }
    }

    void HandleActionMovement()
    {
        if (!moveWithAction || actionTarget == null) return;

        Vector3 dir = actionTarget.position - transform.position;
        dir.y = 0;

        float distance = dir.magnitude;

        if (distance > stoppingDistance)
        {
            Vector3 moveDir = dir.normalized;

            float speedMultiplier = Mathf.Clamp01(distance / 2f);
            float targetSpeed = moveSpeed * 1.8f * speedMultiplier;

            Vector3 targetVelocity = new Vector3(
                moveDir.x * targetSpeed,
                rb.velocity.y,
                moveDir.z * targetSpeed
            );

            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 8f);

            Quaternion rot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime * 2f);
        }
        else
        {
            moveWithAction = false;
            isPerformingAction = false;
            isMovingAfterKill = false;

            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    // 🔥 NEW: CALLED FROM ANIMATION EVENT
    public void OnActionComplete()
    {
        isPerformingAction = false;
    }
    void UpdateAimPosition()
    {
        if (aimSprite == null || currentHitBox == null) return;

        Vector3 worldPos = currentHitBox.transform.position + Vector3.up * aimHeightOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z > 0)
        {
            aimSprite.transform.position = Vector3.Lerp(
                aimSprite.transform.position,
                screenPos,
                Time.deltaTime * 10f
            );
        }
    }

    void AnimateAim()
    {
        if (aimSprite == null) return;

        if (!aimInitialized)
        {
            aimSprite.transform.localScale = Vector3.one * aimScale;
            aimInitialized = true;
        }

        aimSprite.transform.Rotate(0, 0, aimRotateSpeed * Time.deltaTime);
    }

    public void RegisterAction()
    {
        lastActionTime = Time.time;
    }

    public void KillPlayer()
    {
        if (isDead) return;

        isDead = true;

        isShooting = false;
        CancelInvoke();

        foreach (Bullet b in spawnedBullets)
        {
            if (b != null)
                Destroy(b.gameObject);
        }
        spawnedBullets.Clear();

        rb.velocity = Vector3.zero;

        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        anim.SetTrigger("Die");

        if (aimSprite != null)
            aimSprite.SetActive(false);

        StartCoroutine(HandleDeathSequence());
    }

    IEnumerator HandleDeathSequence()
    {
        yield return new WaitForSecondsRealtime(3f);
        GameManager.Instance.OnPlayerDied();
    }

    void HandleMovement()
    {
        if (points == null || points.Count == 0) return;

        if (currentIndex >= points.Count)
        {
            anim.SetBool("isRunning", false);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);

            if (EnemyManager.Instance.AllEnemiesDead)
            {
                GameManager.Instance.OnWin();
            }

            return;
        }

        Transform target = points[currentIndex];

        Vector3 direction = target.position - rb.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > stoppingDistance)
        {
            Vector3 moveDir = direction.normalized;

            rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);

            Quaternion rot = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rot, rotationSpeed * Time.fixedDeltaTime));

            anim.SetBool("isRunning", true);
            RegisterAction();
        }
        else
        {
            currentIndex++;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            anim.SetBool("isRunning", false);
        }
    }

    void DetectEnemy()
    {
        if (currentHitBox != null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

        if (hits.Length == 0)
        {
            isInCombat = false;
            currentHitBox = null;
            return;
        }

        foreach (Collider hit in hits)
        {
            HitBox hb = hit.GetComponent<HitBox>();
            if (hb == null) continue;

            currentHitBox = hb;
            isInCombat = true;

            rb.velocity = Vector3.zero;
            anim.SetBool("isRunning", false);
            break;
        }
    }

    void LookAtEnemy()
    {
        Vector3 dir = currentHitBox.transform.position - transform.position;
        dir.y = 0;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }

    void HandleShoot()
    {
        if (!isInCombat || currentHitBox == null || isShooting || isDead || isPerformingAction)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            isShooting = true;

            anim.ResetTrigger("Shoot");
            anim.SetTrigger("Shoot");

            Invoke(nameof(ShootBullet), 0.2f);
            Invoke(nameof(EndShoot), 0.4f);
        }
    }

    void ShootBullet()
    {
        if (currentHitBox == null || isDead) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector3 dir = (currentHitBox.transform.position - firePoint.position).normalized;
        bullet.transform.forward = dir;

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.SetTarget(currentHitBox.transform);
            b.owner = Bullet.BulletOwner.Player;
            spawnedBullets.Add(b);
        }
    }

    void EndShoot()
    {
        isShooting = false;
    }

    public void OnEnemyKilled(PostKillAction action)
    {
        if (currentIndex >= points.Count) return;

        actionTarget = points[currentIndex];

        moveWithAction = true;
        isPerformingAction = true;
        isMovingAfterKill = true;

        isInCombat = false;

        // 🔥 LOCK INPUT HERE
        isPerformingAction = true;
        TriggerSlowMotion();

        if (action == PostKillAction.Dodge)
            anim.SetTrigger("Dodge");
        else
            anim.SetTrigger("Roll");
    }

    public void TriggerSlowMotion()
    {
        if (!isSlowMotionActive)
            StartCoroutine(SlowMotion());
    }

    IEnumerator SlowMotion()
    {
        isSlowMotionActive = true;

        Time.timeScale = actionSlowScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(actionDuration);

        while (Time.timeScale < 1f)
        {
            Time.timeScale += Time.unscaledDeltaTime * returnSpeed;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isSlowMotionActive = false;
    }
}