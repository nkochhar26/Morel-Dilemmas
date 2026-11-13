using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The only station that turns ingredients into a finished dish.
// Drop ingredients on the plate, then press the plate button (PlateDish).
// The button's label always shows what you'll get before you press it:
//   "Plate Tomato Cremini Soup"  + quality, or a warning like "Tomato isn't chopped".
// Wrongly prepared ingredients still make the dish (with a star penalty), so the
// player is never stuck; ingredients no recipe uses can only be cleared.
public class Plating : DragFoodInto
{
    public List<InventoryItem> itemsOnPlate = new List<InventoryItem>();

    [Header("Plate button")]
    [Tooltip("The 'Plate it' button. Its On Click should call Plating.PlateDish. Greyed out when nothing can be made.")]
    public Button plateButton;
    [Tooltip("The button's label. Updated live with what the plate will make.")]
    public TMP_Text plateButtonText;

    [Header("Feedback")]
    [Tooltip("Optional text that flashes after plating (e.g. warnings).")]
    public TMP_Text resultText;

    private const string Grey = "#8A8A8A";
    private const string Warn = "#D9480F";

    void Start()
    {
        if (resultText != null)
        {
            resultText.raycastTarget = false; // display-only: never block the plate buttons
            resultText.gameObject.SetActive(false);
        }
        RefreshPlateButton();
    }

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
        RefreshPlateButton();
    }

    // Plate button: make whatever the button currently shows
    public void PlateDish()
    {
        List<FoodItemObject> ingredients = CurrentIngredients();
        FoodManager.PlatePreview preview = FoodManager.Instance.PreviewPlate(ingredients);
        if (!preview.IsMatch)
        {
            RefreshPlateButton();
            return;
        }

        FoodItemObject dish = FoodManager.Instance.PlateIngredients(ingredients);
        if (dish == null) return;

        if (!preview.IsExact)
        {
            CookingDoneness.ShowMessage(this, resultText,
                FoodManager.FoodName(dish.foodItem) + ": " + string.Join(", ", preview.problems),
                new Color(1f, 0.6f, 0.2f), 2.5f);
        }

        GameManager.Instance.orderManager.SetHeldOrder(dish);

        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item != null) Destroy(item.gameObject);
        }
        itemsOnPlate.Clear();
        RefreshPlateButton();
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
        RefreshPlateButton();
    }

    private List<FoodItemObject> CurrentIngredients()
    {
        itemsOnPlate.RemoveAll(i => i == null);
        List<FoodItemObject> ingredients = new List<FoodItemObject>();
        foreach (InventoryItem item in itemsOnPlate)
        {
            if (item.foodItem != null) ingredients.Add(item.foodItem);
        }
        return ingredients;
    }

    // Shows exactly what pressing the button will do
    private void RefreshPlateButton()
    {
        List<FoodItemObject> ingredients = CurrentIngredients();
        string label;
        bool canPlate;

        if (ingredients.Count == 0)
        {
            label = $"<color={Grey}>Add ingredients</color>";
            canPlate = false;
        }
        else
        {
            FoodManager.PlatePreview preview = FoodManager.Instance.PreviewPlate(ingredients);
            if (!preview.IsMatch)
            {
                label = $"<color={Grey}>No recipe matches</color>\n<size=60%><color={Grey}>Clear to take them back</color></size>";
                canPlate = false;
            }
            else
            {
                string dishName = FoodManager.FoodName(preview.dish);
                string quality = $"Quality {preview.quality:0.#} / 5";
                if (preview.IsExact)
                {
                    label = $"Plate {dishName}\n<size=60%>{quality}</size>";
                }
                else
                {
                    string penalty = $"-{FoodManager.Instance.mispreparedPenalty:0.#} stars each";
                    label = $"Plate {dishName}\n<size=60%><color={Warn}>Warning: {string.Join(", ", preview.problems)} ({penalty})</color></size>";
                }
                canPlate = true;
            }
        }

        if (plateButtonText != null) plateButtonText.text = label;
        if (plateButton != null) plateButton.interactable = canPlate;
    }
}
