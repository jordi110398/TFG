using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    public bool swordHandEquippable;
    public bool bowHandEquippable;
    public bool itemConsumable;
    public bool shieldHandEquippable;
    public bool boomerangEquippable; // Marca si aquest pickup és un boomerang

    public bool onlyPlayer1; // Marca a l'Inspector si aquest pickup és per Player1
    public bool onlyPlayer2; // Marca a l'Inspector si aquest pickup és per Player2

    private Transform weaponHand;
    private Transform swordHand;
    private Transform shieldHand;

    private GameObject playerNearby = null;

    public GameObject outlineObject;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            playerNearby = collision.gameObject;
            outlineObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == playerNearby)
        {
            playerNearby = null;
            outlineObject.SetActive(false);
        }
    }

    // Aquest mètode l'ha de cridar el Player quan prem la tecla/botó d'agafar
    public void TryPickUp(GameObject player)
    {
        Debug.Log("TryPickUp cridat per: " + player.name);
        //if (player != playerNearby) return;

        // POCIÓ i CLAU
        if (itemConsumable)
        {
            if (player.tag == "Player1")
            {
                Transform itemSlot = player.transform.Find("ItemSlot");
                if (itemSlot != null)
                {
                    transform.SetParent(itemSlot);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    player.GetComponent<Player1Controller>()?.EquipItem(gameObject);
                    // Desactiva el collider perquè no es pugui tornar a detectar
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null)
                        col.enabled = false;
                }
            }
            else if (player.tag == "Player2")
            {
                Transform itemSlot = player.transform.Find("ItemSlot");
                if (itemSlot != null)
                {
                    transform.SetParent(itemSlot);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    player.GetComponent<Player2Controller>()?.EquipItem(gameObject);
                    // Desactiva el collider perquè no es pugui tornar a detectar
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null)
                        col.enabled = false;
                }
            }
        }

        // ARC
        if (player.tag == "Player1" && bowHandEquippable)
        {
            if (weaponHand == null)
                weaponHand = GameObject.Find("WeaponHand").transform;
            transform.SetParent(weaponHand);
            transform.localPosition = Vector3.zero;
            Player1Controller controller = player.GetComponent<Player1Controller>();
            if (controller != null)
                controller.EquipBow(gameObject);
        }
        // BOOMERANG
        if (player.tag == "Player1" && boomerangEquippable)
        {
            if (weaponHand == null)
                weaponHand = GameObject.Find("WeaponHand").transform;
            transform.SetParent(weaponHand);
            transform.localPosition = Vector3.zero;
            Player1Controller controller = player.GetComponent<Player1Controller>();
            if (controller != null)
                controller.EquipBoomerang(gameObject);
        }
        // ESPASA (ja està equipada)
        /*

        if (player.tag == "Player2" && swordHandEquippable)
        {
            if (weaponHand == null)
                weaponHand = GameObject.Find("WeaponHand").transform;
            Transform swordPivot = weaponHand.Find("SwordPivot");
            transform.SetParent(swordPivot);
            //transform.localPosition = Vector3.zero;
            Player2Controller controller = player.GetComponent<Player2Controller>();
            if (controller != null)
                controller.EquipSword(gameObject);
        }
        */

        // ESCUT
        if (player.tag == "Player2" && shieldHandEquippable)
        {
            Debug.Log("Intentant equipar escut");
            if (shieldHand == null)
            {
                shieldHand = GameObject.Find("ShieldHand")?.transform;
                Debug.Log("shieldHand trobat? " + (shieldHand != null));
            }
            transform.SetParent(shieldHand);
            transform.localPosition = Vector3.zero;
            Player2Controller controller = player.GetComponent<Player2Controller>();
            if (controller != null)
            {
                Debug.Log("EquipShield cridat");
                controller.EquipShield(gameObject);
            }
        }
    }
}
