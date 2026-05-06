using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    private Transform target;

    [Header("Normal Offset")]
    public Vector3 normalOffset = new Vector3(0, 3, -5);

    [Header("Shooting Offset")]
    public Vector3 shootingOffset = new Vector3(1.5f, 2.5f, -3f);

    [Header("Smooth Settings")]
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;
    public float transitionSpeed = 8f;

    [Header("Dead Offset")]
    public Vector3 deadOffset = new Vector3(0, 8f, 0);

    [Header("Dead Rotation")]
    public Vector3 deadRotation = new Vector3(90f, 0f, 0f);

    [Header("Combat Hold")]
    public float shootingHoldTime = 0.3f;

    private Vector3 currentOffset;
    private Vector3 currentVelocity;

    private bool isShooting = false;
    private bool isDeadView = false;

    // 🔥 prevents instant snap back
    private float lastShootTime;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player_Main");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player_Main' not found!");
        }

        currentOffset = normalOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 🔥 hold shooting camera briefly
        bool keepShootingView =
            isShooting ||
            Time.time - lastShootTime < shootingHoldTime;

        Vector3 targetOffset;

        if (isDeadView)
        {
            targetOffset = deadOffset;
        }
        else if (keepShootingView)
        {
            targetOffset = shootingOffset;
        }
        else
        {
            targetOffset = normalOffset;
        }

        // smooth transition
        currentOffset = Vector3.Lerp(
            currentOffset,
            targetOffset,
            transitionSpeed * Time.deltaTime
        );

        // desired position
        Vector3 desiredPosition;

        if (isDeadView)
        {
            // world space offset for top-down
            desiredPosition = target.position + deadOffset;
        }
        else
        {
            // stable TPS shoulder offset
            desiredPosition = target.position + (transform.rotation * currentOffset);
        }

        // smooth movement
        transform.position = Vector3.SmoothDamp(
     transform.position,
     desiredPosition,
     ref currentVelocity,
     1f / followSpeed
 );

        Quaternion targetRot;

        if (isDeadView)
        {
            targetRot = Quaternion.Euler(deadRotation);
        }
        else
        {
            Vector3 flatForward = target.forward;
            flatForward.y = 0f;

            if (flatForward.sqrMagnitude < 0.001f)
            {
                flatForward = transform.forward;
            }

            targetRot = Quaternion.LookRotation(flatForward);
        }

        transform.rotation = Quaternion.Lerp(
    transform.rotation,
    targetRot,
    rotationSpeed * Time.deltaTime
);
    }

    public void SetShooting(bool value)
    {
        isShooting = value;

        // 🔥 remember latest combat/shoot state
        if (value)
        {
            lastShootTime = Time.time;
        }
    }
    public void SetDeadView(bool value)
    {
        isDeadView = value;
    }
}