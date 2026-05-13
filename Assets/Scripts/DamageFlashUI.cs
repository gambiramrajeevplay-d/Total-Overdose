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
        Color c = damageImage.color;

        // start invisible
        c.a = 0f;
        damageImage.color = c;

        // =========================
        // FADE IN
        // =========================
        float fadeInTime = 0.15f;
        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(0f, 1f, t / fadeInTime);
            damageImage.color = c;

            yield return null;
        }

        // small hold
        yield return new WaitForSeconds(0.05f);

        // =========================
        // FADE OUT
        // =========================
        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            damageImage.color = c;

            yield return null;
        }

        c.a = 0f;
        damageImage.color = c;

        damageImage.gameObject.SetActive(false);
    }
}