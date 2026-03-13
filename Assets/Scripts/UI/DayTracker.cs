using UnityEngine;
using TMPro;

public class DayTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateText(GameManager.Instance.dayManager.GetDay());
    }

    public void UpdateText(int day)
    {
        dayText.text = "Day " + day;
    }
}
