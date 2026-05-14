using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    private ThirdPersonCamera cam;

    [Header("Audio")]
    public AudioClip footstepSound;
    public AudioClip actionSound;
    public AudioClip deathSound;
    public AudioClip shootSound;

    [Header("Footstep Settings")]
    public int footstepPriority = 200;

    [Header("Action Settings")]
    public float actionSoundDelay = 2f;
    public int actionPriority = 50;

    private AudioSource footstepSource;

    [Header("Post Kill Movement")]
    public float moveAfterKillDelay = 1f;
    public float movementSmoothness = 8f;
    public float rotationSmoothness = 12f;
    private bool gameStarted = false;

    void Start()
    {
        SetupControls();
        GameObject parent = GameObject.FindGameObjectWithTag("Points");
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (parent == null)
        {
            Debug.LogError("Points parent not found!");
            return;
        }

        points = new List<Transform>();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            points.Add(parent.transform.GetChild(i));
        }

        if (rb == null)
            rb = GetComponent<Rigidbody>();
        cam = FindObjectOfType<ThirdPersonCamera>();
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        anim.speed = 1f;

        lastActionTime = Time.time;

        if (aimSprite != null)
            aimSprite.SetActive(false);
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepSound;
        footstepSource.loop = true;
        footstepSource.spatialBlend = 1f;
        footstepSource.playOnAwake = false;
        footstepSource.priority = footstepPriority;
        rb.velocity = Vector3.zero;
        anim.SetBool("isRunning", false);
    }
    void SetupControls()
    {
        GameObject btnObj =
            GameObject.FindGameObjectWithTag("ShootButton");

        if (btnObj == null)
            return;

        if (PlatformManager.Instance.IsMobile())
        {
            btnObj.SetActive(true);

            Button btn = btnObj.GetComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(MobileShoot);
        }
        else
        {
            btnObj.SetActive(false);
        }
    }
    public void StartGameplay()
    {
        gameStarted = true;

        anim.updateMode =
            AnimatorUpdateMode.Normal;
    }
    void FixedUpdate()
    {
        if (isDead) return;

        if (!gameStarted)
        {
            rb.velocity = Vector3.zero;
            anim.SetBool("isRunning", false);
            return;
        }

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
        if (!moveWithAction || actionTarget == null)
            return;

        Vector3 dir = actionTarget.position - transform.position;
        dir.y = 0f;

        float distance = dir.magnitude;

        // stop instantly
        if (distance <= stoppingDistance)
        {
            moveWithAction = false;
            isPerformingAction = false;
            isMovingAfterKill = false;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            anim.SetBool("isRunning", false);

            return;
        }

        Vector3 moveDir = dir.normalized;

        // constant clean movement
        Vector3 velocity = moveDir * (moveSpeed * 1.5f);
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;

        // smooth rotation only
        Quaternion targetRot = Quaternion.LookRotation(moveDir);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSmoothness * Time.deltaTime
            )
        );

        anim.SetBool("isRunning", true);
    }

    // 🔥 NEW: CALLED FROM ANIMATION EVENT
    public void OnActionComplete()
    {
        moveWithAction = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isPerformingAction = false;
        isMovingAfterKill = false;

        anim.SetBool("isRunning", false);
    }
    public void StartActionMove()
    {
        moveWithAction = true;
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
        Pauser.LockPause();

        // 🔊 PLAY ONE SHOT DEATH SOUND
        PlayOneShotSound(deathSound, transform.position);

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
        if (cam != null)
        {
            cam.SetDeadView(true);
        }

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

            if (!isShooting && !isPerformingAction)
            {
                if (!footstepSource.isPlaying && footstepSound != null)
                {
                    footstepSource.Play();
                }
            }
            else
            {
                if (footstepSource.isPlaying)
                {
                    footstepSource.Stop();
                }
            }

        }
        else
        {
            currentIndex++;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            anim.SetBool("isRunning", false);

            if (footstepSource.isPlaying)
                footstepSource.Stop();
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

            if (cam != null)
                cam.SetShooting(false);

            return;
        }

        foreach (Collider hit in hits)
        {
            HitBox hb = hit.GetComponent<HitBox>();
            if (hb == null) continue;

            currentHitBox = hb;
            isInCombat = true;
            if (cam != null) cam.SetShooting(true);
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
        if (!gameStarted)
            return;

        if (!isInCombat || currentHitBox == null || isShooting || isDead || moveWithAction || isPerformingAction)
            return;
        

        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }

        bool shootPressed = false;

