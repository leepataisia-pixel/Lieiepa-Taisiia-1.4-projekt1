using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;

    private InputSystem_Actions input;
    private InputAction cancelAction;

    private bool isPaused;

    private void Awake()
    {
        input = new InputSystem_Actions();
        cancelAction = input.UI.Cancel;
    }

    private void OnEnable()
    {
        input.Enable();
        cancelAction.performed += OnCancel;
    }

    private void OnDisable()
    {
        cancelAction.performed -= OnCancel;
        input.Disable();
    }

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}