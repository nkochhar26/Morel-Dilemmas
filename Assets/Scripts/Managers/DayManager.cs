using UnityEngine;
public class DayManager : MonoBehaviour
{
    public int day;
    public bool hasForaged;

    public void Start()
    {
        day = 1;
    }


    public void IncrementDay()
    {
        hasForaged = false;
        day += 1;
    }

    public void FinishDay()
    {
        IncrementDay();
    }

    public int GetDay()
    {
        return day;
    }

    public void SetHasForaged()
    {
        hasForaged = true;
    }

    public bool GetHasForaged()
    {
        return hasForaged;
    }
}
