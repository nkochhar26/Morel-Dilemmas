using UnityEngine;
using UnityEngine.SceneManagement;


public class ForagingSceneSwap : SceneSwap
{

    public override void SwapScene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveForagingProgress();
        }

        SceneManager.LoadScene(nextScene);
        GameManager.Instance.dayManager.SetHasForaged();
        Time.timeScale = 1f;
        PauseMenu.GameIsPaused = false;
    }   
}
