using UnityEngine;

public class Chest : MonoBehaviour
{
    public bool isOpened = false; // Indica si el cofre està obert
[Header("Contingut")]
    public GameObject potionPrefab;
    public Transform dropPoint;

    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openedSprite;

    [Header("Efectes visuals")]
    public GameObject sparkleVFXPrefab; // Prefab d’una lluentor

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (closedSprite != null) sr.sprite = closedSprite;
    }

    public void Open()
    {
        if (isOpened) return;
        isOpened = true;

        Debug.Log("Cofre obert!");

        // Canvi de sprite
        if (openedSprite != null && sr != null)
            sr.sprite = openedSprite;

        // Drop de la poció
        if (potionPrefab != null && dropPoint != null)
            Instantiate(potionPrefab, dropPoint.position, Quaternion.identity);

        // Lluentor visual
        if (sparkleVFXPrefab != null && dropPoint != null)
            Instantiate(sparkleVFXPrefab, dropPoint.position, Quaternion.identity);
    }
}
