using TMPro;
using UnityEngine;

// Drag a body here to hide it. Rewards: the customer's mushroom, plus the money in their
// wallet (worth more than the meal would have paid, see CustomerType.walletMultiplier).
public class BodyDisposal : MonoBehaviour
{
    [Tooltip("Optional text that briefly shows what you got, e.g. \"+$24\".")]
    [SerializeField] private TMP_Text rewardText;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Body") return;
        Customer customer = collision.gameObject.GetComponent<Customer>();
        if (customer == null) return;

        GameManager.Instance.inventoryManager.AddFoodItem(customer.GetCustomerType().mushroom);

        int money = customer.TakeWallet();
        if (money > 0)
        {
            GameManager.Instance.currencyManager.AddCurrency(money);
        }
        if (rewardText != null)
        {
            rewardText.raycastTarget = false;
            CookingDoneness.ShowMessage(this, rewardText, $"+${money}", new Color(0.55f, 0.9f, 0.4f), 2f);
        }
        Debug.Log($"Body hidden: +{money} money from their wallet");

        Destroy(collision.gameObject);
    }
}
