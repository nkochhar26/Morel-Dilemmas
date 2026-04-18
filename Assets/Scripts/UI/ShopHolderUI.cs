using UnityEngine;

public class ShopHolderUI : MonoBehaviour
{
    public GameObject shopEntrance;
    public ShopUI ingredientsShop;
    public ShopUI specialWares;

    public void OpenShop()
    {
        shopEntrance.SetActive(true);
    }

    public void CloseShop()
    {
        shopEntrance.SetActive(false);
        MusicManager.instance.SceneMusic(2);
    }

    public void OpenIngredientsShop()
    {
        ingredientsShop.OpenShop();
    }

    public void OpenSpecialWares()
    {
        specialWares.OpenShop();
    }
}
