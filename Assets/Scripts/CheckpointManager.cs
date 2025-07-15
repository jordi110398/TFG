using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;
    public Vector3 lastCheckpointPosition;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpointPosition = pos;
    }

    public void MovePlayersToCheckpoint()
    {
        var p1 = FindFirstObjectByType<Player1Controller>();
        var p2 = FindFirstObjectByType<Player2Controller>();
        if (p1 != null) p1.transform.position = lastCheckpointPosition;
        if (p2 != null) p2.transform.position = lastCheckpointPosition;
    }
}
