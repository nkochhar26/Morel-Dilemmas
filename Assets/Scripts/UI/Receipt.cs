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
    public GameObject boilIcon;
    public GameObject chopIcon;
    public GameObject sauteeIcon;
    public GameObject ingredientIcon;

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
        if (recipe.recipe.step == CookingStep.Boil)
        {
            Instantiate(boilIcon, recipeHolder.transform);
        }
        if (recipe.recipe.step == CookingStep.Sautee)
        {
            Instantiate(sauteeIcon, recipeHolder.transform);
        }
        if (recipe.recipe.step == CookingStep.Chop)
        {
            Instantiate(chopIcon, recipeHolder.transform);
        }
        foreach (FoodItemQualifiers food in recipe.recipe.foodList)
        {
            if (food == null || food.foodItem == null) continue;
            GameObject currFoodIcon = Instantiate(ingredientIcon, recipeHolder.transform);
            currFoodIcon.GetComponent<Image>().sprite = food.foodItem.defaultSprite;

            // Steps this ingredient needs before the final step (e.g. tomato must be chopped
            // before going in the soup pot). The final step is already the big icon in front.
            if (food.tag != null)
            {
                List<CookingStep> extraSteps = food.tag.FindAll(s => s != recipe.recipe.step);
                CookingStepIcons.AddBadges((RectTransform)currFoodIcon.transform, extraSteps, 0.5f);
            }
        }
    }

    [Header("Hover preview (large copy on the right)")]
    [Tooltip("Height of the large copy as a fraction of the screen height.")]
    [Range(0.2f, 1f)] public float previewHeight = 0.7f;
    [Tooltip("Offset of the large copy from the top-right corner of the screen, in canvas pixels.")]
    public Vector2 previewOffset = new Vector2(-20f, -20f);
    [Tooltip("Outline drawn around the small ticket while it's hovered.")]
    public Color hoverOutlineColor = Color.white;

    private GameObject preview;   // the large copy, while hovered
    private bool isPreview;       // true on the large copy itself
    private Outline hoverOutline;

    // Hover: show a large copy of this ticket on the right, outline this one, and highlight its table
    public void Grow()
    {
        if (isPreview) return;
        ShowPreview();
        SetHoverOutline(true);
        if (table != null) table.SetOutline(true);
    }

    public void Shrink()
    {
        if (isPreview) return;
        HidePreview();
        SetHoverOutline(false);
        if (table != null) table.SetOutline(false);
    }

    private void ShowPreview()
    {
        HidePreview();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        Canvas root = canvas.rootCanvas;

        // copy this ticket as it is now (dish, steps and badges already filled in)
        preview = Instantiate(gameObject, root.transform);
        preview.name = name + " (Preview)";
        Receipt copy = preview.GetComponent<Receipt>();
        copy.isPreview = true;
        copy.table = null;

        // display-only: never take the mouse away from the small ticket (that would make it flicker)
        foreach (UnityEngine.EventSystems.EventTrigger t in preview.GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>(true))
        {
            t.enabled = false;
        }
        CanvasGroup group = preview.GetComponent<CanvasGroup>();
        if (group == null) group = preview.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        Outline copiedOutline = preview.GetComponent<Outline>();
        if (copiedOutline != null) copiedOutline.enabled = false;

        // pin to the top-right corner, sized to a fraction of the screen height
        RectTransform rt = (RectTransform)preview.transform;
        RectTransform rootRect = (RectTransform)root.transform;
        RectTransform mine = (RectTransform)transform;
        rt.DOKill();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = previewOffset;
        rt.localRotation = Quaternion.identity;
        float height = mine.rect.height > 0f ? mine.rect.height : 1f;
        float scale = rootRect.rect.height * previewHeight / height;
        rt.localScale = new Vector3(scale, scale, 1f);
        preview.transform.SetAsLastSibling(); // draw above the rest of the UI
    }

    private void HidePreview()
    {
        if (preview != null)
        {
            preview.transform.DOKill();
            Destroy(preview);
            preview = null;
        }
    }

    private void SetHoverOutline(bool on)
    {
        if (hoverOutline == null)
        {
            if (!on) return;
            Image paper = GetComponent<Image>();
            if (paper == null) return;
            hoverOutline = gameObject.AddComponent<Outline>();
            hoverOutline.effectDistance = new Vector2(4f, -4f);
        }
        hoverOutline.effectColor = hoverOutlineColor;
        hoverOutline.enabled = on;
    }

    private void OnDisable()
    {
        // e.g. the order was served while the mouse was over this ticket
        if (!isPreview) HidePreview();
    }

    private void OnDestroy()
    {
        transform.DOKill(); // stop any tween still running on this ticket
        if (isPreview) return;
        HidePreview();
        if (table != null)
        {
            table.SetOutline(false);
        }
    }

}
