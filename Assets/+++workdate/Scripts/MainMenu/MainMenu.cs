using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string levelSceneName = "level1";

    private void Start()
    {
        // на всякий случай: если до этого была смерть/пауза
        Time.timeScale = 1f;
    }

    public void PlayGame()
    {
        Debug.Log("PlayGame pressed -> loading: " + levelSceneName);
        SceneManager.LoadScene(levelSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame pressed");
        Application.Quit();
    }
}

