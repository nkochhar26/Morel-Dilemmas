using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Yarn.Unity;

public class CustomerDialogue : MonoBehaviour
{
    [SerializeField] private DialogueReference dialogueReference;
    [SerializeField] private DialogueRunner dialogueRunner;
    private bool isOrdering;
    private string orderNode;
    private string idleNode;
    private string seatedNode;
    private string bodyNode;
    private string servedNode;
    
    
    
    private Customer customer;
    void Awake()
    {
        customer = GetComponentInParent<Customer>();
        GetDialogueNodeNames();
        Debug.Log(customer.CustomerType.name);
    }

    public void ShowOrderDialogue()
    {
        //If we're already doing orderDialogue, just skip
        if (isOrdering) return;
        isOrdering = true;
        ShowOrderDialogueAsync().Forget();
    }
    public void ShowIdleDialogue()
    {
        ShowIdleDialogueAsync().Forget();
    }
    public void ShowSeatedDialogue()
    {
        ShowSeatedDialogueAsync().Forget();
    }
    public void ShowServedDialogue()
    {
        ShowServedDialogueAsync().Forget();
    }
    private async YarnTask ShowOrderDialogueAsync()
    {
        try
        {
            if (dialogueRunner.IsDialogueRunning)
            {
                await dialogueRunner.Stop();
            }

            await dialogueRunner.StartDialogue(orderNode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            isOrdering = false;
        }
    }
    private async YarnTask ShowIdleDialogueAsync()
    {
        try
        {
            if (dialogueRunner.IsDialogueRunning)
            {
                await dialogueRunner.Stop();
            }
            Debug.Log(idleNode);
            await dialogueRunner.StartDialogue(idleNode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    private async YarnTask ShowSeatedDialogueAsync()
    {
        try
        {
            if (dialogueRunner.IsDialogueRunning)
            {
                await dialogueRunner.Stop();
            }
            await dialogueRunner.StartDialogue(seatedNode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    private async YarnTask ShowServedDialogueAsync()
    {
        try
        {
            if (dialogueRunner.IsDialogueRunning)
            {
                await dialogueRunner.Stop();
            }
            await dialogueRunner.StartDialogue(servedNode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async YarnTask HideDialogue()
    {
        if (dialogueRunner != null)
        {
            await dialogueRunner.Stop();
        }
        
        //Todo: Make sure isOrdering is set to false properly in other dialogue shows
        isOrdering = false;
    }

    private void GetDialogueNodeNames()
    {
        //Removes whitespaces and apostrophes
        var customerName  = customer.CustomerType.name.Replace(" ", "").Replace("'", "");
        orderNode = customerName+"Order";
        idleNode = customerName+"Idle";
        seatedNode = customerName+"Seating";
        bodyNode = customerName+"Body";
        servedNode = customerName+"Served";
    }
}