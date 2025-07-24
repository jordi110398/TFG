using UnityEngine;

public class EnemyZoneTrigger : MonoBehaviour
{
    public MovingDoor porta; // Assigna la porta a l'Inspector
    private int playersInside = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playersInside++;
            Debug.Log("Jugador ha entrat a la zona d'enemic: " + other.gameObject.name + " | Jugadors dins: " + playersInside);

            if (playersInside == 2)
            {
                Debug.Log("Tots dos jugadors dins la zona! Tancant porta.");
                porta.CloseDoor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playersInside = Mathf.Max(0, playersInside - 1);
            Debug.Log("Jugador ha sortit de la zona d'enemic: " + other.gameObject.name + " | Jugadors dins: " + playersInside);
        }
    }
}
