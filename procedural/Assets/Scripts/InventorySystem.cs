using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<string> inventory = new List<string>();

    public void AddItem(string itemName)
    {
        inventory.Add(itemName);
        Debug.Log($"Item coletado: {itemName}");
    }

    public void ShowInventory()
    {
        Debug.Log("Inventário:");
        foreach (var item in inventory)
        {
            Debug.Log($"- {item}");
        }
    }
}
