using UnityEngine;
using UnityEngine.UI;

public class StorageRoomUI : MonoBehaviour
{
    public Button forageButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance.dayManager.GetHasForaged())
        {
            forageButton.interactable = false;
        }
    }

}
