using BigRookGames.Weapons;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    public GunfireController gunScript;
    public Transform player, gunContainer;

    public float pickupRange;

    public bool equipped;

    public GameObject EquippedGun;
    public GameObject DroppedGun;

    private void Start()
    {
        // Setup
        if (equipped)
        {
            gunScript.enabled = true;
        }
        else
        {
            gunScript.enabled = false;
        }
    }
    private void Update()
    {
        // Check if player is in range and Interact is pressed
        Vector3 distance = player.position - transform.position;
        if (!equipped && distance.magnitude <= pickupRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        equipped = true;

        gunScript.enabled = true;
        DroppedGun.SetActive(false);
        EquippedGun.SetActive(true);
    }
}
