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
        // Assegura que el joc no està pausat
        Time.timeScale = 1f; 
        // Destrueix la música de fons al canviar d'escena
        GameObject bgMusic = GameObject.Find("BackgroundMusicMenu");
        if (bgMusic != null)
            Destroy(bgMusic);
        SceneManager.LoadScene("SampleScene");
    }

    public void ShowControls()
    {
        if (controlsImage != null)
            controlsImage.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
