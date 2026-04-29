using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    private Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0, 3, -5);

    [Header("Smooth Settings")]
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;

    void Start()
    {
        // 🔍 Find player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player_Main");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player_Main' not found!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 🎯 Desired position
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // 🧈 Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 👀 Look at player
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }
}