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

    // Jugadors
    public GameObject player1;
    public GameObject player2;

    // Camera
    private AdaptiveCamera adaptiveCamera;

    // Menu Game Over
    public GameObject gameOverMenu;

    private void Update()
    {

    }
    private void Start()
    {
        // Inicialitzar la vida dels jugadors
        player1Health = maxHealth1 = player1Hearts.maxHealth;
        player2Health = maxHealth2 = player2Hearts.maxHealth;
        adaptiveCamera = FindAnyObjectByType<AdaptiveCamera>();
    }

    // Funció per infligir mal a un jugador
    public void TakeDamage(string playerTag, float amount)
    {
        // Obtenir els jugadors
        player1 = GameObject.FindGameObjectWithTag("Player1");
        player2 = GameObject.FindGameObjectWithTag("Player2");

        if (playerTag == "Player1")
        {
            var playerController = player1.GetComponent<Player1Controller>();

            // Comprova invencibilitat
            if (playerController.IsInvincible())
            {
                Debug.Log($"{player1.name} està invencible, no rep mal.");
                return;
            }
            player1Health -= amount;
            player1Health = Mathf.Max(player1Health, 0); // Evitar valors negatius
            player1Hearts.TakeDamage(amount); // Actualitzar la barra de vida
                                              // Camera shake
            if (adaptiveCamera != null)
            {
                adaptiveCamera.ShakeCamera(0.15f, 0.3f); // Iniciar el shake de la càmera
            }
            // --- Flash de dany ---
            playerController.StartCoroutine(playerController.PlayDamageFlash());
            playerController.StartCoroutine(playerController.PlayDamagePulse());

            Debug.Log($"Player 1 ha rebut {amount} de mal. Vida restant: {player1Health}");
        }
        else if (playerTag == "Player2")
        {
            if (player2 != null && player2.TryGetComponent(out Player2Controller p2) && p2.IsInvincible())
            {
                Debug.Log("Player2 és invencible! No rep mal.");
                return;
            }
            player2Health -= amount;
            player2Health = Mathf.Max(player2Health, 0); // Evitar valors negatius
            player2Hearts.TakeDamage(amount); // Actualitzar la barra de vida
            // Camera shake P2
            if (adaptiveCamera != null)
            {
                adaptiveCamera.ShakeCamera(0.15f, 0.3f); // Iniciar el shake de la càmera
            }

            // --- Flash de dany i pulsació ---
            if (player2 != null && player2.TryGetComponent(out Player2Controller p2Controller))
            {
                p2Controller.StartCoroutine(p2Controller.PlayDamageFlash());
                p2Controller.StartCoroutine(p2Controller.PlayDamagePulse());
            }

            Debug.Log($"Player 2 ha rebut {amount} de mal. Vida restant: {player2Health}");
        }

        if (player1Health <= 0)
        {
            KillPlayer("Player1");
            GameOverMenu.Instance.Show();
            return;
        }
        if (player2Health <= 0)
        {
            KillPlayer("Player2");
            GameOverMenu.Instance.Show();
            return;
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

    public void KillPlayer(string playerTag)
    {
        if (playerTag == "Player1" && player1 != null)
        {
            player1.GetComponent<Player1Controller>().Die();
        }
        else if (playerTag == "Player2" && player2 != null)
        {
            player2.GetComponent<Player2Controller>().Die();
        }
    }

    public void RevivePlayersAtCheckpoint()
    {
        // Torna la vida als jugadors
        player1Health = maxHealth1;
        player2Health = maxHealth2;
        player1Hearts.Heal(maxHealth1);
        player2Hearts.Heal(maxHealth2);

        // Reactiva els jugadors i els porta al checkpoint
        if (player1 != null)
        {
            player1.GetComponent<Player1Controller>().ReviveAtCheckpoint();
        }
        if (player2 != null)
        {
            player2.GetComponent<Player2Controller>().ReviveAtCheckpoint();
        }
    }
}
