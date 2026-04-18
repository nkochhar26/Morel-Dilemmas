using UnityEngine;

public class Table : MonoBehaviour, IInteractable
{
    public int tableNum;
    private Customer customer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnInteract(GameObject player)
    {
                //check tablefull
        if (GameManager.Instance.customerManager.CheckTableFull(tableNum))
        {
            customer.OnInteract(player);
            return; //can make this trigger customer stuff??
        }

        //check if following
        FollowTed followInfo = player.GetComponent<FollowTed>();
        if (followInfo.GetIsGuiding())
        {
            // set customer to this table
            Customer customer = followInfo.GetCurrentFollow().GetComponent<Customer>();
            followInfo.StopFollow();
            SetCustomerToTable(customer);
        }
    }

    private void SetCustomerToTable(Customer customer)
    {
        GameManager.Instance.customerManager.AddCustomer(customer.GetCustomerType(), tableNum);
        customer.SetTableNum(tableNum);

        customer.gameObject.transform.position = this.gameObject.transform.position + new Vector3(0, 1f, 0);
        GameManager.Instance.customerManager.AddCustomer(customer.GetCustomerType(), tableNum);

        CustomerSpawner.Instance.SetIsOccupied(false);
        this.customer = customer;
    }
}
