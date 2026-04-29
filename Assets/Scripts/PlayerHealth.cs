using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("VFX")]
    public ParticleSystem hitEffect;
    public ParticleSystem deathEffect;

    private Image healthFill;
    private TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject fillObj = GameObject.FindGameObjectWithTag("Health_Fill");
        if (fillObj != null)
            healthFill = fillObj.GetComponent<Image>();

        GameObject textObj = GameObject.FindGameObjectWithTag("Health_Text");
        if (textObj != null)
            healthText = textObj.GetComponent<TextMeshProUGUI>();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, string type)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 💥 HIT EFFECT
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        float percent = (float)currentHealth / maxHealth;

        if (healthFill != null)
            healthFill.fillAmount = percent;

        if (healthText != null)
            healthText.text = currentHealth.ToString();
    }

    void Die()
    {
        Debug.Log("Player Died!");

        // 💀 DEATH EFFECT
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        PlayerAutoMove player = GetComponent<PlayerAutoMove>();
        if (player != null)
        {
            player.KillPlayer();
        }
    }
}