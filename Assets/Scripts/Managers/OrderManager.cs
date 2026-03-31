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
    private int heldOrderIndex;
    public bool isPoisonous;

    public void Start()
    {
        heldOrderIndex = -1;
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

    public void SetHeldOrder(int heldOrderIndex)
    {
        this.heldOrderIndex = heldOrderIndex;
        OnHeldOrderChanged?.Invoke();

    }

    public void RemoveHeldOrder()
    {
        heldOrderIndex = -1;
        OnHeldOrderChanged?.Invoke();
    }

    public FoodItem GetRecipe(int tableNum)
    {
        return currentOrders[tableNum];
    }

    public int GetHeldOrderIndex()
    {
        return heldOrderIndex;
    }

    public FoodItem GetHeldOrder()
    {
        return currentOrders[heldOrderIndex];
    }

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
        ClearOrder(tableNum);
        GameManager.Instance.starManager.DecreaseStarValue(0.25f);  //TODO: figure out thsi part
        ReviewManager.Instance.HandleCustomerRemoved(1f);  //TODO: change to special 'took too long' review
    }

    //returns if the customer dies
    public OrderResult OrderDelivery(int tableNum)
    {
        if (heldOrderIndex == -1 || currentOrders[tableNum] == null || currentOrders[heldOrderIndex] == null)
        {
            return OrderResult.Invalid;
        }
        if (currentOrders[tableNum].itemName == currentOrders[heldOrderIndex].itemName)   // so that if they order similar dishes can be interchangeable
        {
            isPoisonous = false;
            ClearOrder(tableNum);
            ReviewManager.Instance.HandleCustomerRemoved();

            // TODO: Implement forumla
            GameManager.Instance.starManager.IncreaseStarValue(0.25f);
            if (isPoisonous == false)
            {
                return OrderResult.Success;
            }
            else
            {
                return OrderResult.Poisoned;
            }
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
}
