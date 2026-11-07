using System.Collections.Generic;
using UnityEngine;

// The only station that turns ingredients into a finished dish.
// Drop prepared (chopped/boiled/sauteed) ingredients here; once they exactly match
// a recipe, the dish is made and becomes the held order.
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

        TryMakeDish();
    }

    private void TryMakeDish()
    {
        itemsOnPlate.RemoveAll(i => i == null);

        List<FoodItemObject> ingredients = new List<FoodItemObject>();
        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item.foodItem != null)
            {
                ingredients.Add(item.foodItem);
            }
        }

        FoodItemObject dish = FoodManager.Instance.PlateIngredients(ingredients);
        if (dish == null)
        {
            return;
        }

        GameManager.Instance.orderManager.SetHeldOrder(dish);

        foreach (InventoryItem item in itemsOnPlate)
        {
            Destroy(item.gameObject);
        }
        itemsOnPlate.Clear();
    }

    public void ClearPlating()
    {
        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item == null) continue;
            if (item.foodItem != null)
            {
                // keep whatever preparation it already had
                GameManager.Instance.inventoryManager.AddFoodObject(item.foodItem, item.foodItem.isIntermediate);
            }
            Destroy(item.gameObject);
        }
        itemsOnPlate.Clear();
    }
}
