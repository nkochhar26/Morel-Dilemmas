using UnityEngine;

public class ControlsPopup : MonoBehaviour
{
    [SerializeField] private GameObject popup;

    private void OnEnable()
    {
        OpenPopup();
    }

    public void ClosePopup()
    {
        popup.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenPopup()
    {
        popup.SetActive(true);
        // Freeze gameplay (Time.deltaTime becomes 0) while the tutorial is showing
        Time.timeScale = 0f;
    }
}
