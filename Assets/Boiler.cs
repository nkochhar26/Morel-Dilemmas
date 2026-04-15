using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boiler : DragFoodInto
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
        timeRemaining.value = 0;
        tempTime = time;
    }

    public override void AddItem(InventoryItem item)
    {
        tempTime = time;
        base.AddItem(item);
    }

    public void ClearBoiler()
    {
        var emission = boilParticles.emission;
        
        foreach(InventoryItem item in itemsInBoiler)
        {
            Destroy(item.gameObject);
        }
        itemsInBoiler.Clear();

        emission.rateOverTime = boilParticleRateRange.x;
        tempTime = time;
        timeRemaining.value = 0;

    }

    void FixedUpdate()
    {
        foreach(Transform child in transform)
        {
            if(child.GetComponent<InventoryItem>())
            {
                GameObject meshObject = child.GetComponent<InventoryItem>().meshRenderer.gameObject;
                Vector2 pos = meshObject.transform.position-transform.position;
                Vector2 tangent = new Vector2(pos.y, -pos.x);
                meshObject.GetComponent<Rigidbody2D>().AddForce(tangent*stirForce);

                if(!itemsInBoiler.Contains(child.GetComponent<InventoryItem>()))
                {
                    itemsInBoiler.Add(child.GetComponent<InventoryItem>());
                }
            }
        }

        if(itemsInBoiler.Count>0)
        {
            tempTime-=Time.deltaTime;
            timeRemaining.value = 1 - (tempTime * 1.0f / time);
            var emission = boilParticles.emission;
            emission.rateOverTime = Mathf.Lerp(boilParticleRateRange.x, boilParticleRateRange.y, 1-(tempTime/time));

            if(tempTime<=0){                

                FoodItemObject foodObject = FoodManager.Instance.IngredientsToFood(CookingStep.Boil, itemsInBoiler.ConvertAll(i=>i.foodItem));
                if (foodObject.foodItem == null)
                {
                    
                    return;
                }

                tempTime = time;

                //here is a completed order - probably dont immediately set 
                GameManager.Instance.orderManager.SetHeldOrder(foodObject);
                
                foreach(InventoryItem item in itemsInBoiler)
                {
                    Destroy(item.gameObject);
                }
                itemsInBoiler.Clear();

                emission.rateOverTime = boilParticleRateRange.x;
            }
                
        }else
        {
            tempTime = time;
        }

    }
}
