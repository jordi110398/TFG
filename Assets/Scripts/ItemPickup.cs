using Unity.VisualScripting;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public bool swordHandEquippable;
    public bool bowHandEquippable;
    public bool itemConsumable;
    public bool shieldHandEquippable;

    private Transform weaponHand;
    private Transform swordHand;
    private Transform shieldHand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponHand = GameObject.Find("WeaponHand").GetComponent<Transform>();
        shieldHand = GameObject.Find("ShieldHand").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        weaponHand = GameObject.Find("WeaponHand").GetComponent<Transform>();
        shieldHand = GameObject.Find("ShieldHand").GetComponent<Transform>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ARC
        if (collision.gameObject.tag == "Player1" && bowHandEquippable == true)
        {
            transform.SetParent(weaponHand);
            transform.localPosition = Vector3.zero;

            // Notifica al Player1Controller que s'ha equipat l'arco
            Player1Controller controller = collision.gameObject.GetComponent<Player1Controller>();
            if (controller != null)
            {
                controller.EquipBow(gameObject); // Passem l'arc recollit
            }

        }
        // ESPASA
        if (collision.gameObject.tag == "Player2" && swordHandEquippable == true)
        {
            Transform swordPivot = weaponHand.Find("SwordPivot");
            transform.SetParent(swordPivot);
            transform.localPosition = Vector3.zero;


            // Notifica al Player2Controller que s'ha equipat l'espasa
            Player2Controller controller = collision.gameObject.GetComponent<Player2Controller>();
            if (controller != null)
            {
                controller.EquipSword(gameObject); // Passem l'espasa recollida
            }

        }
        // ESCUT
        if (collision.gameObject.tag == "Player2" && shieldHandEquippable == true)
        {
            transform.SetParent(shieldHand);
            transform.localPosition = Vector3.zero;

            // Notifica al Player2Controller que s'ha equipat l'escut
            Player2Controller controller = collision.gameObject.GetComponent<Player2Controller>();
            if (controller != null)
            {
                controller.EquipShield(gameObject); // Passem l'escut recollit
            }

        }
    }
}
