using UnityEngine;

public class CursorOnHover : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ToPointer()
    {
        CursorManager.changeTo(CursorType.POINTER);
    }
    public void ToArrow()
    {
        CursorManager.changeTo(CursorType.ARROW);
    }

    public void toHand(){
        CursorManager.changeTo(CursorType.HAND_OPEN);
    }

    public void toGrab(){
        CursorManager.changeTo(CursorType.HAND_GRAB);
    }
}
