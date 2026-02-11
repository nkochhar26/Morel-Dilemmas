using System;
using UnityEngine;

public class ScaleAnimator : MonoBehaviour
{
    public Animator animator;
    public AlexTopDownMovement movement;
    public float baseScale = .2f;
    public Vector2 scaleBetweenMult = new Vector2(0.8f, 1.2f);
    public bool x, y, z;
    public float speed;
    public float velocityStopScale = 1;
    public float lerpMult = 10;
    void Update()
    {
        if (movement.dir.magnitude == 0)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * baseScale, Time.deltaTime * lerpMult);
        }
        else
        {
            float scaleTarget = Mathf.Lerp(
                scaleBetweenMult.x * baseScale, 
                scaleBetweenMult.y * baseScale, 
                MathF.Sin(animator.GetCurrentAnimatorStateInfo(0). normalizedTime * 2 * Mathf.PI * speed) * 0.5f + 0.5f
            );
            scaleTarget += (1- 1/movement.dir.magnitude) * velocityStopScale;
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(x ? scaleTarget : baseScale, y ? scaleTarget : baseScale, z ? scaleTarget : baseScale), Time.deltaTime * lerpMult);
        }
    }
}
