using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private int lastPlayed = -1;
    private AudioClip currentTrack;

    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private AudioClip[] sceneMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneMusic(scene.buildIndex);
    }

    //Changes tracks depending on scene number
    public void SceneMusic(int index)
    {
        if (index < sceneMusic.Length && sceneMusic[index] != null && currentTrack != sceneMusic[index])
        { 
            currentTrack = sceneMusic[index];
            if (lastPlayed == -1)
                {
                    lastPlayed = 1;
                    musicSource1.clip = sceneMusic[index];
                    musicSource1.volume = 1;
                    musicSource1.Play();
                }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FadeTrack(index));
            }
        }
    }

    private IEnumerator FadeTrack(int index)
    {
        float timeToFade = 1.25f;
        float timeElapsed = 0;

            if (lastPlayed == 1)
            {
                lastPlayed = 2;
                musicSource2.clip = sceneMusic[index];
                musicSource2.Play();

                while (timeElapsed < timeToFade)
                {
                    musicSource2.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                    musicSource1.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                musicSource1.Stop();
                musicSource1.volume = 0;
                musicSource2.volume = 1;
            }
            else
            {
                lastPlayed = 1;
                musicSource1.clip = sceneMusic[index];
                musicSource1.Play();

                while (timeElapsed < timeToFade)
                {
                    musicSource1.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                    musicSource2.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                musicSource2.Stop();
                musicSource2.volume = 0;
                musicSource1.volume = 1;
            }
        }
    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
