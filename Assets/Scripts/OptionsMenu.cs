using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject buttonToSelect;
    public Toggle musicToggle;
    public Toggle parallaxToggle;
    public ParallaxBackground parallaxBackground;

    public void Start()
    {
    }
    public void OnClick()
    {

        if (buttonToSelect != null)
            EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }


}
