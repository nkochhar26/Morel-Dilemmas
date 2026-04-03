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

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if(Camera.main.GetComponent<CameraTransition>().zoomedWorkstation!=null) return;

        StopAllCoroutines();
        touching = true;
        var inventory = AlexKitchenInventoryUI.Instance;
        Camera.main.GetComponent<CameraTransition>().prepareMove(transform, transitionTime);
        StartCoroutine(playerFader());

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        StopAllCoroutines();
        touching = false;

        var inventory = AlexKitchenInventoryUI.Instance;
        Camera.main.GetComponent<CameraTransition>().prepareMove(null, transitionTime);
        CursorManager.changeTo(CursorType.ARROW);
        StartCoroutine(player.GetComponent<PlayerFade>().Fade(1, transitionTime));
    }

    private IEnumerator playerFader()
    {
        yield return StartCoroutine(player.GetComponent<PlayerFade>().Fade(0, transitionTime));
        CursorManager.changeTo(CursorType.KNIFE_KITCHEN);
    }
}
