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
        OnInteract();
        CursorManager.changeTo(CursorType.ARROW);
    }

}
