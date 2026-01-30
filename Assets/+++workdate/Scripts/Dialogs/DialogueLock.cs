using UnityEngine;
using UnityEngine.EventSystems;
using ___WorkData.Scripts.Player;

public class DialogueLock : MonoBehaviour
{
    // ✅ Глобальный флаг: если true — атака запрещена
    public static bool DialogueOpen { get; private set; }

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueBox;          // твой DialogueBox / Panel
    [SerializeField] private GameObject firstSelectedButton;  // Start/Next кнопка (любая)

    [Header("Player")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        // авто-поиск если не проставлено
        if (playerAttack == null) playerAttack = FindObjectOfType<PlayerAttack>();
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OpenDialogue();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CloseDialogue();
    }

    public void OpenDialogue()
    {
        DialogueOpen = true;

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        //  Важно: запрещаем атаку (и по желанию движение)
        if (playerAttack != null) playerAttack.enabled = false;

        // если хочешь только “не бить”, но двигаться — закомментируй строку ниже
        //if (playerController != null) playerController.enabled = false;

        // ✅ чтобы кнопка реально получала ввод
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void CloseDialogue()
    {
        DialogueOpen = false;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (playerAttack != null) playerAttack.enabled = true;
        if (playerController != null) playerController.enabled = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}