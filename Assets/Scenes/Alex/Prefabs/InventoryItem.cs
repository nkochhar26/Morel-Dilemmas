using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 originalSize;
    Vector3 originalSizeWorld;
    public bool followmouse = false;
    public LayerMask targetLayer;
    public MeshRenderer meshRenderer;
    public Image image;
    public FoodItemObject foodItem;
    private Vector3 originalPosition;
    private Transform originalParent;
    public TextMeshProUGUI quantityText;
    public GameObject boilIcon;
    public GameObject sauteeIcon;
    public GameObject chopIcon;
    public GameObject tagHolder;

    void Start()
    {
        originalSize = transform.localScale;
        originalSizeWorld = transform.lossyScale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StopFollowMouse();
    }

    // public void OnPointerUp(PointerEventData eventData)
    // {
    //     StopFollowMouse();
    // }

    public void OnDrag(PointerEventData eventData)
    {
        FollowMouse();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = this.transform.position;
        originalParent = this.transform.parent;
        FollowMouse();
    }   

    public void OnPointerExit(PointerEventData eventData)
    {
        Shrink();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Grow();
    }


    public void Grow()
    {
        if (transform.parent.GetComponent<DragFoodInto>() == null) return;
        //transform.DOScale(originalSize * 1.5f, 0.2f);
    }

    public void Shrink()
    {
        // StopFollowMouse();
        if (transform.parent.GetComponent<DragFoodInto>() == null) return;
        //transform.DOScale(originalSize * 1f, 0.2f);
    }

    public void FollowMouse()
    {
        AlexKitchenInventoryUI.Instance.draggedItem = this;
        this.transform.SetParent(AlexKitchenInventoryUI.Instance.gameObject.transform);
        followmouse = true;
    }

    public void StopFollowMouse()
    {
        followmouse = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, targetLayer);
        if (hit.collider != null)
        {
            hit.transform.GetComponent<DragFoodInto>().AddItem(this);
            
            if (foodItem.quantity > 1)
            {
                foodItem.quantity -= 1;

            }
            else {
                // last item, remove
                InventoryManager.foodItems.Remove(foodItem);
            
            }
            
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = worldPosition;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 0.005f);
            AlexKitchenInventoryUI.Instance.UpdateItems();
        }
        else
        {
            if (AlexKitchenInventoryUI.Instance != null)
            {
                this.transform.position = originalPosition;
                this.transform.SetParent(originalParent);
                LayoutRebuilder.ForceRebuildLayoutImmediate(AlexKitchenInventoryUI.Instance.GetComponent<RectTransform>());
                CursorManager.changeTo(CursorType.HAND_OPEN);
            }
        }
    }

    public void SetItem(FoodItemObject item)
    {
        foodItem = item;
        quantityText.text = item.quantity.ToString();

        if (item.isLookalike)
        {
            image.sprite = ((MushroomItem)(item.foodItem)).lookalikeSprite;
            meshRenderer.material.mainTexture = ((MushroomItem)(item.foodItem)).lookalikeSprite.texture;
        }
        else
        {
            image.sprite = item.foodItem.defaultSprite;
            meshRenderer.material.mainTexture = item.foodItem.defaultSprite.texture;
        }
        meshRenderer.GetComponent<MeshFilter>().mesh = BuildMeshFromSprite(item.foodItem.defaultSprite);
        if (image != null)
        {
            image.color = item.isIntermediate ? new Color(1f, 0.92f, 0.6f, 1f) : Color.white;
        }
        LoadTags(item);
    }

    public void LoadTags(FoodItemObject item)
    {
        foreach (CookingStep tag in item.tags)
        {
            if (tag == CookingStep.Boil)
            {
                Instantiate(boilIcon, tagHolder.transform);
            }
            if (tag == CookingStep.Sautee)
            {
                Instantiate(sauteeIcon, tagHolder.transform);
            }
            if (tag == CookingStep.Chop)
            {
                Instantiate(chopIcon, tagHolder.transform);
            }
        }
    }

    void Update()
    {
        if (followmouse)
        {
            Vector3 mousePosition = Input.mousePosition;
            // mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            // Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = Input.mousePosition;
        }
    }

    Mesh BuildMeshFromSprite(Sprite sprite)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[sprite.vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = sprite.vertices[i];
        }

        int[] triangles = new int[sprite.triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = sprite.triangles[i];
        }

        Vector2[] uvs = sprite.uv;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

}
