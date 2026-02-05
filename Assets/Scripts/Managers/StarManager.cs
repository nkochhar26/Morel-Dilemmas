using UnityEngine;

public class StarManager : MonoBehaviour
{
    public float starValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        starValue = 1;
    }

    public void IncreaseStarValue(float value)
    {
        starValue += value;
    }

    public void DecreaseStarValue(float value)
    {
        starValue -= value;
        if (starValue < 0)
        {
            starValue = 0;
        }
    }
}
