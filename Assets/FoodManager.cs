using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance;
    public List<FoodItem> allFoodItems = new List<FoodItem>();

    private void Awake()
    {
        Instance = this;
    }

    // ---- Cooking stations: mark each ingredient as having gone through a step ----
    public void ApplyCookingStep(FoodItemObject ingredient, CookingStep step)
    {
        if (ingredient == null) return;
        if (ingredient.tags == null) ingredient.tags = new List<CookingStep>();
        if (!ingredient.tags.Contains(step)) ingredient.tags.Add(step);
        ingredient.isIntermediate = true;
    }

    // ---- Plating station: the only place a finished dish is made ----
    // Returns the dish if the plate holds exactly one recipe's ingredients, each prepared
    // with the right steps (the ingredient's own tags + the recipe's final step).
    // Returns null otherwise. Poison/lookalikes don't block a match; they poison the dish.
    public FoodItemObject PlateIngredients(List<FoodItemObject> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0) return null;

        bool poisoned = false;
        List<FoodItemObject> food = new List<FoodItemObject>();
        foreach (FoodItemObject ingredient in ingredients)
        {
            if (ingredient == null || ingredient.foodItem == null) continue;
            if (ingredient.foodItem.name == "Poison")
            {
                poisoned = true;
                continue;
            }
            food.Add(ingredient);
        }
        if (food.Count == 0) return null;

        foreach (FoodItem dish in allFoodItems)
        {
            if (dish == null || dish.recipe == null || dish.recipe.foodList == null) continue;
            if (!System.Enum.IsDefined(typeof(CookingStep), dish.recipe.step)) continue; // test/unfinished recipes

            List<FoodItemQualifiers> required = dish.recipe.foodList.FindAll(q => q != null && q.foodItem != null);
            if (required.Count == 0 || required.Count != food.Count) continue;

            List<FoodItemObject> unmatched = new List<FoodItemObject>(food);
            bool allMatched = true;
            foreach (FoodItemQualifiers q in required)
            {
                FoodItemObject match = unmatched.Find(i => IngredientMeetsRequirement(i, q, dish.recipe.step));
                if (match == null) { allMatched = false; break; }
                unmatched.Remove(match);
            }
            if (!allMatched) continue;

            if (food.Exists(i => i.isLookalike)) poisoned = true;
            if (poisoned) GameManager.Instance.orderManager.SetPoisonous(true);

            FoodItemObject result = new FoodItemObject();
            result.foodItem = dish;
            result.starQuality = CalculateQuality(food);
            result.tags = new List<CookingStep>();
            result.isIntermediate = false;
            return result;
        }
        return null;
    }

    // Same ingredient, and prepared with exactly the steps the recipe asks for
    private bool IngredientMeetsRequirement(FoodItemObject ingredient, FoodItemQualifiers q, CookingStep finalStep)
    {
        if (ingredient.foodItem != q.foodItem) return false;

        HashSet<CookingStep> needed = new HashSet<CookingStep>();
        if (q.tag != null) needed.UnionWith(q.tag);
        needed.Add(finalStep);

        HashSet<CookingStep> has = new HashSet<CookingStep>();
        if (ingredient.tags != null) has.UnionWith(ingredient.tags);

        return has.SetEquals(needed);
    }

    [System.Obsolete("Stations no longer make dishes. Use ApplyCookingStep at stations and PlateIngredients at the plate.")]
    public FoodItemObject IngredientsToFood(CookingStep step, List<FoodItemObject> ingredients)
    {
        foreach(FoodItem foodData in allFoodItems)
        {
            var remaining = new List<FoodItemQualifiers>(foodData.recipe.foodList);
            int ingredientCount = 0;
            if (step != foodData.recipe.step)
            {
                continue;
            }
            foreach (var ingredient in ingredients)
            {
                if (ingredient.foodItem.name == "Poison")
                {
                    GameManager.Instance.orderManager.SetPoisonous(true);
                    continue;
                }

                ingredientCount += 1;
                FoodItemQualifiers match = null;

                foreach (var r in remaining)
                {
                    if (FoodObjectMatchesQualifiers(ingredient, r))
                    {
                        match = r;
                        if (ingredient.isLookalike)
                        {
                            GameManager.Instance.orderManager.SetPoisonous(true);
                        }
                        break;
                    }
                }

                if (match != null)
                {
                    remaining.Remove(match);
                }
            }
            if (remaining.Count == 0 && ingredientCount == foodData.recipe.foodList.Count)
            {
                FoodItemObject rtn = new FoodItemObject();
                rtn.foodItem = foodData;
                rtn.starQuality = CalculateQuality(ingredients);
                rtn.tags = new List<CookingStep>();
                return rtn;
            }
        }

        return null;
    }

    // public FoodItemObject IngredientsToFood(CookingStep step, List<FoodItemObject> ingredients)
    // {
    //     bool isPoisoned;
    //     FoodItemObject newFood = new FoodItemObject();


    //     foreach(FoodItem foodData in allFoodItems)
    //     {
    //         bool isDish = true;
    //         if (foodData.recipe.step != step)
    //         {
    //             continue;
    //         }
    //         // if (ingredients.Count != foodData.recipe.foodList.Count)
    //         // {
    //         //     continue;
    //         // }

    //         int ingredientCount = 0;
    //         foreach(FoodItemObject ingredient in ingredients)
    //         {
    //             //TODO: Fix this scuffed code
    //             if (ingredient.foodItem.name == "Poison")
    //             {
    //                 GameManager.Instance.orderManager.SetPoisonous(true);
    //             }
    //             ingredientCount += 1;
    //             else if (!FoodObjectMatchesQualifiers(ingredient, foodData.recipe.foodList[ingredients.IndexOf(ingredient)]))
    //             {
    //                 isDish = false;
    //             }
    //         }
    //         if (isDish)
    //         {
    //             newFood.foodItem = foodData;
    //         }
            
    //     }

    //     newFood.starQuality = CalculateQuality(ingredients);
    //     return newFood;
    // }

    public float CalculateQuality(List<FoodItemObject> ingredients)
    {
        float quality = 0;
        foreach (FoodItemObject ingredient in ingredients)
        {
            quality += ingredient.starQuality;
        }
        quality /= ingredients.Count;
        return quality;
    }

    //TODO: IDK BRO whats happening with the tags
    public bool FoodObjectMatchesQualifiers(FoodItemObject foodObject, FoodItemQualifiers qualifiers)
    {
        if (qualifiers.foodItem != foodObject.foodItem)
        {
            return false;
        }
        // if (qualifiers.tag != foodObject.tags)
        // {
        //     Debug.Log("Second input" +  qualifiers.tag);
        //     Debug.Log("First input" + foodObject.tags);
        //     return false;
        // }
        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("Load All FoodItems")]
    public void LoadAllFoodItems()
    {
        allFoodItems.Clear();
        string[] guids = AssetDatabase.FindAssets("t:FoodItem");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FoodItem item = AssetDatabase.LoadAssetAtPath<FoodItem>(path);
            if (item != null)
                allFoodItems.Add(item);
        }
        EditorUtility.SetDirty(this);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(FoodManager))]
public class FoodManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FoodManager manager = (FoodManager)target;
        if (GUILayout.Button("Load All FoodItems"))
        {
            manager.LoadAllFoodItems();
        }
    }
}
#endif
