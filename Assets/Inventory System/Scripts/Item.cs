using UnityEngine;

public class Item : MonoBehaviour, IPickupBehaviour
{
    [SerializeField]
    ItemStructureSO itemStructure;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            OnPickup();
        }
    }

    private void Update()
    {
        float rotateSpeed = 720;

        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y + rotateSpeed * Time.deltaTime,
            transform.localEulerAngles.z
            );
    }

    public void OnPickup()
    {
        //For now, just add the item to the inventory
        BackpackInventorySystem.Instance.TryPlacingItemInInventory(itemStructure);
        Destroy(gameObject);
    }
}
