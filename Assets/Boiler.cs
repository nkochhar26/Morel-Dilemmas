using System.Collections.Generic;
using UnityEngine;

public class Boiler : DragFoodInto
{
    public float stirForce = 10f;
    public float time;
    public float tempTime;
    public List<InventoryItem> itemsInBoiler = new List<InventoryItem>();
    public Vector2 boilParticleRateRange = new Vector2(10, 50);
    public ParticleSystem boilParticles;

    void Start()
    {
        tempTime = time;
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
            var emission = boilParticles.emission;
            emission.rateOverTime = Mathf.Lerp(boilParticleRateRange.x, boilParticleRateRange.y, 1-(tempTime/time));

            if(tempTime<=0){                
                tempTime = time;

                FoodItemObject foodObject = FoodManager.Instance.IngredientsToFood(tags.boiled, itemsInBoiler.ConvertAll(i=>i.foodItem));
                
                foreach(InventoryItem item in itemsInBoiler)
                {
                    Destroy(item.gameObject);
                }
                itemsInBoiler.Clear();

                emission.rateOverTime = boilParticleRateRange.x;
            }
                
        }else tempTime = time;


    }
}
