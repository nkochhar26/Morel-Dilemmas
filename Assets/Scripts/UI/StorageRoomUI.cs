using UnityEngine;
using UnityEngine.UI;

public class StorageRoomUI : MonoBehaviour
{
    public Button forageButton;

    void OnEnable()
    {
        Debug.Log("Loading storage room");
        Debug.Log(GameManager.Instance.dayManager.GetHasForaged());
        if (GameManager.Instance.dayManager.GetHasForaged())
        {
            forageButton.interactable = false;
        }
    }

}
