using UnityEngine;

public class MushroomPickup : ItemPickup
{
    private bool isLookalike;

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

    public void SetPickupItem(Mushroom mushroom)
    {
        pickupItem = mushroom.mushroomItem;
        isLookalike = mushroom.isLookalike;
    }

    private void OnMouseUp()
    {
        OnInteract();
    }

}
