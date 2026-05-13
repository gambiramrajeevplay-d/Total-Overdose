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

   
    public enum BulletOwner { Player, Enemy }
    public BulletOwner owner;
    public static List<Bullet> activeBullets = new List<Bullet>();

    // 🔥 prevent instant hit
    private float spawnTime;
    public float minHitDelay = 0.1f;

    private HashSet<Collider> hitColliders = new HashSet<Collider>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        spawnTime = Time.time;
        activeBullets.Add(this);

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

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < minHitDelay)
            return;

        if (hitColliders.Contains(other))
            return;

        hitColliders.Add(other);

        // =========================
        // PLAYER BULLET → ENEMY
        // =========================
        if (owner == BulletOwner.Player)
        {
            HitBox enemyHB = other.GetComponent<HitBox>();

            if (enemyHB == null)
                enemyHB = other.GetComponentInParent<HitBox>();

            if (enemyHB != null)
            {
                // damage enemy
                enemyHB.Hit(damage);

                // play hit effect
                PlayHitEffect(
                    other.ClosestPoint(transform.position),
                    enemyHB.transform
                );

                // destroy bullet immediately
                DisableBullet();
                return;
            }
        }

        // =========================
        // ENEMY BULLET → PLAYER
        // =========================
        if (owner == BulletOwner.Enemy)
        {
            PlayerHitBox playerHB =
                other.GetComponent<PlayerHitBox>();

            if (playerHB == null)
                playerHB = other.GetComponentInParent<PlayerHitBox>();

            if (playerHB != null)
            {
                // damage player
                playerHB.Hit(damage);

                // NO particle for player hit
                gameObject.SetActive(false);
                DisableBullet();
                return;
            }
        }
        gameObject.SetActive(false);
    }
    private void OnCollisionEnter(Collision collision)
    {
        // only for enemy bullets
        if (owner != BulletOwner.Enemy)
            return;

        // hit player body
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHitBox hb =
                collision.gameObject.GetComponentInChildren<PlayerHitBox>();

            if (hb != null)
            {
                hb.Hit(damage);
               
            }

            DisableBullet();
        }
        if (owner == BulletOwner.Player)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // trigger already handles damage
                // collision only destroys bullet
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



    void DisableBullet()
    {
        // stop movement immediately
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // disable collider instantly
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // stop homing/tracking
        enabled = false;

        // destroy instantly
        Destroy(gameObject);
    }
    public static void DestroyAllEnemyBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            if (activeBullets[i] == null) continue;

            if (activeBullets[i].owner == BulletOwner.Enemy)
            {
                Destroy(activeBullets[i].gameObject);
            }
        }
    }
    public void SetTarget(Transform t)
    {
        target = t;
    }

    void OnDestroy()
    {
        activeBullets.Remove(this);
    }
    public void ForceDestroy()
    {
        DisableBullet();
    }
}