using UnityEngine;
using UnityEngine.UI;

public class GradientSlider : MonoBehaviour
{
    [SerializeField] Gradient gradient;
    [SerializeField] Image image;

    public void UpdateColor(float value)
    {
        image.color = gradient.Evaluate(value);
    }
}
