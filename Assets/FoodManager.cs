using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
        if (!qualifiers.tag.OrderBy(x => x).SequenceEqual(foodObject.tags.OrderBy(x => x)))
        {
            Debug.Log("Qualifiers tags: " + string.Join(", ", qualifiers.tag));
            Debug.Log("FoodObject tags: " + string.Join(", ", foodObject.tags));
            return false;
        }
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