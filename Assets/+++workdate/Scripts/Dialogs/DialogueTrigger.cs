using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;           // сюда вставляешь имя и фразы
    public DialogueManager manager;     // сюда перетащи объект DialogueManager из сцены

    // Эту функцию вызываем из кнопки StartDialogue -> OnClick()
    public void TriggerDialogue()
    {
        if (manager == null)
        {
            Debug.LogError("DialogueTrigger: manager не назначен в инспекторе!");
            return;
        }

        manager.StartDialogue(dialogue);
    }
}