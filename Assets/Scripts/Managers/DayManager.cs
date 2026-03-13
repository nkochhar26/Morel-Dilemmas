using UnityEngine;
public class DayManager : MonoBehaviour
{
    public int day;

    public void Start()
    {
        day = 1;
    }


    public void IncrementDay()
    {
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
}
