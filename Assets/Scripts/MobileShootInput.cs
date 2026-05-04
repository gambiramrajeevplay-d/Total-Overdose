using UnityEngine;
using UnityEngine.UI;

public class MobileShootInput : MonoBehaviour
{
    private PlayerAutoMove player;

    void Start()
    {
        player = FindObjectOfType<PlayerAutoMove>();

        GameObject btnObj = GameObject.FindGameObjectWithTag("ShootButton");

        if (btnObj != null)
        {
            Button shootBtn = btnObj.GetComponent<Button>();

            if (shootBtn != null)
            {
                shootBtn.onClick.AddListener(OnShootPressed);
            }
            else
            {
                Debug.LogError("ShootButton has no Button component!");
            }
        }
        else
        {
            Debug.LogError("No GameObject with tag 'ShootButton' found!");
        }
    }

    void OnShootPressed()
    {
        if (player != null)
        {
            player.MobileShoot(); // 👈 call your custom shoot function
        }
    }
}