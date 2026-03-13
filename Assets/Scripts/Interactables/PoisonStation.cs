using UnityEngine;

public class PoisonStation : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        GameManager.Instance.orderManager.SetPoisonous(true);
    }
}
