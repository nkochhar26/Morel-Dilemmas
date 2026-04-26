using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class Receipt : MonoBehaviour
{
    public Image dishImage;
    public List<GameObject> steps = new List<GameObject>();
    public GameObject stepPrefab;
    public Vector2 originalPosition, originalSize;
    public TableOutline table;
    public GameObject recipeHolder;
    public GameObject ingredientPrefab;


    void Start()
    {
        originalPosition = this.GetComponent<Transform>().localPosition;
        originalSize = this.GetComponent<Transform>().localScale;
    }

    public void LoadReceipt(FoodItem recipe, TableOutline table)
    {
        // load dish
        dishImage.sprite = recipe.defaultSprite;
        this.table = table;
        // load steps
        LoadRecipe(recipe);
    }

    public void LoadRecipe(FoodItem recipe)
    {
        foreach (FoodItemQualifiers food in recipe.recipe.foodList)
        {
            GameObject currFoodIcon = Instantiate(ingredientPrefab, recipeHolder.transform);
            currFoodIcon.GetComponent<Image>().sprite = food.foodItem.defaultSprite;
            currFoodIcon.GetComponent<IngredientUI>().SetTags(food.tag);
        }
    }

    public void Grow()
    {
        transform.DOScale(originalSize * 1.2f, 0.2f);
        table.SetOutline(true);
    }

    public void Shrink()
    {
        transform.DOScale(originalSize, 0.2f);
        table.SetOutline(false);
    }

    private void OnDestroy()
    {
        if (table != null)
        {
            table.SetOutline(false);
        }
    }

}
