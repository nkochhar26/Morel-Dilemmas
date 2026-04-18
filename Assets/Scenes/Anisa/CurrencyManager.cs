using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public event Action OnCurrencyChanged;
    public int currency;

    private void Awake()
    {
        currency = 100;   // starting currency hardcoded here atm
    }

    public void AddCurrency(int amount)
    {
        Debug.Log("adding currency");
        currency += amount;
        OnCurrencyChanged?.Invoke();
    }

    public void DecreaseCurrency(int amount)
    {
        currency -= amount;
        if (currency < 0)
        {
            currency = 0;
        }
        OnCurrencyChanged?.Invoke();
    }

    public int GetCurrentCurrency()
    {
        Debug.Log(currency);
        return currency;
    }
}
