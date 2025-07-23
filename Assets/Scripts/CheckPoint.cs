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
                CheckpointManager.Instance.SetCheckpoint(transform.position);
            }

            var player2 = other.GetComponent<Player2Controller>();
            if (player2 != null)
            {
                Debug.Log("Player2 Checkpoint actualitzat");
                CheckpointManager.Instance.SetCheckpoint(transform.position);
            }

            // So de checkpoint
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.checkpointSound);
        }
    }
}
