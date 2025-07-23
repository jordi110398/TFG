using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject controlsImage;

    void Update()
    {
        if (controlsImage != null && controlsImage.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            controlsImage.SetActive(false);
        }
    }

    public void Play()
    {
        Time.timeScale = 1f; // Assegura que el joc no està pausat
        // Destrueix la música de fons si existeix
        GameObject bgMusic = GameObject.Find("BackgroundMusicMenu");
        if (bgMusic != null)
            Destroy(bgMusic);
        //Destroy(gameObject); // Destrueix el MainMenu abans de carregar la nova escena
        SceneManager.LoadScene("SampleScene");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ShowControls()
    {
        if (controlsImage != null)
            controlsImage.SetActive(true);
    }
}
