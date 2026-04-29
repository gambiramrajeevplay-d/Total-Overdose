using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public DamageFlashUI damageUI; // 🔥 IMPORTANT

    public void Hit(int damage)
    {
        Debug.Log("PLAYER GOT HIT"); // 🔥 DEBUG

        if (playerHealth != null)
            playerHealth.TakeDamage(damage, "Enemy");

        // 🔥 SHOW DAMAGE UI
        if (damageUI != null)
        {
            Debug.Log("SHOWING DAMAGE UI"); // 🔥 DEBUG
            damageUI.ShowDamage();
        }
        else
        {
            Debug.LogError("Damage UI NOT ASSIGNED!");
        }
    }
}