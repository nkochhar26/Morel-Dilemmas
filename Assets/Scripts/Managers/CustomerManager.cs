using UnityEngine;
using System.Collections.Generic;

public class CustomerInfo
{
    public int tableNum;
    public GameObject customer;
    public bool takenOrder;
    public CustomerType customerType;

    public CustomerInfo(CustomerType customerType, int tableNum, bool takenOrder)
    {
        this.customerType = customerType;
        this.tableNum = tableNum;
        this.takenOrder = takenOrder;
    }

    //TODO: Store dish order info here
}

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    public CustomerInfo[] activeCustomers =  new CustomerInfo[5];
    public List<GameObject> possibleCustomers = new List<GameObject>();  // could be stored in customer spawner as well but i didnt wanna keep reloading the info
    public Dictionary<CustomerType, GameObject> customerTypeToGameObject = new Dictionary<CustomerType, GameObject>();
    
    public void Awake()
    {
        SetupDictionary();
    }

    private void SetupDictionary()
    {
        foreach (GameObject customer in possibleCustomers)
        {
            CustomerType customerType = customer.GetComponent<Customer>().GetCustomerType();
            if (customerTypeToGameObject.ContainsKey(customerType) == false)
            {
                customerTypeToGameObject.Add(customerType, customer);
            }
            else
            {
                Debug.Log("You put a dupe in the customers. lol");
            }
        }
    }

    public GameObject GetGameObjectFromCustomerType(CustomerType customerType)
    {
        if (customerTypeToGameObject.ContainsKey(customerType))
        {
            return customerTypeToGameObject[customerType];
        }
        else
        {
            Debug.Log("CustomerType not in dictionary.");
            return null;
        }
    }

    public void AddCustomer(CustomerType customerType, int tableIndex)
    {
        activeCustomers[tableIndex] = new CustomerInfo(customerType, tableIndex, false);
    }

    public void AddCustomer(CustomerType customerType, int tableIndex, bool takenOrder)
    {
        activeCustomers[tableIndex] = new CustomerInfo(customerType, tableIndex, takenOrder);
    }

    /// <summary>
    /// The RemoveCustomer function removes a customer by taking them out from a certain table.
    /// Before removing the customer, it checks to see if there is a customer at the table.
    /// If there is, then the HandleCustomerRemoved() function will be called from the ReviewManager.
    /// Will calculate a rating for the dish once that has been implemented
    /// </summary>
    /// <param name="tableIndex"></param>
    public void RemoveCustomer(int tableIndex)
    {
        var customer = activeCustomers[tableIndex];
        if (customer != null)
        {
            ReviewManager.Instance.HandleCustomerRemoved();
        }
        activeCustomers[tableIndex] = null;
    }

    public CustomerInfo[] GetSpawnedCustomers()
    {
        return activeCustomers;
    }

    public void SetTakenOrder(int tableNum, bool value)
    {
        activeCustomers[tableNum].takenOrder = value;
    }

    public bool GetTakenOrder(int tableNum)
    {
        return activeCustomers[tableNum].takenOrder;
    }

    public List<int> GetFreeTables()
    {
        List<int> rtn = new List<int>();
        for (int i = 0; i < activeCustomers.Length; i++)
        {
            if (activeCustomers[i] == null)
            {
                rtn.Add(i);
            }
        }
        return rtn;
    }

    public List<GameObject> GetPossibleCustomers()
    {
        return possibleCustomers;
    }
}
