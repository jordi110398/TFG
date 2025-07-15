using UnityEngine;
using UnityEngine.UI;

public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)]
    public float minParallax = 0.1f;
    [Range(0f, 1f)]
    public float maxParallax = 0.6f;
    public bool parallaxEnabled = false;
    public Toggle parallaxToggle;

    private Transform cam;
    private Vector3 previousCamPosition;
    private Transform[] layers;

    void Start()
    {
        parallaxEnabled = true; // Activa el parallax per defecte
        cam = Camera.main.transform;
        previousCamPosition = cam.position;

        // Agafa tots els fills com a capes
        int count = transform.childCount;
        layers = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            layers[i] = transform.GetChild(i);
        }
    }

    void LateUpdate()
    {
        if (!parallaxEnabled) return;

        Vector3 delta = cam.position - previousCamPosition;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] != null)
            {
                // Calcula un parallaxFactor en funció de l'ordre (el més llunyà és el primer fill)
                float t = (float)i / (layers.Length - 1); // valor entre 0 i 1
                float parallaxFactor = Mathf.Lerp(minParallax, maxParallax, t);

                layers[i].position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0);
            }
        }

        previousCamPosition = cam.position;
    }

    public void SetParallaxActive(bool _)
{
    bool active = parallaxToggle != null ? parallaxToggle.isOn : false;
    Debug.Log("SetParallaxActive (lectura directa) cridat amb: " + active);
    parallaxEnabled = active;
}

    public void ForceParallaxOn()
    {
        parallaxEnabled = true;
        Debug.Log("Parallax forçat a ON");
        //optionsMenu?.SyncParallaxToggle();
    }

    public void ForceParallaxOff()
    {
        parallaxEnabled = false;
        Debug.Log("Parallax forçat a OFF");
        //optionsMenu?.SyncParallaxToggle();
    }
}

