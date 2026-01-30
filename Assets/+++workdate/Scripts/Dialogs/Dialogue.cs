using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string name;

    // Чтобы удобно вставлять многострочный текст прямо в инспекторе
    [TextArea(3, 10)]
    public string[] sentences;
}