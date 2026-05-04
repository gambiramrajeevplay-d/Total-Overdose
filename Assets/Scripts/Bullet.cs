using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public float speed = 80f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    Transform target;
    Vector3 shootDirection;

    [Header("Damage")]
    public int damage = 20;

    [Header("VFX")]
    public ParticleSystem hitEffect;

    [Header("SFX")]
    public AudioClip hitSound;

    public enum BulletOwner { Player, Enemy }
    public BulletOwner owner;

    // 🔥 prevent instant hit
    private float spawnTime;
    public float minHitDelay = 0.1f;

    private HashSet<Collider> hitColliders = new HashSet<Collider>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        spawnTime = Time.time;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (target != null)
                shootDirection = (target.position - transform.position).normalized;
            else
                shootDirection = transform.forward;

            // 🔥 avoid spawn overlap
            transform.position += shootDirection * 0.5f;

            rb.velocity = shootDirection * speed;
        }

        IgnoreOwnerCollision();

        Destroy(gameObject, lifeTime);
    }

    void IgnoreOwnerCollision()
    {
        Collider myCol = GetComponent<Collider>();
        Collider[] parentCols = GetComponentsInParent<Collider>();

        foreach (var col in parentCols)
        {
            Physics.IgnoreCollision(myCol, col);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            shootDirection = Vector3.Lerp(shootDirection, dir, 0.05f);
            rb.velocity = shootDirection * speed;
        }

        Vector3 rot = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(90f, rot.y, rot.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < minHitDelay) return;

        if (hitColliders.Contains(other)) return;
        hitColliders.Add(other);

        Vector3 hitPoint = other.transform.position + Vector3.up * 0.5f;

        // =========================
        // PLAYER → ENEMY
        // =========================
        if (owner == BulletOwner.Player && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            HitBox enemyHB = other.GetComponentInParent<HitBox>();

            if (enemyHB != null)
            {
                enemyHB.Hit(damage);

                PlayHitEffect(hitPoint, enemyHB.transform);
                PlayHitSound(hitPoint);

                DisableBullet();
                return;
            }
        }

        // =========================
        // ENEMY → PLAYER
        // =========================
        if (owner == BulletOwner.Enemy && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerHitBox playerHB = other.GetComponentInParent<PlayerHitBox>();

            if (playerHB != null)
            {
                playerHB.Hit(damage);

                PlayHitSound(hitPoint);

                DisableBullet();
                return;
            }
        }
    }

    void PlayHitEffect(Vector3 position, Transform parent)
    {
        if (hitEffect == null) return;

        ParticleSystem fx = Instantiate(hitEffect, position, Quaternion.identity);
        fx.transform.SetParent(parent);

        Destroy(fx.gameObject, 2f);
    }

    void PlayHitSound(Vector3 position)
    {
        if (hitSound == null) return;

        GameObject audioObj = new GameObject("HitSound");
        audioObj.transform.position = position;

        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = hitSound;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.playOnAwake = false;

        audioSource.Play();

        Destroy(audioObj, hitSound.length);
    }

    void DisableBullet()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer rend = GetComponent<MeshRenderer>();
        if (rend != null) rend.enabled = false;

        Destroy(gameObject);
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}