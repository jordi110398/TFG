using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public bool isActive = false;
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    private Vector3 nextPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Comença anant cap al punt més llunyà
        float distA = Vector3.Distance(transform.position, pointA.position);
        float distB = Vector3.Distance(transform.position, pointB.position);
        nextPosition = (distA > distB) ? pointA.position : pointB.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextPosition) < 0.01f)
        {
            nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            collision.gameObject.transform.parent = transform;
            var player = collision.gameObject.GetComponent<MonoBehaviour>();
            player?.Invoke("ResetScale", 0f); // Crida ResetScale si existeix
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            collision.gameObject.transform.parent = null;
            var player = collision.gameObject.GetComponent<MonoBehaviour>();
            player?.Invoke("ResetScale", 0f); // Crida ResetScale si existeix
        }
    }

    public void Activate()
    {
        Debug.Log("Plataforma activada");
        isActive = true;
        //nextPosition = pointB.position; // Comença movent-se cap al punt B
    }

    public void Deactivate()
    {
        isActive = false;
        //nextPosition = pointA.position; // Comença movent-se cap al punt A
    }
}
