using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameOverMenu : MonoBehaviour
{
    public static GameOverMenu Instance;
    public GameObject menuUI;
    public GameObject firstSelectedButton;

    private void Awake()
    {
        Instance = this;
        menuUI.SetActive(false);
    }

    public void Show()
    {
        menuUI.SetActive(true);
        Time.timeScale = 0f; // Pausa el joc
        // Selecciona el botó per defecte
        if (firstSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void Hide()
    {
        menuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRevive()
    {
        Hide();
        // Aquí crida la funció per reviure al checkpoint
        FindAnyObjectByType<HealthSystem>().RevivePlayersAtCheckpoint();
    }
}
