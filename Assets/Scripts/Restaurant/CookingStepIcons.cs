using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Icons for each cooking step (knife / pot / pan), shared by the inventory and recipe tickets.
// The asset lives at Assets/Resources/CookingStepIcons.asset.
[CreateAssetMenu(fileName = "CookingStepIcons", menuName = "Cooking Step Icons")]
public class CookingStepIcons : ScriptableObject
{
    public Sprite chop;
    public Sprite boil;
    public Sprite sautee;

    private static CookingStepIcons instance;
    public static CookingStepIcons Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<CookingStepIcons>("CookingStepIcons");
            return instance;
        }
    }

    public Sprite Get(CookingStep step)
    {
        switch (step)
        {
            case CookingStep.Chop: return chop;
            case CookingStep.Boil: return boil;
            case CookingStep.Sautee: return sautee;
            default: return null;
        }
    }

    // Adds a row of small step icons in the top-left corner of `parent`.
    // sizeFraction is relative to the parent's smaller side.
    public static List<GameObject> AddBadges(RectTransform parent, IEnumerable<CookingStep> steps, float sizeFraction = 0.38f)
    {
        List<GameObject> created = new List<GameObject>();
        if (parent == null || steps == null || Instance == null) return created;

        float side = Mathf.Min(parent.rect.width, parent.rect.height);
        if (side <= 0f) side = 100f;
        float size = side * sizeFraction;

        int i = 0;
        HashSet<CookingStep> seen = new HashSet<CookingStep>();
        foreach (CookingStep step in steps)
        {
            if (!seen.Add(step)) continue;
            Sprite sprite = Instance.Get(step);
            if (sprite == null) continue;

            GameObject go = new GameObject("StepBadge_" + step, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();

            // ignore parent layout groups so badges sit on top of the icon
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(i * size * 0.85f, 0f);

            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false; // never block clicks/drags on the item

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            created.Add(go);
            i++;
        }
        return created;
    }
}
