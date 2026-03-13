using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] TMP_Text CurrencyUIText;
    int currency;

    public void AddCurrency(int amount)
    {
        currency += amount;
        CurrencyUIText.text = "Currency: " + currency;
    }

    public void DecreaseCurrency(int amount)
    {
        currency -= amount;
        if (currency < 0)
        {
            currency = 0;
        }
        CurrencyUIText.text = "Currency: " + currency;
    }

    public int GetCurrentCurrency()
    {
        return currency;
    }
}
