using UnityEngine;

public class BodyDisposal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Body")
        {
            GameManager.Instance.inventoryManager.AddFoodItem(collision.gameObject.GetComponent<Customer>().GetCustomerType().mushroom);
            Destroy(collision.gameObject);
        }
    }
}
