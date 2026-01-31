using UnityEngine;

public class DialogueZoneT : MonoBehaviour
{
    [Header("Links")]
    public DialogueManager dm;   // DialogueManager в сцене
    public Dialogue dialogue;    // КОНКРЕТНЫЙ диалог для этой зоны

    [Header("Input")]
    public KeyCode interactKey = KeyCode.T; // английская T

    [Header("Settings")]
    public string playerTag = "Player";
    public bool endDialogueOnExit = true;

    private bool playerInside = false;

    private void Update()
    {
        if (!playerInside) return;
        if (dm == null || dialogue == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            // если диалог закрыт — запускаем
            if (!dm.IsOpen)
            {
                dm.StartDialogue(dialogue);
            }
            // если открыт — листаем
            else
            {
                dm.Next();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;

        if (endDialogueOnExit && dm != null && dm.IsOpen)
            dm.EndDialogue();
    }
}