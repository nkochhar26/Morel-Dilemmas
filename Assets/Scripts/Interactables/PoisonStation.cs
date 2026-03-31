using UnityEngine;

public class PoisonStation : MonoBehaviour, IInteractable
{
    public void OnInteract(GameObject player)
    {
        GameManager.Instance.orderManager.SetPoisonous(true);
    }
}
