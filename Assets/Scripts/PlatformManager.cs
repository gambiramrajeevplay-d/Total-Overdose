using UnityEngine;
using Script;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance;

    public enum PlatformType
    {
        Mobile,
        TV
    }

    [Header("Editor Testing")]
    [SerializeField] private bool testTVInEditor = false;

    [Header("Current Platform")]
    public PlatformType currentPlatform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            DetectPlatform();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void DetectPlatform()
    {
#if UNITY_EDITOR

        currentPlatform =
            testTVInEditor
            ? PlatformType.TV
            : PlatformType.Mobile;

#elif UNITY_ANDROID

        if (AndroidTV.IsAndroidOrFireTv())
        {
            currentPlatform = PlatformType.TV;
        }
        else
        {
            currentPlatform = PlatformType.Mobile;
        }

#else

        currentPlatform = PlatformType.Mobile;

#endif

        Debug.Log("Detected Platform : " + currentPlatform);
    }

    public bool IsMobile()
    {
        return currentPlatform == PlatformType.Mobile;
    }

    public bool IsTV()
    {
        return currentPlatform == PlatformType.TV;
    }
}