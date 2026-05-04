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
    public float transitionSpeed = 5f;

    private Vector3 currentOffset;

    private bool isShooting = false;

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

        // 🎯 Smooth offset switch
        Vector3 targetOffset = isShooting ? shootingOffset : normalOffset;
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, transitionSpeed * Time.deltaTime);

        // 🎯 Desired position
        Vector3 desiredPosition = target.position + target.TransformDirection(currentOffset);

        // 🧈 Smooth follow position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 🔥 FIX: follow player rotation (no LookAt)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target.rotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // 🔥 Called from Player script
    public void SetShooting(bool value)
    {
        isShooting = value;
    }
}