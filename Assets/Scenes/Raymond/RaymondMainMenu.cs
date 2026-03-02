using UnityEngine;
using UnityEngine.SceneManagement;

public class RaymondMainMenu : MonoBehaviour
{
    [SerializeField] private string mainSceneName;
    public void StartGame()
    {
        SoundManager.PlaySound(SoundType.UI, 0);
        SceneManager.LoadScene(mainSceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
