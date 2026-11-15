using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class AlexKitchenInventoryUI : MonoBehaviour
{
    public GameObject foodItemUI;
    public Vector2 originalPosition, originalSize;
    public static AlexKitchenInventoryUI Instance;
    public InventoryItem draggedItem;
    public GameObject itemPanel;

    // ---------------------------------------------------------------------------------
    // Temporary usability fixes until proper inventory art/UI arrives:
    //  - faster mouse-wheel scrolling
    //  - items sorted so the same kinds sit together
    //  - filter tabs (All / Mushrooms / Raw / Prepped), drawn as plain code-made boxes
    // ---------------------------------------------------------------------------------
    public enum Filter { All, Mushrooms, Raw, Prepped }

    [Header("Temporary inventory helpers")]
    [Tooltip("Mouse-wheel scroll speed for the item list (the ScrollRect default of 1 is very slow).")]
    public float scrollSensitivity = 25f;
    [Tooltip("Sort items so mushrooms, raw ingredients and prepped items are grouped, then by name.")]
    public bool sortItems = true;
    [Tooltip("Show the filter tabs above the list.")]
    public bool showFilterTabs = true;
    [Tooltip("Optional: where to put the tabs. Leave empty to auto-create a row just above the list.")]
    public RectTransform filterBar;
    public float tabHeight = 14f;
    public float tabFontSize = 7f;
    public Color tabColor = new Color(0.25f, 0.18f, 0.12f, 0.85f);
    public Color tabSelectedColor = new Color(0.85f, 0.6f, 0.25f, 1f);

    private Filter currentFilter = Filter.All;
    private readonly Dictionary<Filter, Image> tabImages = new Dictionary<Filter, Image>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        originalPosition = this.GetComponent<Transform>().localPosition;
        originalSize = this.GetComponent<Transform>().localScale;
    }

    void OnEnable()
    {
        ScrollRect scroll = GetComponent<ScrollRect>();
        if (scroll != null) scroll.scrollSensitivity = scrollSensitivity;
        if (showFilterTabs && tabImages.Count == 0) BuildFilterTabs();
        UpdateItems();
    }

    public void UpdateItems()
    {
        foreach(Transform child in itemPanel.transform)
        {
            if (child.gameObject != itemPanel)
            {
                Destroy(child.gameObject);
            }

        }
        List<FoodItemObject> items = new List<FoodItemObject>(GameManager.Instance.inventoryManager.GetFoodItems());
        items.RemoveAll(i => i == null || i.foodItem == null || !Matches(i, currentFilter));
        if (sortItems) items.Sort(CompareItems);

        foreach (FoodItemObject item in items)
        {
            GameObject gamefoodItemUIIns = Instantiate(foodItemUI, itemPanel.transform);
            gamefoodItemUIIns.GetComponent<InventoryItem>().SetItem(item);
        }
        RefreshTabColors();
    }

    // ----- filtering and sorting -----

    private static int Category(FoodItemObject item)
    {
        bool prepped = item.isIntermediate || (item.tags != null && item.tags.Count > 0);
        if (prepped) return 2;
        if (item.foodItem is MushroomItem) return 0;
        return 1;
    }

    private static bool Matches(FoodItemObject item, Filter filter)
    {
        switch (filter)
        {
            case Filter.Mushrooms: return Category(item) == 0;
            case Filter.Raw:       return Category(item) == 1;
            case Filter.Prepped:   return Category(item) == 2;
            default:               return true;
        }
    }

    private static int CompareItems(FoodItemObject a, FoodItemObject b)
    {
        int c = Category(a).CompareTo(Category(b));
        if (c != 0) return c;
        c = string.Compare(FoodManager.FoodName(a.foodItem), FoodManager.FoodName(b.foodItem), System.StringComparison.OrdinalIgnoreCase);
        if (c != 0) return c;
        c = (a.tags != null ? a.tags.Count : 0).CompareTo(b.tags != null ? b.tags.Count : 0);
        if (c != 0) return c;
        return a.isLookalike.CompareTo(b.isLookalike);
    }

    public void SetFilter(Filter filter)
    {
        currentFilter = filter;
        UpdateItems();
        ScrollRect scroll = GetComponent<ScrollRect>();
        if (scroll != null) scroll.verticalNormalizedPosition = 1f; // back to the top
    }

    // ----- placeholder tabs (code-drawn boxes, swap for real art later) -----

    private void BuildFilterTabs()
    {
        RectTransform bar = filterBar;
        if (bar == null)
        {
            GameObject barObj = new GameObject("FilterTabs (temp)", typeof(RectTransform));
            bar = (RectTransform)barObj.transform;
            bar.SetParent(transform, false);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.anchoredPosition = new Vector2(0f, 2f);
            bar.sizeDelta = new Vector2(0f, tabHeight);
        }

        HorizontalLayoutGroup row = bar.GetComponent<HorizontalLayoutGroup>();
        if (row == null) row = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 2f;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;

        // reuse the font the item tiles already use
        TMP_FontAsset font = null;
        TMP_Text sample = foodItemUI != null ? foodItemUI.GetComponentInChildren<TMP_Text>(true) : null;
        if (sample != null) font = sample.font;

        AddTab(bar, Filter.All, "All", font);
        AddTab(bar, Filter.Mushrooms, "Shrooms", font);
        AddTab(bar, Filter.Raw, "Raw", font);
        AddTab(bar, Filter.Prepped, "Prepped", font);
        RefreshTabColors();
    }

    private void AddTab(RectTransform bar, Filter filter, string label, TMP_FontAsset font)
    {
        GameObject tab = new GameObject(label + " Tab", typeof(RectTransform), typeof(Image), typeof(Button));
        tab.transform.SetParent(bar, false);
        Image img = tab.GetComponent<Image>();
        img.color = tabColor;
        tab.GetComponent<Button>().onClick.AddListener(() => SetFilter(filter));
        tabImages[filter] = img;

        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(tab.transform, false);
        RectTransform tr = (RectTransform)textObj.transform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = label;
        text.fontSize = tabFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false; // clicks go to the tab's button
    }

    private void RefreshTabColors()
    {
        foreach (KeyValuePair<Filter, Image> pair in tabImages)
        {
            if (pair.Value != null) pair.Value.color = pair.Key == currentFilter ? tabSelectedColor : tabColor;
        }
    }

    public void Grow()
    {
        transform.DOScale(originalSize * 1.5f, 0.2f);
        CursorManager.changeTo(CursorType.POINTER);
    }

    public void Shrink()
    {
        transform.DOScale(originalSize, 0.2f);
        CursorManager.changeTo(CursorType.ARROW);
    }

    public IEnumerator setTransform(Vector2 position, Vector2 size)
    {
        float time = 1;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float normalizedTime = t / time;
            this.GetComponent<Transform>().localPosition = Vector2.Lerp(this.GetComponent<Transform>().localPosition, position, normalizedTime);
            this.GetComponent<Transform>().localScale = Vector2.Lerp(this.GetComponent<Transform>().localScale, size, normalizedTime);
            yield return null;
        }
    }
}
