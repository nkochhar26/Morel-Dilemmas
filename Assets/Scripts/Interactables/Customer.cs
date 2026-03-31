using UnityEngine;


/*states of the customer
1. Is waiting: waiting for ted to walk them to a table
2. Seated: seated by ted, order NOT taken yet
3. TakenOrder: order taken by ted
4. Dead: dead and is a body, will trigger vision cones in stealth
*/
public enum CustomerState
{
    IsWaiting,
    Seated,
    TakenOrder,
    Dead
}

public class Customer : MonoBehaviour, IInteractable
{
    private int tableNum;

    //TODO: change recipe based on day, remove serialized field
    private FoodItem orderedDish;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private CustomerType customerType;
    [SerializeField] private VisionCone visionCone;
    public CustomerState state;

    public void Start()
    {
        orderedDish = GameManager.Instance.orderManager.SelectRandomDish();
    }

    public void OnInteract(GameObject player)
    {
        if (state == CustomerState.IsWaiting)
        {
            //follow code
            player.GetComponent<FollowTed>().SetIsGuiding(this.gameObject);
        }

        //check if served
        if (state == CustomerState.TakenOrder)
        {
            OrderResult result = GameManager.Instance.orderManager.OrderDelivery(tableNum);
            if (result == OrderResult.Success)
            {
                GameManager.Instance.customerManager.OnDespawnCustomer();
                Destroy(this.gameObject);
            }
            else if (result == OrderResult.Poisoned)
            {
                GameManager.Instance.customerManager.OnDespawnCustomer();
                BecomeABody();
            }
            else
            {
                Debug.Log("This isn't the correct order or you have no currently held dishes");
            }
        }

        else if (state == CustomerState.Seated)
        {
            SoundManager.PlaySound(SoundType.NPC, 0, 1);
            GameManager.Instance.orderManager.AddOrder(tableNum, orderedDish);  
            Debug.Log("Ordered: " + orderedDish.name + " at table " + tableNum);    
            SetTakenOrder(true);
            
            /** 
                TODO: Replace following conditions with outcome (fail/success) of order and
                quality of mushrooms used in a dish
            */
            // if (Random.Range(0f, 1f) < 0.5f)
            // {
            //     Debug.Log("Success condition");
            //     GameManager.Instance.currencyManager.AddCurrency(1);
            // } else
            // {
            //     Debug.Log("Recipe Fail Condition");
            //     GameManager.Instance.currencyManager.DecreaseCurrency(1);
            // }
        }
    }

    public CustomerType GetCustomerType()
    {
        return customerType;
    }

    public void SetTakenOrder(bool value)
    {
        state = CustomerState.TakenOrder;
        GameManager.Instance.customerManager.SetTakenOrder(tableNum, value);
    }

    public void SetTableNum(int tableNum)
    {
        state = CustomerState.Seated;
        this.tableNum = tableNum;
    }

    private void BecomeABody()
    {
        state = CustomerState.Dead;
        this.gameObject.layer = 0;
        this.gameObject.tag = "Body";
        boxCollider.isTrigger = true;
        animator.enabled = false;
        Destroy(visionCone);
        spriteRenderer.color = new Color(103/255f, 192/255f, 101/255f, 1f);
    }

    public void SetState(CustomerState state)
    {
        this.state = state;
    }
}