using UnityEngine;

public class EnemyZoneTrigger : MonoBehaviour
{
    public MovingDoor porta; // Assigna la porta a l'Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") && other.CompareTag("Player2"))
        {
            Debug.Log("Jugadors han entrat a la zona d'enemic!");
            porta.CloseDoor(); // Tanca la porta quan entra el jugador
        }
    }
}
