using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    public GameObject targetObject;

    public void ToggleActive()
    {
        targetObject.SetActive(!targetObject.activeSelf);
    }
}