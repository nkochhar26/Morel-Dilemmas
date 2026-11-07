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
    public int quantity = 1;

    // A separate single unit of this item, so moving one out of a stack
    // never changes (or gets re-added as) the stack itself
    public FoodItemObject CloneSingle()
    {
        return new FoodItemObject
        {
            foodItem = foodItem,
            starQuality = starQuality,
            isLookalike = isLookalike,
            tags = new List<CookingStep>(tags),
            sliceOperation = sliceOperation,
            isIntermediate = isIntermediate,
            quantity = 1,
        };
    }

    // Same food, quality, lookalike status and preparation (e.g. both chopped) share one stack.
    // The exact slice shape isn't compared: the stack keeps the first item's shape.
    public bool CanStackWith(FoodItemObject other)
    {
        return other != null
            && foodItem == other.foodItem
            && starQuality == other.starQuality
            && isLookalike == other.isLookalike
            && isIntermediate == other.isIntermediate
            && SameTags(tags, other.tags);
    }

    private static bool SameTags(List<CookingStep> a, List<CookingStep> b)
    {
        int countA = a == null ? 0 : a.Count;
        int countB = b == null ? 0 : b.Count;
        if (countA != countB) return false;
        if (countA == 0) return true;
        var sortedA = new List<CookingStep>(a); sortedA.Sort();
        var sortedB = new List<CookingStep>(b); sortedB.Sort();
        for (int i = 0; i < sortedA.Count; i++)
        {
            if (sortedA[i] != sortedB[i]) return false;
        }
        return true;
    }
}

public class InventoryManager : MonoBehaviour
{
    public Dictionary<Item, int> items = new Dictionary<Item, int>();
    public static List<FoodItemObject> foodItems = new List<FoodItemObject>();


    public void AddFoodItem(FoodItem item, float quality = 5, int quantity =1, bool isLookalike=false)
    {
        FoodItemObject foodobject = new FoodItemObject();
        foodobject.foodItem = item;
        foodobject.starQuality = quality;
        foodobject.isLookalike = isLookalike;
        foodobject.quantity = quantity;
        AddToStacks(foodobject);
    }

    // Merge into a matching stack if there is one, otherwise add a new entry
    private void AddToStacks(FoodItemObject item)
    {
        if (item.quantity <= 0) item.quantity = 1;

        if (!foodItems.Contains(item))
        {
            FoodItemObject stack = foodItems.Find(f => f.CanStackWith(item));
            if (stack != null)
            {
                stack.quantity += item.quantity;
            }
            else
            {
                foodItems.Add(item);
            }
        }

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
        AddFoodObject(item, false);
    }

    public void AddFoodObject(FoodItemObject item, bool isIntermediate)
    {
        if (item == null) return;
        item.isIntermediate = isIntermediate;
        AddToStacks(item);
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
