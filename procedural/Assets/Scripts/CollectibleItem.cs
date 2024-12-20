using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string itemName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventorySystem inventory = other.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                inventory.AddItem(itemName);
                Destroy(gameObject);
            }
        }
    }
}
