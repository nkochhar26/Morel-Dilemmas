using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CreateAssetMenu(fileName = "MushroomItem", menuName = "MushroomItem", order = 0)]
public class MushroomItem : FoodItem
{
    [HideInInspector] public Sprite lookalikeSprite;  // lookalikes will be interchangeable but poisonous!
    [HideInInspector] public string lookalikeName; 
    [HideInInspector] public string lookalikeDescription;
}

[CustomEditor(typeof(MushroomItem))]
public class MushroomDataEditor : Editor
{
    private Dictionary<int, ReorderableList> tagLists = new Dictionary<int, ReorderableList>();
    private ReorderableList recipeList;

    private ReorderableList GetRecipeList(FoodItem script)
    {
        if (script.recipe.foodList == null) script.recipe.foodList = new List<TaggedFoodItem>();
        if (recipeList != null) return recipeList;

        recipeList = new ReorderableList(script.recipe.foodList, typeof(TaggedFoodItem), true, true, true, true);
        recipeList.drawHeaderCallback = r => EditorGUI.LabelField(r, "Ingredients");
        recipeList.elementHeight = 24;
        
        recipeList.drawElementCallback = (rect, i, a, f) =>
        {
            var item = script.recipe.foodList[i];
            if (item.tag == null) item.tag = new List<tags>();
            float hw = rect.width / 2 - 5;

            item.foodItem = (FoodItem)EditorGUI.ObjectField(new Rect(rect.x, rect.y + 2, hw, 20), item.foodItem, typeof(FoodItem), false);

            if (EditorGUI.DropdownButton(new Rect(rect.x + hw + 10, rect.y + 2, hw, 20),
                new GUIContent(item.tag.Count > 0 ? string.Join(", ", item.tag) : "Tags..."), FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                foreach (tags t in Enum.GetValues(typeof(tags)))
                {
                    tags tag = t;
                    menu.AddItem(new GUIContent(tag.ToString()), item.tag.Contains(tag), () =>
                    {
                        if (item.tag.Contains(tag)) item.tag.Remove(tag); else item.tag.Add(tag);
                        EditorUtility.SetDirty(script);
                    });
                }
                menu.ShowAsContext();
            }
        };
        return recipeList;
    }

    private ReorderableList GetTagList(FoodItem script, int idx)
    {
        if (script.SingleIngredientRecipes[idx].tag == null)
            script.SingleIngredientRecipes[idx].tag = new List<tags>();

        if (!tagLists.ContainsKey(idx))
        {
            var list = new ReorderableList(script.SingleIngredientRecipes[idx].tag, typeof(tags), true, false, true, true);
            
            list.drawElementCallback = (rect, i, a, f) =>
            {
                if (i >= 0 && i < script.SingleIngredientRecipes[idx].tag.Count)
                    EditorGUI.LabelField(rect, script.SingleIngredientRecipes[idx].tag[i].ToString());
            };
            
            list.onAddDropdownCallback = (rect, l) =>
            {
                var menu = new GenericMenu();
                foreach (tags t in Enum.GetValues(typeof(tags)))
                {
                    tags tag = t;
                    menu.AddItem(new GUIContent(tag.ToString()), false, () =>
                    {
                        script.SingleIngredientRecipes[idx].tag.Add(tag);
                        EditorUtility.SetDirty(script);
                    });
                }
                menu.ShowAsContext();
            };
            
            list.onRemoveCallback = (l) =>
            {
                if (l.index >= 0 && l.index < script.SingleIngredientRecipes[idx].tag.Count)
                {
                    script.SingleIngredientRecipes[idx].tag.RemoveAt(l.index);
                    tagLists.Remove(idx);
                    EditorUtility.SetDirty(script);
                }
            };
            
            list.elementHeight = 20;
            list.headerHeight = 0;
            tagLists[idx] = list;
        }
        return tagLists[idx];
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        MushroomItem script = (MushroomItem)target;
        
        // Initialize lists if null
        if (script.SingleIngredientRecipes == null) script.SingleIngredientRecipes = new List<SingleIngredientRecipe>();
        if (script.recipe == null) script.recipe = new recipe();
        if (script.recipe.foodList == null) script.recipe.foodList = new List<TaggedFoodItem>();
        
        var itemName = serializedObject.FindProperty("itemName");
        var itemDescription = serializedObject.FindProperty("itemDescription");

        // Food Item Section
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Food Item", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpBox");
        EditorGUILayout.PropertyField(itemName, new GUIContent("Name"));
        EditorGUILayout.PropertyField(itemDescription, new GUIContent("Description"));
        script.defaultSprite = (Sprite)EditorGUILayout.ObjectField("Default Sprite", script.defaultSprite, typeof(Sprite), false);
        EditorGUILayout.EndVertical();

        //lookalike info
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Lookalike Mushroom", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpBox");
        var lookalikeName = serializedObject.FindProperty("lookalikeName");  // name is used to check if one exists! leave blank if none
        var lookalikeDescription = serializedObject.FindProperty("lookalikeDescription");
        EditorGUILayout.PropertyField(lookalikeName, new GUIContent("Lookalike Name"));
        EditorGUILayout.PropertyField(lookalikeDescription, new GUIContent("Lookalike Description"));
        script.lookalikeSprite = (Sprite)EditorGUILayout.ObjectField("Lookalike Sprite", script.lookalikeSprite, typeof(Sprite), false);
        EditorGUILayout.EndVertical();

