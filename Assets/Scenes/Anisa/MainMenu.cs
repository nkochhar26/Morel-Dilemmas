using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string mainSceneName;
    public void StartGame()
    {
        SceneManager.LoadScene(mainSceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