#if UNITY_EDITOR

        if (PlatformManager.Instance.IsTV())
        {
            shootPressed =
                Input.GetKeyDown(KeyCode.JoystickButton0);
        }
        else
        {
            shootPressed =
                Input.GetKeyDown(KeyCode.Space);
        }

#else

if (PlatformManager.Instance.IsTV())
{
    shootPressed =
        Input.GetKeyDown(KeyCode.JoystickButton0);
}

#endif
        // KEYBOARD TESTING
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            shootPressed = true;
        }
#endif

        if (shootPressed)
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

        // 🔥 instant gunshot sound
        PlayOneShotSound(shootSound, transform.position, 40);

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Vector3 dir =
            (currentHitBox.transform.position - firePoint.position).normalized;

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

        // only disable if truly out of combat
        if (!isInCombat && cam != null)
        {
            cam.SetShooting(false);
           
        }
    }
    public void OnEnemyKilled(PostKillAction action)
    {
        if (currentIndex >= points.Count) return;

        StartCoroutine(HandlePostKill(action));
        
    }
    IEnumerator HandlePostKill(PostKillAction action)
    {
        // stay in combat state while enemy dies
        isShooting = true;

        // keep camera in shooting mode
        if (cam != null)
            cam.SetShooting(true);

        // stop movement
        rb.velocity = Vector3.zero;

        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }

        // wait for enemy death animation
        yield return new WaitForSeconds(moveAfterKillDelay);

        // NOW exit combat
        isInCombat = false;
        isShooting = false;
        currentHitBox = null;

        actionTarget = points[currentIndex];

        moveWithAction = false;
        isPerformingAction = true;
        isMovingAfterKill = true;
        StartCoroutine(PlayActionSoundWithDelay());

        TriggerSlowMotion();

        // roll instantly after delay
        switch (action)
        {
            case PostKillAction.Dodge:
                anim.SetTrigger("Dodge");
                break;

            case PostKillAction.Roll:
                anim.SetTrigger("Roll");
                break;

            case PostKillAction.Jump:
                anim.SetTrigger("Jump");
                break;
        }
    }
    IEnumerator PlayActionSoundWithDelay()
    {
        yield return new WaitForSeconds(actionSoundDelay);

        if (actionSound == null) yield break;

        GameObject audioObj = new GameObject("ActionSound");
        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = actionSound;
        source.spatialBlend = 1f;
        source.priority = actionPriority;
        source.Play();

        Destroy(audioObj, actionSound.length);
    }
    public void TriggerSlowMotion()
    {
        Pauser.LockPause();
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
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            Pauser.UnlockPause();
        }
    }
    public void MobileShoot()
    {
        if (!gameStarted)
            return;

        if (!isInCombat || currentHitBox == null || isShooting || isDead || isPerformingAction)
            return;

        isShooting = true;

        anim.ResetTrigger("Shoot");
        anim.SetTrigger("Shoot");

        Invoke(nameof(ShootBullet), 0.2f);
        Invoke(nameof(EndShoot), 0.4f);
    }
    void PlayOneShotSound(AudioClip clip, Vector3 position, int priority = 128)
    {
        if (clip == null) return;

        GameObject audioObj = new GameObject("OneShotAudio");
        audioObj.transform.position = position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;
        source.priority = priority;
        source.playOnAwake = false;

        source.Play();

        Destroy(audioObj, clip.length);
    }
    public void ForceStopShooting()
    {
        CancelInvoke(nameof(ShootBullet));
        CancelInvoke(nameof(EndShoot));

        isShooting = false;

        // 🔥 RESET SHOOT STATE
        anim.ResetTrigger("Shoot");

        // optional but safer
        anim.Play("Idle", 0, 0f);
    }
}