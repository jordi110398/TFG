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
        if (musicToggle != null)
        {
            // Sincronitza el toggle amb l'estat actual
            musicToggle.isOn = AudioManager.Instance.IsMusicMuted();
            musicToggle.onValueChanged.AddListener(OnAudioToggleChanged);
        }
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
    public void OnAudioToggleChanged(bool mute)
    {
        AudioManager.Instance.MuteMusic(mute);
        AudioManager.Instance.MuteSFX(mute);
    }
}
