using UnityEngine;

public class TableOutline : MonoBehaviour
{
    public GameObject outlined;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        outlined.SetActive(false);
    }

    public void SetOutline(bool val)
    {
        outlined.SetActive(val);
    }
}
