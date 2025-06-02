using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Vector3 originalPos;
    bool moveBack = false;
    SpriteRenderer spriteRenderer;
    private float maxDownDistance = 0.09f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player2") || collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Pushable"))
        {
            collision.transform.parent = transform;
            moveBack = false;
            GetComponent<SpriteRenderer>().color = Color.red;
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player2") || collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Pushable"))
        {
            // Només baixar si no ha superat el límit
            if (transform.position.y > originalPos.y - maxDownDistance)
            {
                transform.Translate(0, -0.01f, 0);
            }
            moveBack = false;
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player2") || collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Pushable"))
        {
            collision.transform.parent = null;
            moveBack = true;
            GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    void Update()
    {
        if (moveBack)
        {
            if (transform.position.y < originalPos.y)
            {
                transform.Translate(0f, 0.01f, 0f);
            }
            else
            {
                // Ens assegurem que es queda exactament a originalPos
                transform.position = new Vector3(transform.position.x, originalPos.y, transform.position.z);
                moveBack = false;
                moveBack = false;
            }

        }
    }
}
