using UnityEngine;
using System.Collections.Generic;

public class IngredientUI : MonoBehaviour
{
    public GameObject boilIcon;
    public GameObject sauteeIcon;
    public GameObject chopIcon;
    public GameObject tagHolder;
    public void SetTags(List<CookingStep> steps)
    {
        foreach(CookingStep step in steps)
        {
            if (step == CookingStep.Boil)
            {
                Instantiate(boilIcon, tagHolder.transform);
            }
            if (step == CookingStep.Sautee)
            {
                Instantiate(sauteeIcon, tagHolder.transform);
            }
            if (step == CookingStep.Chop)
            {
                Instantiate(chopIcon, tagHolder.transform);
            }
        }
    }
}
