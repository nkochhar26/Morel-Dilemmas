using System.Collections.Generic;
using UnityEngine;

public class Plating : DragFoodInto
{
    public List<InventoryItem> itemsOnPlate = new List<InventoryItem>();

    public override void AddItem(InventoryItem item)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        base.AddItem(item);

        if (item != null && !itemsOnPlate.Contains(item))
        {
            itemsOnPlate.Add(item);
        }

        TryMatchIntermediateRecipe();
    }

    private void TryMatchIntermediateRecipe()
    {
        List<FoodItemObject> intermediateIngredients = new List<FoodItemObject>();
        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item != null && item.foodItem != null && item.foodItem.isIntermediate)
            {
                intermediateIngredients.Add(item.foodItem);
            }
        }

        if (intermediateIngredients.Count == 0)
        {
            return;
        }

        FoodItemObject matchedDish = FoodManager.Instance.IngredientsToFood(CookingStep.Boil, intermediateIngredients);
        if (matchedDish == null || matchedDish.foodItem == null)
        {
            return;
        }

        matchedDish.isIntermediate = false;
        GameManager.Instance.orderManager.SetHeldOrder(matchedDish);

        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        itemsOnPlate.Clear();
    }

    public void ClearPlating()
    {
        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item != null && item.foodItem != null)
            {
                GameManager.Instance.inventoryManager.AddFoodObject(item.foodItem);
            }
            Destroy(item.gameObject);
        }
        itemsOnPlate.Clear();
    }
}
