using UnityEngine;

public class Chest : MonoBehaviour
{
    public bool isOpened = false; // Indica si el cofre està obert
    public bool needsKey = false; // Indica si es necessita una clau per obrir el cofre

    [Header("Contingut")]
    public GameObject potionPrefab;
    public Transform dropPoint;
    public GameObject[] dropPrefabs; // Array d'objectes a deixar caure

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

        // Drop d’objectes
        if (dropPrefabs != null && dropPoint != null)
        {
            foreach (var prefab in dropPrefabs)
            {
                if (prefab != null)
                    Instantiate(prefab, dropPoint.position, Quaternion.identity);
            }
        }

        // Lluentor visual
        if (sparkleVFXPrefab != null && dropPoint != null)
            Instantiate(sparkleVFXPrefab, dropPoint.position, Quaternion.identity);
    }

    public void TryOpen(bool hasKey)
    {
        if (isOpened) return;
        if (needsKey && !hasKey)
        {
            Debug.Log("Necessites una clau!");
            return;
        }
        Open();
    }
}
