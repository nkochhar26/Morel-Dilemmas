using System;
using UnityEngine;
using System.Collections.Generic;

public enum OrderResult
{
    Success,
    Poisoned,
    Invalid
}

public class OrderManager : MonoBehaviour
{
    public List<FoodItem> dailyDishes;
    public FoodItem[] currentOrders = new FoodItem[5]; // hardcoded at 5 rn - can be changed if needed

    public event Action OnHeldOrderChanged;
    public event Action OnOrdersChanged;
    //private int heldOrderIndex;
    private FoodItemObject heldDish;
    public bool isPoisonous;

    public void Start()
    {
        //heldOrderIndex = -1;
        heldDish = null;
    }

    public void AddOrder(int tableNum, FoodItem recipe)
    {
        currentOrders[tableNum] = recipe;
        OnOrdersChanged?.Invoke();
    }

    public void RemoveOrder(int tableNum)
    {
        currentOrders[tableNum] = null;
        OnOrdersChanged?.Invoke();
    }

    public void SetHeldOrder(FoodItemObject dish)
    {
        heldDish = dish;
        OnHeldOrderChanged?.Invoke();

    }

    public void RemoveHeldOrder()
    {
        heldDish = null;
        OnHeldOrderChanged?.Invoke();
    }

    public FoodItem GetRecipe(int tableNum)
    {
        return currentOrders[tableNum];
    }

    public FoodItemObject GetHeldOrder()
    {
        return heldDish;
    }

    //public FoodItemObject GetHeldOrder()
    //{
    //    return heldDish;
    //}

    public FoodItem[] GetCurrentOrders()
    {
        return currentOrders;
    }


    public void ClearOrder(int tableNum)
    {
        RemoveOrder(tableNum);
        RemoveHeldOrder();
        currentOrders[tableNum] = null;
        GameManager.Instance.customerManager.RemoveCustomer(tableNum);
    }


    // when you take too long and the customer leaves, called in customer
    public void OrderTooLong(int tableNum)
    {
        // free the table, but keep the dish the player is carrying (it may be for someone else)
        RemoveOrder(tableNum);
        GameManager.Instance.customerManager.RemoveCustomer(tableNum);
        CustomerGaveUp();
    }

    // rating hit for a customer leaving unserved
    public void CustomerGaveUp()
    {
        GameManager.Instance.starManager.DecreaseStarValue(0.25f);  //TODO: figure out thsi part
        ReviewManager.Instance.HandleCustomerRemoved(1f);  //TODO: change to special 'took too long' review
    }

    //returns if the customer dies
    public OrderResult OrderDelivery(int tableNum)
    {
        if (heldDish == null || currentOrders[tableNum] == null)
        {
            return OrderResult.Invalid;
        }
        //if (currentOrders[tableNum].itemName == currentOrders[heldOrderIndex].itemName)   // so that if they order similar dishes can be interchangeable
        if (currentOrders[tableNum].itemName == heldDish.foodItem.itemName)   // so that if they order similar dishes can be interchangeable
        {
            // Dish quality (0-5, average of its ingredients) drives the rating:
            // 5 = +0.25 restaurant stars, 3 = no change, 1 or less = -0.25
            float quality = Mathf.Clamp(heldDish.starQuality, 0f, 5f);
            float starChange = Mathf.Lerp(-0.25f, 0.25f, Mathf.InverseLerp(1f, 5f, quality));

            int mealPrice = heldDish.foodItem.price;
            ClearOrder(tableNum);

            if (isPoisonous)
            {
                // Dead customers don't write reviews: however badly the dish was cooked,
                // it can't hurt the rating. They don't pay either; their wallet is on
                // the body, collected by hiding it in a BodyDropZone.
                isPoisonous = false;
                return OrderResult.Poisoned;
            }

            GameManager.Instance.currencyManager.AddCurrency(mealPrice);
            ReviewManager.Instance.HandleCustomerRemoved(Mathf.Clamp(Mathf.Round(quality), 1f, 5f));
            if (starChange >= 0f) GameManager.Instance.starManager.IncreaseStarValue(starChange);
            else GameManager.Instance.starManager.DecreaseStarValue(-starChange);
            return OrderResult.Success;
        }
        else
        {
            return OrderResult.Invalid;
        }
    }

    public void SetPoisonous(bool value)
    {
        isPoisonous = value;
    }

    public FoodItem SelectRandomDish()
    {
        return dailyDishes[UnityEngine.Random.Range(0, dailyDishes.Count)];
    }

    public List<FoodItem> GetDishes()
    {
        return dailyDishes;
    }
}
