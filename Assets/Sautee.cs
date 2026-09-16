using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sautee : DragFoodInto
{
    public float stirForce = 10f;
    public float time;
    public float tempTime;
    public List<InventoryItem> itemsInBoiler = new List<InventoryItem>();
    public Vector2 boilParticleRateRange = new Vector2(10, 50);
    public ParticleSystem boilParticles;
    public Slider timeRemaining;

    void Start()
    {
        if (timeRemaining != null)
        {
            timeRemaining.value = 0;
        }
        else
        {
            Debug.LogWarning($"{GetType().Name} ({name}): timeRemaining slider is not assigned.");
        }

        tempTime = time;
    }

    public override void AddItem(InventoryItem item)
    {
        tempTime = time;
        base.AddItem(item);
    }

    private void RefreshItemsInBoiler()
    {
        List<InventoryItem> currentItems = new List<InventoryItem>();

        foreach (Transform child in transform)
        {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item != null)
            {
                currentItems.Add(item);
            }
        }

        itemsInBoiler = currentItems;
    }

    public void ClearBoiler()
    {
        if (boilParticles != null)
        {
            var emission = boilParticles.emission;
            emission.rateOverTime = boilParticleRateRange.x;
        }
        else
        {
            Debug.LogWarning($"{GetType().Name} ({name}): boilParticles is not assigned; cannot reset particle emission.");
        }

        foreach (Transform child in transform)
        {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item == null)
            {
                continue;
            }

            if (item.foodItem != null)
            {
                GameManager.Instance.inventoryManager.AddFoodObject(item.foodItem, true);
            }

            Destroy(child.gameObject);
        }

        itemsInBoiler.Clear();

        tempTime = time;
        if (timeRemaining != null)
        {
            timeRemaining.value = 0;
        }
        else
        {
            Debug.LogWarning($"{name}: timeRemaining slider is not assigned; cannot reset timer.");
        }
    }

    void FixedUpdate()
    {
        RefreshItemsInBoiler();

        foreach (Transform child in transform)
        {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item == null)
            {
                continue;
            }

            if (item.meshRenderer == null || item.meshRenderer.gameObject == null)
            {
                continue;
            }

            GameObject meshObject = item.meshRenderer.gameObject;
            Rigidbody2D rb = meshObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 pos = meshObject.transform.position - transform.position;
                Vector2 tangent = new Vector2(pos.y, -pos.x);
                rb.AddForce(tangent * stirForce);
            }
        }

        if (itemsInBoiler.Count > 0)
        {
            tempTime -= Time.deltaTime;

            if (timeRemaining != null)
            {
                timeRemaining.value = 1 - (tempTime * 1.0f / time);
            }

            if (boilParticles != null)
            {
                var emission = boilParticles.emission;
                emission.rateOverTime = Mathf.Lerp(boilParticleRateRange.x, boilParticleRateRange.y, 1 - (tempTime / time));
            }

            if (tempTime <= 0)
            {
                FoodItemObject foodObject = FoodManager.Instance.IngredientsToFood(CookingStep.Sautee, itemsInBoiler.ConvertAll(i => i.foodItem));
                if (foodObject == null || foodObject.foodItem == null)
                {
                    tempTime = time;
                    if (timeRemaining != null)
                    {
                        timeRemaining.value = 0;
                    }
                    return;
                }

                tempTime = time;

                GameManager.Instance.orderManager.SetHeldOrder(foodObject);

                foreach (InventoryItem item in itemsInBoiler.ToArray())
                {
                    if (item != null)
                    {
                        Destroy(item.gameObject);
                    }
                }
                itemsInBoiler.Clear();

                if (boilParticles != null)
                {
                    var emission = boilParticles.emission;
                    emission.rateOverTime = boilParticleRateRange.x;
                }
            }
        }
        else
        {
            tempTime = time;
            if (timeRemaining != null)
            {
                timeRemaining.value = 0;
            }
        }
    }
}
