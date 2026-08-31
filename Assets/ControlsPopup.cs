using UnityEngine;

public class ControlsPopup : MonoBehaviour
{
    [SerializeField] private GameObject popup;

    private void OnEnable()
    {
        popup.SetActive(true);
    }

    public void ClosePopup()
    {
        popup.SetActive(false);
    }

    public void OpenPopup()
    {
        popup.SetActive(true);
    }
}