using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI (TextMeshPro)")]
    public TMP_Text dialogueText;
    public TMP_Text nameText;

    [Header("Animators")]
    public Animator boxAnim;    // bool: boxOpen
    public Animator startAnim;  // bool: startOpen

    private Queue<string> sentences = new Queue<string>();
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentSentence = "";

    private Dialogue currentDialogue;   // текущий диалог
    public bool IsOpen { get; private set; } = false;

    // === СТАРТ ДИАЛОГА ===
    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null) return;

        currentDialogue = dialogue;
        IsOpen = true;

        // Открываем окно
        if (boxAnim != null) boxAnim.SetBool("boxOpen", true);

        // Прячем кнопку старта
        if (startAnim != null) startAnim.SetBool("startOpen", false);

        // Имя персонажа
        if (nameText != null) nameText.text = dialogue.name;

        // Загружаем реплики
        sentences.Clear();
        foreach (string s in dialogue.sentences)
            sentences.Enqueue(s);

        DisplayNextSentence();
    }

    // === ВНЕШНИЙ ВЫЗОВ (КНОПКА T) ===
    public void Next()
    {
        if (!IsOpen) return;
        DisplayNextSentence();
    }

    // === СЛЕДУЮЩАЯ РЕПЛИКА ===
    public void DisplayNextSentence()
    {
        // Если печатается — допечатать сразу
        if (isTyping)
        {
            FinishTypingInstant();
            return;
        }

        // Если реплик нет — конец диалога
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    // === ПЕЧАТАНИЕ ===
    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char c in sentence)
        {
            if (dialogueText != null)
                dialogueText.text += c;

            yield return null;
        }

        isTyping = false;
        typingCoroutine = null;
    }

    // === МГНОВЕННО ПОКАЗАТЬ ТЕКСТ ===
    private void FinishTypingInstant()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueText != null)
            dialogueText.text = currentSentence;

        isTyping = false;
        typingCoroutine = null;
    }

    // === КОНЕЦ ДИАЛОГА ===
    public void EndDialogue()
    {
        if (!IsOpen) return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        typingCoroutine = null;
        IsOpen = false;

        if (boxAnim != null)
            boxAnim.SetBool("boxOpen", false);
    }
}