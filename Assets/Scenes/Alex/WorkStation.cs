using UnityEngine;

public class WorkStation : MonoBehaviour
{
    public GameObject UIElement;
    public Transform inventoryTransform;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StopAllCoroutines();
            AlexKitchenInventoryUI inventory = AlexKitchenInventoryUI.Instance.GetComponent<AlexKitchenInventoryUI>();
            UIElement.SetActive(true);
            StartCoroutine(inventory.setTransform(inventoryTransform.localPosition, inventoryTransform.transform.localScale));
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StopAllCoroutines();
            AlexKitchenInventoryUI inventory = AlexKitchenInventoryUI.Instance.GetComponent<AlexKitchenInventoryUI>();
            StartCoroutine(inventory.setTransform(inventory.originalPosition, inventory.originalSize));
            UIElement.SetActive(false);
        }
    }
}
