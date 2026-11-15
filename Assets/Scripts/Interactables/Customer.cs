using UnityEngine;
using UnityEngine.UI;


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
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private CustomerType customerType;
    [SerializeField] private VisionCone visionCone;
    [SerializeField] private Slider timer;
    [SerializeField] private CustomerDialogue customerDialogue;
    [SerializeField] private Animator animator;
    private CustomerState state;
    [SerializeField] private GameObject ghost;
    private float currTimer;
    private float maxTimer = 80f;   //TODO hardcoded atm

    public CustomerType CustomerType => customerType;
    public void Start()
    {
        ghost.SetActive(false);
        orderedDish = GameManager.Instance.orderManager.SelectRandomDish();
        currTimer = 0f;
    }

    public void Update()
    {
        if (state == CustomerState.Dead)
        {
            return;
        }
        currTimer += Time.deltaTime;
        timer.value = 1f - (currTimer / maxTimer);
        if (currTimer >= maxTimer)
        {
            LeaveRestaurant();
        }
    }

    public void WalkAnimToggle(bool value)
    {
        animator.SetBool("isWalking", value);
    }

    public void OnInteract(GameObject player)
    {
        if (state == CustomerState.IsWaiting)
        {
            //follow code
            player.GetComponent<FollowTed>().SetIsGuiding(this.gameObject);
            WalkAnimToggle(true);
        }

        //check if served
        if (state == CustomerState.TakenOrder)
        {
            OrderResult result = GameManager.Instance.orderManager.OrderDelivery(tableNum);
            if (result == OrderResult.Success)
            {
                GameManager.Instance.customerManager.OnDespawnCustomer();
                SoundManager.PlaySound(SoundType.UI, 1, 0.2f);
                Destroy(this.gameObject);
            }
            else if (result == OrderResult.Poisoned)
            {
                GameManager.Instance.customerManager.OnDespawnCustomer();
                SoundManager.PlaySound(SoundType.UI, 2, 0.2f);
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
            customerDialogue.ShowOrderDialogue();
            SetTakenOrder(true);
        
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
        animator.SetBool("isDead", true);
        this.gameObject.layer = 0;
        this.gameObject.tag = "Body";
        boxCollider.isTrigger = true;
        // animator.enabled = false;
        spriteRenderer.color = new Color(103/255f, 192/255f, 101/255f, 1f);
        ghost.SetActive(true);
        Destroy(visionCone);
    }

    public bool IsDead => state == CustomerState.Dead;

    // Money on the body, taken once when it's hidden (BodyDisposal).
    // Worth more than the meal would have paid, to reward dealing with the body.
    private bool walletTaken;
    public int TakeWallet()
    {
        if (walletTaken) return 0;
        walletTaken = true;
        int meal = orderedDish != null ? orderedDish.price : 0;
        float multiplier = customerType != null ? customerType.walletMultiplier : 2f;
        int bonus = customerType != null ? customerType.walletBonus : 0;
        return Mathf.Max(0, Mathf.RoundToInt(meal * multiplier) + bonus);
    }

    public void SetState(CustomerState state)
    {
        this.state = state;
    }

    private void LeaveRestaurant()
    {
        if (state == CustomerState.IsWaiting)
        {
            // Never seated (still at the door, or being walked to a table). They have no
            // table, so don't clear one (tableNum would be 0 and wipe table 0's real customer).
            FollowTed ted = FindFirstObjectByType<FollowTed>();
            if (ted != null) ted.ReleaseIfFollowing(gameObject);
            GameManager.Instance.customerManager.SetWaitingCustomer(null);
            if (CustomerSpawner.Instance != null) CustomerSpawner.Instance.SetIsOccupied(false); // let the next customer in
            GameManager.Instance.orderManager.CustomerGaveUp();
        }
        else
        {
            //remove from customer manager
            GameManager.Instance.orderManager.OrderTooLong(tableNum);
        }
        GameManager.Instance.customerManager.OnDespawnCustomer();
        SoundManager.PlaySound(SoundType.UI, 2, 0.2f);
        Destroy(this.gameObject);
    }
}