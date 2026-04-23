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
}
public class InventoryManager : MonoBehaviour
{
    public Dictionary<Item, int> items = new Dictionary<Item, int>();
    public static List<FoodItemObject> foodItems = new List<FoodItemObject>();


    public void AddFoodItem(FoodItem item, float quality = 5, bool isLookalike=false)
    {
        FoodItemObject foodobject = new FoodItemObject();
        foodobject.foodItem = item;
        foodobject.starQuality = quality;
        foodobject.isLookalike = isLookalike;

        foodItems.Add(foodobject);
        if (AlexKitchenInventoryUI.Instance != null)
        {
            AlexKitchenInventoryUI.Instance.UpdateItems();
        }
    }

    public void AddFoodObject(FoodItemObject item)
    {
        AddFoodObject(item, false);
    }

    public void AddFoodObject(FoodItemObject item, bool isIntermediate)
    {
        if (item == null) return;
        item.isIntermediate = isIntermediate;
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
