using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class DragFoodInto : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    public void AddItem(InventoryItem item)
    {
        Vector3 itemPosition = item.transform.position;
        item.transform.SetParent(transform, true);
        item.transform.position = itemPosition;
        item.transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y, -1);
        item.transform.localScale = new Vector3(item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.y);

        item.meshRenderer.gameObject.SetActive(true);
    }

    public void Remove(InventoryItem item)
    {
        item.meshRenderer.gameObject.SetActive(false);
    }
}
