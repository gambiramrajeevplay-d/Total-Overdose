using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial UI")]
    public GameObject mobileUI;
    public GameObject tvUI;

    private void Awake()
    {
        Instance = this;

        HideTutorial();
    }

    public void ShowTutorial()
    {
        if (PlatformManager.Instance.IsTV())
        {
            tvUI.SetActive(true);
            mobileUI.SetActive(false);
        }
        else
        {
            mobileUI.SetActive(true);
            tvUI.SetActive(false);
        }
    }

    public void HideTutorial()
    {
        mobileUI.SetActive(false);
        tvUI.SetActive(false);
    }
}