        // Recipe Section
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Recipe", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpBox");
        
        GetRecipeList(script).DoLayoutList();
        script.recipe.step = (tags)EditorGUILayout.EnumPopup("Step", script.recipe.step);

        // Recipe sentence preview
        if (script.recipe.foodList != null && script.recipe.foodList.Count > 0)
        {
            EditorGUILayout.Space(5);
            var richStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
            EditorGUILayout.LabelField($"What if I <b>{script.recipe.step}</b> the:", richStyle);
            
            for (int r = 0; r < script.recipe.foodList.Count; r++)
            {
                var item = script.recipe.foodList[r];
                string foodName = item.foodItem != null ? item.foodItem.itemName : "???";
                string line = "";
                
                if (item.tag != null && item.tag.Count > 0)
                {
                    for (int t = 0; t < item.tag.Count; t++)
                    {
                        line += $"<b>{item.tag[t]}</b>";
                        if (t < item.tag.Count - 2) line += ", ";
                        else if (t == item.tag.Count - 2) line += " and ";
                    }
                    line += " ";
                }
                line += foodName;
                
                if (r < script.recipe.foodList.Count - 1)
                    line += " with the";
                else
                    line += "?";
                
                EditorGUILayout.LabelField(line, richStyle);
            }
        }

        EditorGUILayout.EndVertical();

        // SingleIngredientRecipes Section
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Single Ingredient Recipes", EditorStyles.boldLabel);

        float w = 200f;
        int cols = Mathf.Max(1, Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 20) / w));

        for (int i = 0; i < script.SingleIngredientRecipes.Count; i++)
        {
            if (i % cols == 0) EditorGUILayout.BeginHorizontal();

            int tagCount = script.SingleIngredientRecipes[i].tag?.Count ?? 0;
            float h = 180 + (tagCount * 22);

            Rect boxRect = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.ExpandWidth(false));
            GUI.BeginGroup(boxRect);
            GUI.Box(new Rect(0, 0, w, h), GUIContent.none, "helpBox");

            GUILayout.BeginArea(new Rect(5, 5, w - 10, h - 10));
            EditorGUILayout.BeginVertical();

            // Name and remove button
            EditorGUILayout.BeginHorizontal();
            if (string.IsNullOrEmpty(script.SingleIngredientRecipes[i].name))
                script.SingleIngredientRecipes[i].name = $"Recipe {i + 1}";
            script.SingleIngredientRecipes[i].name = EditorGUILayout.TextField(script.SingleIngredientRecipes[i].name, EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("✕", GUILayout.Width(25))) {
                script.SingleIngredientRecipes.RemoveAt(i);
                tagLists.Clear();
                EditorUtility.SetDirty(script);
            };
            EditorGUILayout.EndHorizontal();

            // Sprite
            script.SingleIngredientRecipes[i].itemSprite = (Sprite)EditorGUILayout.ObjectField(
                script.SingleIngredientRecipes[i].itemSprite, typeof(Sprite), false,
                GUILayout.Width(60), GUILayout.Height(60));

            // Tags
            GetTagList(script, i).DoLayoutList();
            
            EditorGUILayout.Space(10);

            // Question preview
            var tagList = script.SingleIngredientRecipes[i].tag;
            if (tagList != null && tagList.Count > 0)
            {
                string sentence = "What if I ";
                for (int t = 0; t < tagList.Count; t++)
                {
                    sentence += $"<b>{tagList[t]}</b>";
                    sentence += (t < tagList.Count - 1) ? " then " : $" the {script.itemName}?";
                }
                EditorGUILayout.LabelField(sentence, new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true });
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            GUI.EndGroup();
            GUILayout.Space(5);

            if (i % cols == cols - 1 || i == script.SingleIngredientRecipes.Count - 1)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
        }

        if (GUILayout.Button("+ Add Recipe", GUILayout.Height(30)))
            script.SingleIngredientRecipes.Add(new SingleIngredientRecipe());

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(script);
        
        serializedObject.ApplyModifiedProperties();
    }
}