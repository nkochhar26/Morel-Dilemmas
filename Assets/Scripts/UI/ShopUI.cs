using UnityEngine;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private List<FoodItem> itemsSold = new List<FoodItem>();
    [SerializeField] private GameObject itemHolder;
    [SerializeField] private GameObject itemBasket;
    private List<GameObject> baskets = new List<GameObject>();

    private void Start()
    {
        // CloseShop();
        LoadShopItems();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private void LoadShopItems()
    {
        foreach (GameObject basket in baskets)
        {
            Destroy(basket);
        }
        foreach (FoodItem item in itemsSold)
        {
            GameObject currItem = Instantiate(itemBasket, itemHolder.transform);
            currItem.GetComponent<ShopBasketUI>().LoadItem(item);
            baskets.Add(currItem);
        }
    }
}
