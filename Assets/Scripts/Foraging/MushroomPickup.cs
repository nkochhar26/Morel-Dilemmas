using UnityEngine;

public class MushroomPickup : ItemPickup
{
    private bool isLookalike;

    [Tooltip("Lookalikes grow in clusters: picking one up gives this many. Real mushrooms always give 1.")]
    [SerializeField] private int lookalikeYield = 2;

    void Start()
    {
        if (isLookalike)
        {
            GetComponent<SpriteRenderer>().sprite = ((MushroomItem) pickupItem).lookalikeSprite;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = pickupItem.defaultSprite;
        }
    }

    public override void OnInteract(GameObject player)
    {
        int quantity = isLookalike ? Mathf.Max(1, lookalikeYield) : 1;
        GameManager.Instance.inventoryManager.AddFoodItem(pickupItem, 5, quantity, isLookalike);
        Destroy(this.gameObject);
    }

    public void SetPickupItem(Mushroom mushroom)
    {
        pickupItem = mushroom.mushroomItem;
        isLookalike = mushroom.isLookalike;
    }

    private void OnMouseEnter(){
        CursorManager.changeTo(CursorType.HAND_OPEN);
    }
    private void OnMouseExit(){
        CursorManager.changeTo(CursorType.ARROW);
    }
    private void OnMouseDown(){
        CursorManager.changeTo(CursorType.HAND_GRAB);
    }
    private void OnMouseUp()
    {
        OnInteract(null);
        CursorManager.changeTo(CursorType.ARROW);
    }

}
