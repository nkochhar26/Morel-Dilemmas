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

    public FoodItemObject IngredientsToFood(CookingStep step, List<FoodItemObject> ingredients)
    {
        bool isPoisoned;
        FoodItemObject newFood = new FoodItemObject();


        foreach(FoodItem foodData in allFoodItems)
        {
            if (foodData.recipe.step != step)
            {
                continue;
            }
            if (ingredients.Count != foodData.recipe.foodList.Count)
            {
                continue;
            }

            foreach(FoodItemObject ingredient in ingredients)
            {
                //TODO: Fix this scuffed code
                if (ingredient.foodItem.name == "Poison")
                {
                    GameManager.Instance.orderManager.SetPoisonous(true);
                }
                if (!FoodObjectMatchesQualifiers(ingredient, foodData.recipe.foodList[ingredients.IndexOf(ingredient)]))
                {
                    continue;
                }
            }

            newFood.foodItem = foodData;
            
        }

        newFood.starQuality = CalculateQuality(ingredients);
        return newFood;
    }

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

    public bool FoodObjectMatchesQualifiers(FoodItemObject foodObject, FoodItemQualifiers qualifiers)
    {
        if (qualifiers.foodItem != foodObject.foodItem)
        {
            return false;
        }
        if (qualifiers.tag != foodObject.tags) return false;
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
