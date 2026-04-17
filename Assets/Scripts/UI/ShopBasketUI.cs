using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopBasketUI : MonoBehaviour
{
    public FoodItem item;
    [SerializeField] public Image image;
    [SerializeField] public TextMeshProUGUI price;

    public void LoadItem(FoodItem item)
    {
        this.item = item;
        image.sprite = item.defaultSprite;
        price.text = item.price.ToString();
    }

    // returns false if cant buy and you're poor
    public void PurchaseItem()
    {
        if (GameManager.Instance.currencyManager.GetCurrentCurrency() >= item.price)
        {
            Debug.Log("PURCHASING");
            GameManager.Instance.currencyManager.DecreaseCurrency(item.price);
            GameManager.Instance.inventoryManager.AddFoodItem(item);
        }
        //TODO: add a popup or granny celia says you have no money
    }
}
