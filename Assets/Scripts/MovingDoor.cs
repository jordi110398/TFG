using UnityEngine;

public class MovingDoor : MonoBehaviour
{
    public float moveDistance = 2f;      // Distància que puja la porta
    public float moveSpeed = 2f;         // Velocitat de moviment
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpening = false;

    public bool startOpen = true; // Pots posar-ho a false per començar tancada

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * moveDistance;

        if (startOpen)
        {
            transform.position = openPosition;
            isOpening = true;
        }
        else
        {
            transform.position = closedPosition;
            isOpening = false;
        }
    }

    void Update()
    {
        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, closedPosition, moveSpeed * Time.deltaTime);
        }
    }

    // Crida això des de la PressurePlate (amb UnityEvent)
    public void OpenDoor()
    {
        isOpening = true;
    }

    public void CloseDoor()
    {
        Debug.Log("Tancant porta");
        isOpening = false;
    }
}
