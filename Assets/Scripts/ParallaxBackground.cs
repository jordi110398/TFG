using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)]
    public float minParallax = 0.1f;
    [Range(0f, 1f)]
    public float maxParallax = 0.6f;

    private Transform cam;
    private Vector3 previousCamPosition;
    private Transform[] layers;

    void Start()
    {
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
}

