using UnityEngine;

public class Cooking : MonoBehaviour
{
    //temp just for demo and debug probably
    public void MakeFirstDish()
    {
        GameManager.Instance.orderManager.SetHeldOrder(0);
    }
}
