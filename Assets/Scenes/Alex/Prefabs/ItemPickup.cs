using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] protected FoodItem pickupItem;
    [SerializeField] private InventoryItem foodItemUI;
    public void OnInteract()
    {
        GameManager.Instance.inventoryManager.AddFoodItem(pickupItem);
        Destroy(this.gameObject);
    }


    //later make to on click
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnInteract();
        }
    }

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = pickupItem.defaultSprite;
    }
}