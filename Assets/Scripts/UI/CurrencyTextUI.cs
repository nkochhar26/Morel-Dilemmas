using UnityEngine;
using TMPro;

public class CurrencyTextUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI CurrencyUIText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeCurrencyText();
        GameManager.Instance.currencyManager.OnCurrencyChanged += ChangeCurrencyText;
    }

    private void ChangeCurrencyText()
    {
        CurrencyUIText.text = "Currency: " + GameManager.Instance.currencyManager.GetCurrentCurrency();
    }

    public void OnDestroy()
    {
        GameManager.Instance.currencyManager.OnCurrencyChanged -= ChangeCurrencyText;
    }
}
