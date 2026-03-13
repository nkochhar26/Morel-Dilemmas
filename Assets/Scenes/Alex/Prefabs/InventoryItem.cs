using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryItem : MonoBehaviour
{
    Vector3 originalSize;
    Vector3 originalSizeWorld;
    public bool followmouse = false;
    public LayerMask targetLayer;
    public MeshRenderer meshRenderer;
    public Image image;
    public FoodItemObject foodItem;
    
    void Start()
    {
        originalSize = transform.localScale;
        originalSizeWorld = transform.lossyScale;
    }
    
    public void Grow()
    {
        if(transform.parent.GetComponent<DragFoodInto>() == null) return;
        //transform.DOScale(originalSize * 1.5f, 0.2f);
    }

    public void Shrink()
    {
        StopFollowMouse();
        if(transform.parent.GetComponent<DragFoodInto>() == null) return;
        //transform.DOScale(originalSize * 1f, 0.2f);
    }

    public void FollowMouse()
    {
        AlexKitchenInventoryUI.Instance.draggedItem = this;
        followmouse = true;
    }

    public void StopFollowMouse()
    {
        followmouse = false;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, targetLayer);
        if (hit != null)
        {
            DragFoodInto target = hit.GetComponent<DragFoodInto>();
            if (target != null)
            {
                target.AddItem(this);
                InventoryManager.foodItems.Remove(foodItem);
                return;
            }
        }

        if (AlexKitchenInventoryUI.Instance != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                AlexKitchenInventoryUI.Instance.GetComponent<RectTransform>()
            );
        }
    }

    public void SetItem(FoodItemObject item){
        foodItem = item;
        meshRenderer.material.mainTexture = item.foodItem.defaultSprite.texture;
        image.sprite = item.foodItem.defaultSprite;
        meshRenderer.GetComponent<MeshFilter>().mesh = BuildMeshFromSprite(item.foodItem.defaultSprite);
    }

    void Update()
    {
        if (followmouse)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = worldPosition;
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
