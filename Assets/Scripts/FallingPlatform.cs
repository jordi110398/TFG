using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public Vector3 originalPos;
    public float maxDownDistance = 1.5f;
    public float speed = 0.01f;
    public float waitTime = 1.5f; // temps d'espera abans de pujar

    private bool playerOnPlatform = false;
    private float waitTimer = 0f;

    private void Start()
    {
        originalPos = transform.position;
    }

    private void Update()
    {
        if (playerOnPlatform)
        {
            // Mentre el jugador és a sobre, baixa fins a la distància màxima
            if (transform.position.y > originalPos.y - maxDownDistance)
            {
                transform.Translate(0f, -speed, 0f);
            }

            waitTimer = 0f; // reset de l’espera si el jugador hi és
        }
        else
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
            }
            else
            {
                // Torna a pujar si ha acabat el temps d’espera
                if (transform.position.y < originalPos.y)
                {
                    float newY = Mathf.Min(transform.position.y + speed, originalPos.y);
                    transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player1") || collision.collider.CompareTag("Player2"))
        {
            playerOnPlatform = true;
            collision.transform.parent = transform;
            var player = collision.gameObject.GetComponent<MonoBehaviour>();
            player?.Invoke("ResetScale", 0f); // Crida ResetScale si existeix
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player1") || collision.collider.CompareTag("Player2"))
        {
            playerOnPlatform = false;
            waitTimer = waitTime;
            collision.transform.parent = null;
            var player = collision.gameObject.GetComponent<MonoBehaviour>();
            player?.Invoke("ResetScale", 0f); // Crida ResetScale si existeix
        }
    }
}
