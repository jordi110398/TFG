using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseUI;
    public GameObject firstSelectedButton;
    public GameObject firstOptionsButton;
    public GameObject optionsPanel;
    private bool isPaused = false;

    private Player1Controller player1;
    private Player2Controller player2;

    //public OptionsMenu optionsMenu; // assigna-ho a l'Inspector

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TogglePause()
    {
        // Si el menú d'opcions està actiu, tanca'l i torna al menú de pausa
        if (optionsPanel.activeSelf)
        {
            CloseOptions();
            return;
        }

        if (isPaused)
            Resume();
        else
            Pause();
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
        optionsPanel.SetActive(true);
        pauseUI.SetActive(false);

        if (firstOptionsButton != null)
            EventSystem.current.SetSelectedGameObject(firstOptionsButton);
    }
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pauseUI.SetActive(true);

        // Desactiva el panel de controls si està actiu
        var optionsMenu = optionsPanel.GetComponent<OptionsMenu>();
        if (optionsMenu != null && optionsMenu.controlsPanel != null)
            optionsMenu.controlsPanel.SetActive(false);

        if (firstSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private void TryFindPlayers()
    {
        if (player1 == null)
            player1 = FindFirstObjectByType<Player1Controller>();
        if (player2 == null)
            player2 = FindFirstObjectByType<Player2Controller>();
    }
}
