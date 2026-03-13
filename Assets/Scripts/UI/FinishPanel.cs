using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class FinishPanel : MonoBehaviour
{
    public GameObject panel;
    public Button finishDayButton;

    private void Start()
    {
        panel.SetActive(false);
        finishDayButton.onClick.AddListener(OnFinishClicked);
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    private void OnFinishClicked()
    {
        GameManager.Instance.dayManager.FinishDay();
        SceneManager.LoadScene("Forage Area");
        Time.timeScale = 1f;
        PauseMenu.GameIsPaused = false;
    }
}
