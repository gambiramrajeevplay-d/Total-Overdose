using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageFlashUI : MonoBehaviour
{
    public Image damageImage;
    public float fadeDuration = 2f;

    private Coroutine currentRoutine;

    void Start()
    {
        if (damageImage != null)
        {
            damageImage.gameObject.SetActive(false); // 🔥 start hidden
        }
    }

    public void ShowDamage()
    {
        if (damageImage == null) return;

        // 🔥 ENABLE IMAGE
        damageImage.gameObject.SetActive(true);

        // stop previous fade if running
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        float t = 0f;

        Color c = damageImage.color;
        c.a = 1f;
        damageImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            c.a = alpha;
            damageImage.color = c;

            yield return null;
        }

        // 🔥 hide again after fade
        damageImage.gameObject.SetActive(false);
    }
}