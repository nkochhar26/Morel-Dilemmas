
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ChalkboardSlide : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private Vector2 shownPosition;
    [SerializeField] private float slideDuration = 0.35f;

    private RectTransform rectTransform;
    private Coroutine slideCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = hiddenPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SlideTo(shownPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SlideTo(hiddenPosition);
    }

    private void SlideTo(Vector2 target)
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(AnimateSlide(target));
    }

    private IEnumerator AnimateSlide(Vector2 target)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);

            // Smooth easing
            t = t * t * (3f - 2f * t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(start, target, t);

            yield return null;
        }

        rectTransform.anchoredPosition = target;
        slideCoroutine = null;
    }
}
