using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private List<GameObject> players = new List<GameObject>(); // Llista de prefabs per a cada jugador
    [SerializeField] private Transform[] spawnPoints; // Llocs de spawn per als jugadors
    private PlayerInputManager manager;
    private int nextSpawnIndex = 0; // �ndex del següent punt de spawn
    private int nextPlayerIndex = 0; // Index del següent prefab a assignar

    private void Start()
    {
        manager = GetComponent<PlayerInputManager>();
        SetNextCharacter();
    }

    // Assigna el següent prefab en ordre a ser spawnejat
    private void SetNextCharacter()
    {
        if (nextPlayerIndex < players.Count)
        {
            manager.playerPrefab = players[nextPlayerIndex];
        }
        else
        {
            Debug.LogWarning("No hi ha més prefabs disponibles per assignar als jugadors.");
        }
    }

    // Cridat autom�ticament quan un jugador es connecta
    public void OnPlayerJoined(PlayerInput input)
    {
        if (nextSpawnIndex < spawnPoints.Length)
        {
            // Posicionar el jugador al punt de spawn corresponent
            input.transform.position = spawnPoints[nextSpawnIndex].position;
            nextSpawnIndex++;
            Debug.Log($"Jugador {nextSpawnIndex} connectat i posicionat al punt de spawn {nextSpawnIndex - 1}");
        }
        else
        {
            Debug.LogWarning("No hi ha més punts de spawn disponibles.");
        }

        // Incrementar l'�ndex del prefab
        nextPlayerIndex++;
        SetNextCharacter(); // Assignar el seg�ent prefab
    }
}
