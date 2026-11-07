using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum CookResult
{
    Raw,          // taken out almost immediately: not cooked at all
    Undercooked,
    Perfect,
    Overcooked,
    Burnt,        // left in until the timer ran out
}

// Timing window for a timed station (boiler, sautee pan).
// "Doneness" is how far the timer is, 0..1 (same value as the slider).
[Serializable]
public class DonenessWindow
{
    [Header("Zones (fraction of the cook time)")]
    [Tooltip("Taken out before this: returned raw, no penalty (acts as a cancel).")]
    [Range(0, 1)] public float rawUntil = 0.35f;
    [Tooltip("Green zone start. Inside the green zone there is no penalty.")]
    [Range(0, 1)] public float perfectStart = 0.65f;
    [Tooltip("Green zone end.")]
    [Range(0, 1)] public float perfectEnd = 0.81f;

    [Header("Star penalties per ingredient (ingredients start at 5)")]
    public float undercookPenaltyMax = 2f;    // right after rawUntil
    public float undercookPenaltyMin = 0.5f;  // just before green
    public float overcookPenaltyMin = 0.5f;   // just after green
    public float overcookPenaltyMax = 2.5f;   // just before burning
    public float burntPenalty = 3f;

    [Header("Slider")]
    [Tooltip("Recolor the timer slider so its green zone always matches the window above.")]
    public bool colorSlider = true;

    public CookResult Evaluate(float doneness, out float penalty)
    {
        if (doneness >= 1f)
        {
            penalty = burntPenalty;
            return CookResult.Burnt;
        }
        if (doneness < rawUntil)
        {
            penalty = 0f;
            return CookResult.Raw;
        }
        if (doneness < perfectStart)
        {
            float t = Mathf.InverseLerp(rawUntil, perfectStart, doneness);
            penalty = Mathf.Lerp(undercookPenaltyMax, undercookPenaltyMin, t);
            return CookResult.Undercooked;
        }
        if (doneness <= perfectEnd)
        {
            penalty = 0f;
            return CookResult.Perfect;
        }
        float o = Mathf.InverseLerp(perfectEnd, 1f, doneness);
        penalty = Mathf.Lerp(overcookPenaltyMin, overcookPenaltyMax, o);
        return CookResult.Overcooked;
    }

    // red -> yellow while undercooked, solid green window, yellow -> dark red while overcooking
    public Gradient BuildGradient()
    {
        Color red = new Color(0.95f, 0.1f, 0.1f);
        Color yellow = new Color(1f, 0.85f, 0f);
        Color green = new Color(0.1f, 1f, 0.12f);
        Color burnt = new Color(0.35f, 0.05f, 0.05f);
        const float edge = 0.005f;

        Gradient g = new Gradient();
        g.mode = GradientMode.Blend;
        g.SetKeys(
            new[]
            {
                new GradientColorKey(red, 0f),
                new GradientColorKey(red, rawUntil),
                new GradientColorKey(yellow, Mathf.Max(rawUntil, perfectStart - edge)),
                new GradientColorKey(green, perfectStart),
                new GradientColorKey(green, perfectEnd),
                new GradientColorKey(yellow, Mathf.Min(1f, perfectEnd + edge)),
                new GradientColorKey(red, Mathf.Lerp(perfectEnd, 1f, 0.6f)),
                new GradientColorKey(burnt, 1f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }
}

public static class CookingDoneness
{
    // Applies the timing result to one ingredient. Returns what happened.
    // Raw ingredients are left untouched; everything else gets the step tag and loses quality.
    public static CookResult Cook(FoodItemObject food, CookingStep step, float doneness, DonenessWindow window)
    {
        CookResult result = window.Evaluate(doneness, out float penalty);
        if (food == null || result == CookResult.Raw) return result;

        FoodManager.Instance.ApplyCookingStep(food, step);
        // round to half stars so items cooked about equally well still stack in the inventory
        food.starQuality = Mathf.Max(0f, Mathf.Round((food.starQuality - penalty) * 2f) / 2f);
        return result;
    }

    // Colors the slider's fill from the doneness window whenever its value changes.
    // Works on any slider, with or without a GradientSlider wired up in the scene.
    public static void ApplySliderColors(UnityEngine.UI.Slider slider, DonenessWindow window)
    {
        if (slider == null || window == null || !window.colorSlider) return;

        Gradient gradient = window.BuildGradient();

        // keep an existing GradientSlider in agreement, so it doesn't paint different colors
        GradientSlider gs = slider.GetComponentInChildren<GradientSlider>(true);
        if (gs == null) gs = slider.GetComponentInParent<GradientSlider>();
        if (gs != null) gs.SetGradient(gradient);

        UnityEngine.UI.Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<UnityEngine.UI.Image>() : null;
        if (fill == null) return;

        fill.color = gradient.Evaluate(slider.value);
        slider.onValueChanged.AddListener(v => fill.color = gradient.Evaluate(v));
    }

    public static string Label(CookResult r)
    {
        switch (r)
        {
            case CookResult.Perfect: return "Perfect!";
            case CookResult.Undercooked: return "Undercooked";
            case CookResult.Overcooked: return "Overcooked";
            case CookResult.Burnt: return "Burnt!";
            default: return "";
        }
    }

    public static Color LabelColor(CookResult r)
    {
        switch (r)
        {
            case CookResult.Perfect: return new Color(0.2f, 0.9f, 0.2f);
            case CookResult.Undercooked: return new Color(1f, 0.85f, 0.2f);
            case CookResult.Overcooked: return new Color(1f, 0.5f, 0.1f);
            case CookResult.Burnt: return new Color(0.9f, 0.15f, 0.1f);
            default: return Color.white;
        }
    }

    // Optional on-screen feedback ("Perfect!", "Burnt!") if the station has a text assigned
    public static void ShowResult(MonoBehaviour host, TMP_Text text, CookResult r, float seconds = 1.5f)
    {
        ShowMessage(host, text, Label(r), LabelColor(r), seconds);
    }

    public static void ShowMessage(MonoBehaviour host, TMP_Text text, string message, Color color, float seconds = 1.5f)
    {
        if (string.IsNullOrEmpty(message)) return;
        Debug.Log($"{host.name}: {message}");
        if (text == null) return;
        host.StartCoroutine(ShowRoutine(text, message, color, seconds));
    }

    private static IEnumerator ShowRoutine(TMP_Text text, string label, Color color, float seconds)
    {
        text.text = label;
        text.color = color;
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        if (text != null && text.text == label) text.gameObject.SetActive(false);
    }
}
