using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public Dictionary<Item, int> items = new Dictionary<Item, int>();
    public Dictionary<FoodItem, int> foodItems = new Dictionary<FoodItem, int>();

    public void AddFoodItem(FoodItem item, int quantity = 1)
    {
        if (foodItems.ContainsKey(item))
        {
            foodItems[item] += quantity;
        }
        else
        {
            foodItems.Add(item, quantity);
        }
    }

    public void AddItem(Item item) // remove later
    {
        if (items.ContainsKey(item))
        {
            items[item] += 1;
        }
        else
        {
            items.Add(item, 1);
        }
    }

    public void AddItem(Item item, int quantity) // remove later
    {
        if (items.ContainsKey(item))
        {
            items[item] += quantity;
        }
        else
        {
            items.Add(item, quantity);
        }
    }

    public Dictionary<Item, int> GetItems() // remove later
    {
        return items;
    }

    public Dictionary<FoodItem, int> GetFoodItems()
    {
        return foodItems;
    }
}
