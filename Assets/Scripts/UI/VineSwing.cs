using UnityEngine;

// Gently swings a hanging UI element (e.g. a vine) around the point where it's attached.
// Add it to each vine. At start it moves the pivot to the attachment point without
// moving the vine on screen, then rocks it back and forth.
[RequireComponent(typeof(RectTransform))]
public class VineSwing : MonoBehaviour
{
    [Tooltip("Where the vine is attached, in the image's own space (0-1). (0.5, 1) = top centre.")]
    public Vector2 attachPoint = new Vector2(0.5f, 1f);

    [Tooltip("How far it swings each way, in degrees.")]
    public float swingAngle = 3f;

    [Tooltip("Seconds for one full swing (left, right and back).")]
    public float swingPeriod = 3.5f;

    [Tooltip("Start each vine at a different point in its swing so they don't move in sync.")]
    public bool randomStart = true;

    [Tooltip("Small extra wobble layered on top so the motion feels less mechanical. 0 = off.")]
    [Range(0f, 1f)] public float wobble = 0.25f;

    private RectTransform rt;
    private float baseAngle;
    private float phase;

    void Awake()
    {
        rt = (RectTransform)transform;
        MovePivotKeepingPosition(attachPoint);
        baseAngle = rt.localEulerAngles.z;
        phase = randomStart ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Update()
    {
        // unscaled, so the menu still sways if the game was left paused
        float t = Time.unscaledTime * Mathf.PI * 2f / Mathf.Max(0.1f, swingPeriod) + phase;
        float angle = Mathf.Sin(t) * swingAngle;
        angle += Mathf.Sin(t * 2.7f + 1.3f) * swingAngle * wobble * 0.35f;
        rt.localRotation = Quaternion.Euler(0f, 0f, baseAngle + angle);
    }

    // Changing a RectTransform's pivot normally shifts it on screen; compensate so it stays put
    private void MovePivotKeepingPosition(Vector2 newPivot)
    {
        Vector2 size = rt.rect.size;
        Vector2 delta = newPivot - rt.pivot;
        Vector3 shift = new Vector3(delta.x * size.x * rt.localScale.x, delta.y * size.y * rt.localScale.y, 0f);
        rt.pivot = newPivot;
        rt.localPosition += rt.localRotation * shift;
    }
}
