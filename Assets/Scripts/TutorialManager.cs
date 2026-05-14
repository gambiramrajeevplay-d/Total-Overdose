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

        // 🔥 CHECK IF TUTORIAL EXISTS IN SCENE
        bool tutorialActive =
            (mobileUI != null && mobileUI.activeSelf) ||
            (tvUI != null && tvUI.activeSelf);

        if (tutorialActive)
        {
            Pauser.LockPause();
        }
        else
        {
            HideTutorial();
        }
    }

    public void ShowTutorial()
    {
        // 🔒 ALWAYS LOCK WHILE TUTORIAL IS OPEN
        Pauser.LockPause();

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

    // 🔥 OPTIONAL HELPER
    public bool IsTutorialOpen()
    {
        return mobileUI.activeSelf || tvUI.activeSelf;
    }
}