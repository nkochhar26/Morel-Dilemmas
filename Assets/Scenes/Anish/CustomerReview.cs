using UnityEngine;

[CreateAssetMenu(fileName = "CustomerReview", menuName = "Scriptable Objects/CustomerReview")]
public class CustomerReview : ScriptableObject
{
    [Range(0f, 5f)]
    public float starsGiven;
    public string writtenReview;
}
