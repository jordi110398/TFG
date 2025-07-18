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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
