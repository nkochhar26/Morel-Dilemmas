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

    [Header("Plating")]
    [Tooltip("Stars lost per ingredient that was prepared wrong (e.g. raw instead of boiled) when plated anyway.")]
    public float mispreparedPenalty = 2.5f;

    // ---- Plating station: the only place a finished dish is made ----

    // What the plate would make right now. Pure: looking at it changes nothing.
    public class PlatePreview
    {
        public FoodItem dish;                              // null = no recipe uses these ingredients
        public float quality;                              // 0-5, after wrong-prep penalties
        public List<string> problems = new List<string>(); // e.g. "Tomato isn't chopped"
        public bool IsMatch => dish != null;
        public bool IsExact => dish != null && problems.Count == 0;
    }

    // Finds the recipe whose food items are exactly what's on the plate. Correct preparation
    // (the ingredient's own steps + the recipe's final step) is preferred, but the right
    // ingredients are enough: each wrongly prepared one costs `mispreparedPenalty` stars,
    // so the player is never stuck with an unusable plate.
    // Poison/lookalikes are ignored for matching and never revealed in the preview.
    public PlatePreview PreviewPlate(List<FoodItemObject> ingredients)
    {
        PlatePreview preview = new PlatePreview();
        List<FoodItemObject> food = FoodOnly(ingredients);
        if (food.Count == 0) return preview;

        FoodItem bestDish = null;
        List<KeyValuePair<FoodItemObject, FoodItemQualifiers>> bestWrong = null;
        foreach (FoodItem dish in allFoodItems)
        {
            if (dish == null || dish.recipe == null || dish.recipe.foodList == null) continue;
            if (!System.Enum.IsDefined(typeof(CookingStep), dish.recipe.step)) continue; // test/unfinished recipes

            List<FoodItemQualifiers> required = dish.recipe.foodList.FindAll(q => q != null && q.foodItem != null);
            if (required.Count == 0 || required.Count != food.Count) continue;

            if (!MatchIngredients(required, food, dish.recipe.step, out var wrong)) continue;
            if (bestDish == null || wrong.Count < bestWrong.Count)
            {
                bestDish = dish;
                bestWrong = wrong;
                if (wrong.Count == 0) break; // can't do better than exact
            }
        }
        if (bestDish == null) return preview;

        float total = 0f;
        foreach (FoodItemObject i in food)
        {
            float q = i.starQuality;
            if (bestWrong.Exists(w => w.Key == i)) q -= mispreparedPenalty;
            total += Mathf.Max(0f, q);
        }

        preview.dish = bestDish;
        preview.quality = total / food.Count;
        foreach (var w in bestWrong)
        {
            preview.problems.Add(DescribeProblem(w.Key, w.Value, bestDish.recipe.step));
        }
        return preview;
    }

    // Makes the dish shown by PreviewPlate (and applies poison). Returns null if nothing matches.
    public FoodItemObject PlateIngredients(List<FoodItemObject> ingredients)
    {
        PlatePreview preview = PreviewPlate(ingredients);
        if (!preview.IsMatch) return null;

        bool poisoned = ingredients.Exists(i => i != null && i.foodItem != null
            && (i.foodItem.name == "Poison" || i.isLookalike));
        if (poisoned) GameManager.Instance.orderManager.SetPoisonous(true);

        FoodItemObject result = new FoodItemObject();
        result.foodItem = preview.dish;
        result.starQuality = preview.quality;
        result.tags = new List<CookingStep>();
        result.isIntermediate = false;
        return result;
    }

    private static List<FoodItemObject> FoodOnly(List<FoodItemObject> ingredients)
    {
        List<FoodItemObject> food = new List<FoodItemObject>();
        if (ingredients == null) return food;
        foreach (FoodItemObject i in ingredients)
        {
            if (i == null || i.foodItem == null || i.foodItem.name == "Poison") continue;
            food.Add(i);
        }
        return food;
    }

    // Pairs each required ingredient with one on the plate (same food item).
    // Correctly prepared ones are matched first; `wrong` lists the rest with what they were meant to be.
    // Returns false if the plate doesn't hold the right set of food items at all.
    private bool MatchIngredients(List<FoodItemQualifiers> required, List<FoodItemObject> food,
        CookingStep finalStep, out List<KeyValuePair<FoodItemObject, FoodItemQualifiers>> wrong)
    {
        wrong = new List<KeyValuePair<FoodItemObject, FoodItemQualifiers>>();
        List<FoodItemObject> unmatched = new List<FoodItemObject>(food);
        List<FoodItemQualifiers> stillNeeded = new List<FoodItemQualifiers>();

        foreach (FoodItemQualifiers q in required)
        {
            FoodItemObject exact = unmatched.Find(i => IngredientMeetsRequirement(i, q, finalStep));
            if (exact != null) unmatched.Remove(exact);
            else stillNeeded.Add(q);
        }
        foreach (FoodItemQualifiers q in stillNeeded)
        {
            FoodItemObject sameFood = unmatched.Find(i => i.foodItem == q.foodItem);
            if (sameFood == null) return false;
            unmatched.Remove(sameFood);
            wrong.Add(new KeyValuePair<FoodItemObject, FoodItemQualifiers>(sameFood, q));
        }
        return unmatched.Count == 0;
    }

    private static HashSet<CookingStep> NeededSteps(FoodItemQualifiers q, CookingStep finalStep)
    {
        HashSet<CookingStep> needed = new HashSet<CookingStep>();
        if (q.tag != null) needed.UnionWith(q.tag);
        needed.Add(finalStep);
        return needed;
    }

    // Same ingredient, and prepared with exactly the steps the recipe asks for
    private bool IngredientMeetsRequirement(FoodItemObject ingredient, FoodItemQualifiers q, CookingStep finalStep)
    {
        if (ingredient.foodItem != q.foodItem) return false;
        HashSet<CookingStep> has = new HashSet<CookingStep>();
        if (ingredient.tags != null) has.UnionWith(ingredient.tags);
        return has.SetEquals(NeededSteps(q, finalStep));
    }

    // e.g. "Tomato isn't chopped", "Cremini shouldn't be sauteed"
    private static string DescribeProblem(FoodItemObject ingredient, FoodItemQualifiers q, CookingStep finalStep)
    {
        HashSet<CookingStep> needed = NeededSteps(q, finalStep);
        HashSet<CookingStep> has = new HashSet<CookingStep>();
        if (ingredient.tags != null) has.UnionWith(ingredient.tags);

        List<string> missing = new List<string>();
        foreach (CookingStep s in needed) if (!has.Contains(s)) missing.Add(PastTense(s));
        List<string> extra = new List<string>();
        foreach (CookingStep s in has) if (!needed.Contains(s)) extra.Add(PastTense(s));

        List<string> parts = new List<string>();
        if (missing.Count > 0) parts.Add("isn't " + string.Join(" or ", missing));
        if (extra.Count > 0) parts.Add("shouldn't be " + string.Join(" or ", extra));
        return FoodName(ingredient.foodItem) + " " + string.Join(" and ", parts);
    }

    public static string FoodName(FoodItem item)
    {
        if (item == null) return "";
        return string.IsNullOrEmpty(item.itemName) ? item.name : item.itemName;
    }

    private static string PastTense(CookingStep step)
    {
        switch (step)
        {
            case CookingStep.Chop: return "chopped";
            case CookingStep.Boil: return "boiled";
            case CookingStep.Sautee: return "sauteed";
            default: return step.ToString().ToLower();
        }
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
