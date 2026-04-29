using UnityEngine;

public class SpriteHealthBar : MonoBehaviour
{
    public Transform fill;

    private Vector3 originalScale;

    void Awake()
    {
        if (fill != null)
        {
            // store original scale safely
            originalScale = fill.localScale;

            // force reset to full at start
            fill.localScale = originalScale;
        }
    }

    public void UpdateHealth(float percent)
    {
        percent = Mathf.Clamp01(percent);

        if (fill == null) return;

        fill.localScale = new Vector3(
            originalScale.x * percent,
            originalScale.y,
            originalScale.z
        );
    }
}