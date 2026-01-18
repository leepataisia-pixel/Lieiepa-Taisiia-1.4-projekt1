using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject deathMenuPanel;

    private playerHealth _player;
    private bool _isOpen;

    private void Start()
    {
        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(false);

        _isOpen = false;
    }

    // playerHealth передаёт сам себя, чтобы Resume точно знал кого оживлять
    public void ShowDeathMenu(playerHealth player)
    {
        _player = player;

        if (deathMenuPanel == null)
        {
            Debug.LogError("DeathMenuUI: deathMenuPanel не назначен!");
            return;
        }

        deathMenuPanel.SetActive(true);
        _isOpen = true;

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!_isOpen) return;

        // снимаем паузу
        Time.timeScale = 1f;

        // оживляем игрока
        if (_player != null)
            _player.Revive();
        else
            Debug.LogError("DeathMenuUI: playerHealth не найден (player == null).");

        // закрываем меню
        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(false);

        _isOpen = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        _isOpen = false;

        SceneManager.LoadScene("MainMenu");
    }
}