using UnityEngine;

public class DialogueAnimator : MonoBehaviour
{
    [Header("Animator кнопки StartDialogue")]
    public Animator startAnim;   // bool: startOpen

    [Header("DialogueManager (чтобы закрыть диалог при выходе)")]
    public DialogueManager dm;

    [Header("Тег игрока")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Показать кнопку “НАЧАТЬ ДИАЛОГ”
        if (startAnim != null)
            startAnim.SetBool("startOpen", true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Спрятать кнопку
        if (startAnim != null)
            startAnim.SetBool("startOpen", false);

        // Закрыть диалог
        if (dm != null)
            dm.EndDialogue();
    }
}