using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public OrderManager orderManager;
    public InventoryManager inventoryManager;
    public CustomerManager customerManager;
    public StarManager starManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void SaveForagingProgress()
    {
        if (inventoryManager != null)
        {
            Debug.Log("GameManager: Inventory sync complete. Moving to next scene with " + InventoryManager.foodItems.Count + " items.");
        }
    }
}
