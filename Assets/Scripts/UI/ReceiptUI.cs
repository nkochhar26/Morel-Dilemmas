using UnityEngine;
using System.Collections.Generic;

public class ReceiptUI : MonoBehaviour
{
    public GameObject receiptPrefab;
    public List<GameObject> receipts = new List<GameObject>();
    public GameObject[] tables = new GameObject[5];

    public void Start()
    {
        GameManager.Instance.orderManager.OnOrdersChanged += LoadReceipts;
        tables = FindFirstObjectByType<CustomerSpawner>().GetTables();
    }

    public void LoadReceipts()
    {
        foreach (GameObject receipt in receipts)
        {
            Destroy(receipt);
        }
        FoodItem[] currentOrders = GameManager.Instance.orderManager.GetCurrentOrders();
        for (int i = 0; i < currentOrders.Length; i++)
        {
            if (currentOrders[i] == null)
            {
                continue;
            }
            AddReceipt(i, currentOrders[i], tables[i].GetComponent<TableOutline>());
        }
    }

    public void AddReceipt(int tableNum, FoodItem foodItem, TableOutline table)
    {
        GameObject receipt = Instantiate(receiptPrefab, this.gameObject.transform);
        receipt.GetComponent<Receipt>().LoadReceipt(foodItem, table);
        receipts.Add(receipt);
    }
}
