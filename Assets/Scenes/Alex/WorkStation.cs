using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WorkStation : MonoBehaviour
{
    public float transitionTime = 1f;
    public bool touching;

    float timer;
    Transform player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        StopAllCoroutines();
        touching = true;

        var inventory = AlexKitchenInventoryUI.Instance;
        Camera.main.GetComponent<CameraTransition>().prepareMove(transform, transitionTime);

        StartCoroutine(player.GetComponent<PlayerFade>().Fade(0, transitionTime));
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        StopAllCoroutines();
        touching = false;

        var inventory = AlexKitchenInventoryUI.Instance;
        Camera.main.GetComponent<CameraTransition>().prepareMove(null, transitionTime);

        StartCoroutine(player.GetComponent<PlayerFade>().Fade(1, transitionTime));
    }
}
