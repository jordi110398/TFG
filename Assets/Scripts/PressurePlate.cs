using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Vector3 originalPos;
    private bool moveBack = false;
    private SpriteRenderer spriteRenderer;
    private float maxDownDistance = 0.09f;
    private bool isPressed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player2") || collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Pushable"))
        {
            isPressed = true;
            moveBack = false;
            spriteRenderer.color = Color.red;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player2") || collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Pushable"))
        {
            isPressed = false;
            moveBack = true;
            spriteRenderer.color = Color.green;
        }
    }

    void Update()
    {
        if (isPressed)
        {
            if (transform.position.y > originalPos.y - maxDownDistance)
            {
                transform.Translate(0, -0.01f, 0);
                spriteRenderer.color = Color.blue;
            }
        }
        else if (moveBack)
        {
            if (transform.position.y < originalPos.y)
            {
                transform.Translate(0f, 0.01f, 0f);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, originalPos.y, transform.position.z);
                moveBack = false;
            }
        }
    }
}
