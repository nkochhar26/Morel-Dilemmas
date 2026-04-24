using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SliceOperation
{
    public Vector3 centerPoint;
    public Vector3 planeNormal;
    public SliceOperation upperHullSlice;
    public SliceOperation lowerHullSlice;
    public bool upperDestroyed = false;
    public bool lowerDestroyed = false;
}

[System.Serializable]
public class FoodItemObject
{
    public FoodItem foodItem;
    public float starQuality;
    public bool isLookalike;
    public List<CookingStep> tags = new List<CookingStep>();
    public SliceOperation sliceOperation;
    public bool isIntermediate;
    public int quantity;
}

public class InventoryManager : MonoBehaviour
{
    public Dictionary<Item, int> items = new Dictionary<Item, int>();
    public static List<FoodItemObject> foodItems = new List<FoodItemObject>();


    public void AddFoodItem(FoodItem item, float quality = 5, int quantity =1, bool isLookalike=false)
    {
        foreach (FoodItemObject f in foodItems)
        {
            if (item == f.foodItem && f.starQuality == quality)
            {
                f.quantity += quantity;
                if (AlexKitchenInventoryUI.Instance != null)
                {
                    AlexKitchenInventoryUI.Instance.UpdateItems();
                }
            return;
            }
        }
        FoodItemObject foodobject = new FoodItemObject();
        foodobject.foodItem = item;
        foodobject.starQuality = quality;
        foodobject.isLookalike = isLookalike;
        foodobject.quantity = quantity;

        foodItems.Add(foodobject);
        if (AlexKitchenInventoryUI.Instance != null)
        {
            AlexKitchenInventoryUI.Instance.UpdateItems();
        }
    }

    public void RemoveFoodItem(FoodItemObject item)
    {
        for (int i = 0; i < foodItems.Count; i++)
        {
            if (item.foodItem == foodItems[i].foodItem && foodItems[i].starQuality == item.starQuality)
            {
                foodItems[i].quantity -= 1;
                if (foodItems[i].quantity <= 0)
                {
                    foodItems.RemoveAt(i);
                }
                return;
            }
        }
    }

    public void AddFoodObject(FoodItemObject item)
    {
        // AddFoodObject(item, false, null);
        if (item == null) return;
        // item.isIntermediate = isIntermediate;
        // item.tags.Add(step);
        foodItems.Add(item);
        if (AlexKitchenInventoryUI.Instance != null)
        {
            AlexKitchenInventoryUI.Instance.UpdateItems();
        }
    }

    public void AddFoodObject(FoodItemObject item, bool isIntermediate, CookingStep step)
    {
        if (item == null) return;
        item.isIntermediate = isIntermediate;
        item.tags.Add(step);
        foodItems.Add(item);
        if (AlexKitchenInventoryUI.Instance != null)
        {
            AlexKitchenInventoryUI.Instance.UpdateItems();
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

    public List<FoodItemObject> GetFoodItems()
    {
        return foodItems;
    }
}
