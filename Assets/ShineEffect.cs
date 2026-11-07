
using System.Collections;
using UnityEngine;

public class ShineEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer shineRenderer;
    [SerializeField] private float interval = 3f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float maxScale = 1.3f;

    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = shineRenderer.transform.localScale;
        shineRenderer.enabled = false;
    }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            shineRenderer.enabled = true;
            float elapsed = 0f;
            Color color = shineRenderer.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = Mathf.Sin(t * Mathf.PI);

                color.a = pulse;
                shineRenderer.color = color;

                shineRenderer.transform.localScale =
                    initialScale * Mathf.Lerp(1f, maxScale, pulse);

                yield return null;
            }

            shineRenderer.enabled = false;
            shineRenderer.transform.localScale = initialScale;
        }
    }
}
