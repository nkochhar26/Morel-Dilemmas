using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner Instance { get; private set; }
    // private List<GameObject> possibleCustomers = new List<GameObject>();
    // public GameObject[] tables = new GameObject[5]; // also hardcoded as 5 atm 
    public GameObject[] spawnedCustomers = new GameObject[5]; //hardcoded as 5 atm
    public float timeInBetween;  // in second
    public float time;
    public GameObject finishDayPanel;
    private bool isOccupied;  //if someone is waiting to be seated - true if there is

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        finishDayPanel.SetActive(false);
        GameManager.Instance.customerManager.NewDay();
        // LoadData();
        time = timeInBetween - 1;
    }
    
    private void Update()
    {
        time += Time.deltaTime;
        if (time > timeInBetween)
        {
            SpawnCustomer();
            time = 0;
        }
    }

    //randomized, updates gameobject and customerManager
    // public void SpawnCustomer()
    // {
    //     if (GameManager.Instance.customerManager.IsMaxDayCustomer())
    //     {
    //         return;
    //     }
    //     List<int> openTables = GameManager.Instance.customerManager.GetFreeTables();
    //     if (openTables.Count == 0)
    //     {
    //         return;
    //     }
    //     int tableIndex = Random.Range(0, openTables.Count);
    //     List<GameObject> possibleCustomers = GameManager.Instance.customerManager.GetPossibleCustomers();
    //     int customerIndex = Random.Range(0, possibleCustomers.Count);

    //     LoadCustomer(possibleCustomers[customerIndex].GetComponent<Customer>().GetCustomerType(), openTables[tableIndex], false);
    // }

    public void SpawnCustomer()
    {
        if (isOccupied)
        {
            return;
        }
        if (GameManager.Instance.customerManager.IsMaxDayCustomer())
        {
            return;
        }
        List<int> openTables = GameManager.Instance.customerManager.GetFreeTables();
        if (openTables.Count == 0)
        {
            return;
        }
        List<GameObject> possibleCustomers = GameManager.Instance.customerManager.GetPossibleCustomers();
        int customerIndex = Random.Range(0, possibleCustomers.Count);
        GameManager.Instance.customerManager.IncrementDayCustomer();
        CustomerType customerType = possibleCustomers[customerIndex].GetComponent<Customer>().GetCustomerType();
        GameObject toSpawnCustomer = GameManager.Instance.customerManager.GetGameObjectFromCustomerType(customerType);
        GameObject spawnedCustomer = Instantiate(toSpawnCustomer, this.gameObject.transform.position, Quaternion.identity);

        spawnedCustomer.GetComponent<Customer>().SetState(CustomerState.IsWaiting);
        GameManager.Instance.customerManager.SetWaitingCustomer(customerType);
        isOccupied = true;
    }

    public void SetIsOccupied(bool value)
    {
        isOccupied = value;
    }
}