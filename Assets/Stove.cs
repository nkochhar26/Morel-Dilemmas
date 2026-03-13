using UnityEngine;

[RequireComponent(typeof(WorkStation))]
public class Stove : MonoBehaviour
{
    WorkStation workstation;
    public GameObject button;

    bool lastTouchingState;

    void Start()
    {
        workstation = GetComponent<WorkStation>();

        lastTouchingState = workstation.touching;
        button.SetActive(lastTouchingState);
    }

    void Update()
    {
        if (workstation.touching != lastTouchingState)
        {
            button.SetActive(workstation.touching);
            lastTouchingState = workstation.touching;
        }
    }
}