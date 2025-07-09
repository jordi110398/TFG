using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            var player = other.GetComponent<Player1Controller>();
            if (player != null)
            {
                Debug.Log("Player1 Checkpoint actualitzat");
                player.lastCheckpointPosition = transform.position;
            }

            var player2 = other.GetComponent<Player2Controller>();
            if (player2 != null)
            {
                Debug.Log("Player2 Checkpoint actualitzat");
                player2.lastCheckpointPosition = transform.position;
            }
        }
    }
}
