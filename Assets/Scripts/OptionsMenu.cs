using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject buttonToSelect;
    public GameObject controlsPanel;
    public Toggle musicToggle;
    public Toggle parallaxToggle;
    public ParallaxBackground parallaxBackground;

    void Update()
    {
        if (controlsPanel != null && controlsPanel.activeSelf &&
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)))
        {
            controlsPanel.SetActive(false);
        }
    }

    public void Start()
    {
    }
    public void OnClick()
    {

        if (buttonToSelect != null)
            EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }

    public void ShowControlsPanel()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }
}
