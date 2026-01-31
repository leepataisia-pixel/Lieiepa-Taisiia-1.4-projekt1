using UnityEngine;

public class HideUIInZone : MonoBehaviour
{
    public string playerTag = "Player";
    public GameObject uiToHide;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (uiToHide != null) uiToHide.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (uiToHide != null) uiToHide.SetActive(true);
    }
}