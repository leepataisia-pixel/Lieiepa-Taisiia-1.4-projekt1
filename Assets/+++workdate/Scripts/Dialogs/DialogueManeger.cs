using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI (TextMeshPro)")]
    public TMP_Text dialogueText;   // сюда перетащи DialogueText (TMP)
    public TMP_Text nameText;       // сюда перетащи NameText (TMP)

    [Header("Animators")]
    public Animator boxAnim;        // Animator окна диалога (bool: boxOpen)
    public Animator startAnim;      // Animator кнопки StartDialogue (bool: startOpen)

    private Queue<string> sentences = new Queue<string>(); // очередь реплик
    private Coroutine typingCoroutine; // корутина печати
    private bool isTyping = false;     // печатаем ли сейчас
    private string currentSentence = ""; // текущая реплика (для “допечатать”)

    // СТАРТ ДИАЛОГА
    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null) return;

        // Открываем окно диалога
        if (boxAnim != null) boxAnim.SetBool("boxOpen", true);

        // Прячем кнопку старта (как в видео)
        if (startAnim != null) startAnim.SetBool("startOpen", false);

        // Имя персонажа
        if (nameText != null) nameText.text = dialogue.name;

        // Загружаем реплики
        sentences.Clear();
        if (dialogue.sentences != null)
        {
            foreach (string s in dialogue.sentences)
                sentences.Enqueue(s);
        }

        DisplayNextSentence();
    }

    // СЛЕДУЮЩАЯ РЕПЛИКА (кнопка NextButton вызывает это)
    public void DisplayNextSentence()
    {
        // Если текст печатается — по нажатию сразу показываем всю строку
        if (isTyping)
        {
            FinishTypingInstant();
            return;
        }

        // Если реплик больше нет — закрываем диалог
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    // ПЕЧАТАНИЕ ТЕКСТА
    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char c in sentence)
        {
            if (dialogueText != null)
                dialogueText.text += c;

            yield return null; // 1 символ за кадр
        }

        isTyping = false;
        typingCoroutine = null;
    }

    // МГНОВЕННО ДОПЕЧАТАТЬ ТЕКУЩУЮ СТРОКУ
    private void FinishTypingInstant()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (dialogueText != null)
            dialogueText.text = currentSentence;

        isTyping = false;
        typingCoroutine = null;
    }

    // КОНЕЦ ДИАЛОГА (ВАЖНО: public, чтобы другие скрипты могли закрывать)
    public void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
        typingCoroutine = null;

        if (boxAnim != null) boxAnim.SetBool("boxOpen", false);
    }
}