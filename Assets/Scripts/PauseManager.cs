using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseUI;
    public GameObject firstSelectedButton;
    private bool isPaused = false;

    private Player1Controller player1;
    private Player2Controller player2;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        TryFindPlayers();
        Debug.Log("Partida pausada");
        isPaused = true;
        Time.timeScale = 0f;
        pauseUI.SetActive(true);
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
        player1.playerInput.actions.FindActionMap("Player").Disable();
        player2.playerInput.actions.FindActionMap("Player").Disable();
    }

    public void Resume()
    {
        TryFindPlayers();
        isPaused = false;
        Time.timeScale = 1f;
        pauseUI.SetActive(false);
        player1.playerInput.actions.FindActionMap("Player").Enable();
        player2.playerInput.actions.FindActionMap("Player").Enable();


    }

    public void SaveGame()
    {
        Debug.Log("Partida desada");
        // Crida el teu sistema de desament aquí
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenOptions()
    {
        Debug.Log("Obrint menú d'opcions");
        // Pots activar un altre panell aquí
    }

    private void TryFindPlayers()
    {
        if (player1 == null)
            player1 = FindFirstObjectByType<Player1Controller>();
        if (player2 == null)
            player2 = FindFirstObjectByType<Player2Controller>();
    }
}
