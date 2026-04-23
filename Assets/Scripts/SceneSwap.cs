using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneSwap : MonoBehaviour
{
    [SerializeField] protected string nextScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collision is with the player object
        if (other.CompareTag("Player"))
        {
            SwapScene();
        }
    }

    public virtual void SwapScene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveForagingProgress();
        }

        SceneManager.LoadScene(nextScene);
        Time.timeScale = 1f;
        PauseMenu.GameIsPaused = false;
    }   
}
