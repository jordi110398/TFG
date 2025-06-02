using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartSystem : MonoBehaviour
{
    public GameObject heartPrefab;
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;
    public Transform heartsContainer;

    private List<Image> hearts = new List<Image>();
    public int maxHearts = 5;   // Pots modificar-ho
    private float currentHealth; // Vida actual del jugador
    public float maxHealth = 10f; // Vida màxima del jugador (cada cor = 2 punts de vida)

    // 🔸 Variables per l'animació
    private float pulseDuration = 0.2f; // Durada de la pulsació
    private Vector3 originalScale = Vector3.one;
    private Vector3 pulseScale = new Vector3(1.3f, 1.3f, 1.3f);

    void Start()
    {
        currentHealth = maxHealth;
        InitializeHearts();
        UpdateHearts();
    }

    void InitializeHearts()
    {
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            Image heartImage = heart.GetComponent<Image>();
            hearts.Add(heartImage);
        }
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            float heartValue = Mathf.Clamp(currentHealth - (i * 2), 0, 2);
            if (heartValue == 2)
            {
                hearts[i].sprite = fullHeart;
                StartCoroutine(PulseAnimation(hearts[i].transform));
            }
            else if (heartValue == 1)
            {
                hearts[i].sprite = halfHeart;
                StartCoroutine(PulseAnimation(hearts[i].transform));
            }
            else
            {
                hearts[i].sprite = emptyHeart;
                StartCoroutine(PulseAnimation(hearts[i].transform));
            }
        }
    }

    // 🔸 Coroutine per fer l'animació de polsació
    private IEnumerator PulseAnimation(Transform heartTransform)
    {
        // Ampliar
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            heartTransform.localScale = Vector3.Lerp(originalScale, pulseScale, elapsed / pulseDuration);
            yield return null;
        }

        // Reduir
        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            heartTransform.localScale = Vector3.Lerp(pulseScale, originalScale, elapsed / pulseDuration);
            yield return null;
        }

        heartTransform.localScale = originalScale;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Evitar valors negatius
        UpdateHearts();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Evitar superar el màxim
        UpdateHearts();
    }
}
