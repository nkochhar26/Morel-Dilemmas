using UnityEngine;

public class Cooking : MonoBehaviour
{
    //temp just for demo and debug probably
    public void MakeFirstDish()
    {
        GameManager.Instance.orderManager.SetHeldOrder(0);
    }
    public void MakeSecondDish()
    {
        GameManager.Instance.orderManager.SetHeldOrder(1);
    }
    public void MakeThirdDish()
    {
        GameManager.Instance.orderManager.SetHeldOrder(2);
    }
    public void MakeFourthDish()
    {
        GameManager.Instance.orderManager.SetHeldOrder(3);
    }
    public void MakeFifthDish()
    {
        GameManager.Instance.orderManager.SetHeldOrder(4);
    }
}
