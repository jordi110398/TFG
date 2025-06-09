using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public HeartSystem player1Hearts; // Referència al HeartSystem del Player1
    public HeartSystem player2Hearts; // Referència al HeartSystem del Player2

    // Vida màxima (igual que al HeartSystem)
    public float maxHealth1;
    public float maxHealth2;

    // Vida actual dels jugadors
    private float player1Health;
    private float player2Health;

    private void Start()
    {
        // Inicialitzar la vida dels jugadors
        player1Health = maxHealth1 = player1Hearts.maxHealth;
        player2Health = maxHealth2 = player2Hearts.maxHealth;
    }

    // Funció per infligir mal a un jugador
    public void TakeDamage(string playerTag, float amount)
    {
        if (playerTag == "Player1")
        {
            // Comprovem si és invencible
            if (player1.TryGetComponent(out Player1Controller p1) && p1.IsInvincible)
            {
                Debug.Log("Player1 és invencible! No rep mal.");
                return;
            }
            player1Health -= amount;
            player1Health = Mathf.Max(player1Health, 0); // Evitar valors negatius
            player1Hearts.TakeDamage(amount); // Actualitzar la barra de vida
            Debug.Log($"Player 1 ha rebut {amount} de mal. Vida restant: {player1Health}");
        }
        else if (playerTag == "Player2")
        {
            player2Health -= amount;
            player2Health = Mathf.Max(player2Health, 0); // Evitar valors negatius
            player2Hearts.TakeDamage(amount); // Actualitzar la barra de vida
            Debug.Log($"Player 2 ha rebut {amount} de mal. Vida restant: {player2Health}");
        }
    }

    // Funció per curar un jugador
    public void Heal(string playerTag, float amount)
    {
        if (playerTag == "Player1")
        {
            player1Health += amount;
            player1Health = Mathf.Min(player1Health, maxHealth1); // Evitar passar-se de la vida màxima
            player1Hearts.Heal(amount); // Actualitzar la barra de vida
            Debug.Log($"Player 1 s'ha curat {amount} de vida. Vida actual: {player1Health}");
        }
        else if (playerTag == "Player2")
        {
            player2Health += amount;
            player2Health = Mathf.Min(player2Health, maxHealth2);
            player2Hearts.Heal(amount);
            Debug.Log($"Player 2 s'ha curat {amount} de vida. Vida actual: {player2Health}");
        }
    }
}
