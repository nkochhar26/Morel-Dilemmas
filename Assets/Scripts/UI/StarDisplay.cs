using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Slider> stars = new List<Slider>();

    [Header("Optional: a different drawing per star (in order, left to right)")]
    [Tooltip("e.g. Stars Filling_0 ... Stars Filling_4. Leave empty to use the prefab's sprite on every star.")]
    public List<Sprite> fillSprites = new List<Sprite>();
    [Tooltip("e.g. Stars Outline_0 ... Stars Outline_4. Leave empty to use the prefab's sprite on every star.")]
    public List<Sprite> outlineSprites = new List<Sprite>();

    private void Start()
    {
        ApplyStarSprites();
        GameManager.Instance.starManager.OnStarValueChanged += LoadStars;
        LoadStars();
    }

    private void ApplyStarSprites()
    {
        for (int i = 0; i < stars.Count; i++)
        {
            Slider star = stars[i];
            if (star == null) continue;
            star.interactable = false; // the rating is display-only: no click-and-drag

            if (i < fillSprites.Count && fillSprites[i] != null && star.fillRect != null)
            {
                Image fill = star.fillRect.GetComponent<Image>();
                if (fill != null) fill.sprite = fillSprites[i];
            }
            if (i < outlineSprites.Count && outlineSprites[i] != null)
            {
                Transform bg = star.transform.Find("Background");
                Image outline = bg != null ? bg.GetComponent<Image>() : null;
                if (outline != null) outline.sprite = outlineSprites[i];
            }
        }
    }
    void LoadStars()
    {
        LoadStars(GameManager.Instance.starManager.GetStarValue());
    }

    //TODO: Probably unncessary later on when stars are day based but temporary testing
    private void LoadStars(float starValue)
    {
        foreach (Slider star in stars)
        {
            star.value = 0;
        }
        int index = 0;
        while (starValue > 0)
        {
            if (starValue >= 1)
            {
                starValue -= 1;
                stars[index].value = 1;
            }
            else
            {
                stars[index].value = starValue;
                starValue = 0;
            }

            index += 1;
        }
    }
}
