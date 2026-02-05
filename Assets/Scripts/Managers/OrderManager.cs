using UnityEngine;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public List<FoodItem> dailyDishes;
    public FoodItem[] currentOrders = new FoodItem[5]; // hardcoded at 5 rn - can be changed if needed
    public int heldOrderIndex;

    public void Start()
    {
        heldOrderIndex = -1;
    }

    public void AddOrder(int tableNum, FoodItem recipe)
    {
        currentOrders[tableNum] = recipe;
    }

    public void RemoveOrder(int tableNum)
    {
        currentOrders[tableNum] = null;
    }

    public void SetHeldOrder(int heldOrderIndex)
    {
        this.heldOrderIndex = heldOrderIndex;
    }

    public void RemoveHeldOrlder()
    {
        heldOrderIndex = -1;
    }

    public FoodItem GetRecipe(int tableNum)
    {
        return currentOrders[tableNum];
    }

    // public int GetHeldOrderIndex()
    // {
    //     return heldOrderIndex;
    // }

    // public FoodItem GetHeldOrder()
    // {
    //     return currentOrders[heldOrderIndex];
    // }

    public bool OrderDelivery(int tableNum)
    {
        if (heldOrderIndex == -1 || currentOrders[tableNum] == null || currentOrders[heldOrderIndex] == null)
        {
            return false;
        }
        if (currentOrders[tableNum].itemName == currentOrders[heldOrderIndex].itemName)   // so that if they order similar dishes can be interchangeable
        {
            heldOrderIndex = -1;
            currentOrders[tableNum] = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    public FoodItem SelectRandomDish()
    {
        return dailyDishes[Random.Range(0, dailyDishes.Count)];
    }
}
