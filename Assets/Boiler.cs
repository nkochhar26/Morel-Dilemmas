using System.Collections.Generic;
using TMPro;
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

    [Header("Doneness")]
    [Tooltip("Take ingredients out (Clear Ingredients) while the slider is green for full quality.")]
    public DonenessWindow doneness = new DonenessWindow();
    [Tooltip("Optional text that flashes 'Perfect!', 'Overcooked', etc.")]
    public TMP_Text resultText;

    // 0 = just started, 1 = timer ran out (same as the slider value)
    private float Doneness => time > 0 ? Mathf.Clamp01(1f - tempTime / time) : 1f;

    void Start()
    {
        if (timeRemaining != null)
        {
            timeRemaining.value = 0;
            CookingDoneness.ApplySliderColors(timeRemaining, doneness);
        }
        else
        {
            Debug.LogWarning($"{GetType().Name} ({name}): timeRemaining slider is not assigned.");
        }
        if (resultText != null)
        {
            resultText.raycastTarget = false; // display-only: never block the Clear button
            resultText.gameObject.SetActive(false);
        }

        tempTime = time;
    }

    public override void AddItem(InventoryItem item)
    {
        tempTime = time;
        base.AddItem(item);
    }

    // "Clear Ingredients" button: take everything out now. How well it's cooked depends on the timer.
    public void ClearBoiler()
    {
        TakeOut(itemsInBoiler.Count > 0 ? Doneness : 0f);
    }

    private void TakeOut(float donenessAtRemoval)
    {
        CookResult shown = CookResult.Raw;
        foreach (InventoryItem item in itemsInBoiler)
        {
            if (item == null) continue;
            if (item.foodItem != null)
            {
                shown = CookingDoneness.Cook(item.foodItem, CookingStep.Boil, donenessAtRemoval, doneness);
                GameManager.Instance.inventoryManager.AddFoodObject(item.foodItem, item.foodItem.isIntermediate);
            }
            item.transform.SetParent(null); // so the next physics step doesn't pick it up again
            Destroy(item.gameObject);
        }
        itemsInBoiler.Clear();
        CookingDoneness.ShowResult(this, resultText, shown);

        tempTime = time;
        if (timeRemaining != null)
        {
            timeRemaining.value = 0;
        }
        if (boilParticles != null)
        {
            var emission = boilParticles.emission;
            emission.rateOverTime = boilParticleRateRange.x;
        }
    }

    // Swirl every piece of the item. A sliced item's original mesh is replaced by its
    // slice pieces, so stir all of its rigidbodies rather than item.meshRenderer.
    private void Stir(InventoryItem item)
    {
        foreach (Rigidbody2D rb in item.GetComponentsInChildren<Rigidbody2D>())
        {
            Vector2 pos = rb.transform.position - transform.position;
            Vector2 tangent = new Vector2(pos.y, -pos.x);
            rb.AddForce(tangent * stirForce);
        }
    }

    void FixedUpdate()
    {
        foreach(Transform child in transform)
        {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item == null) continue;

            // register first, so a problem with the visuals can never stop the item cooking
            if(!itemsInBoiler.Contains(item))
            {
                itemsInBoiler.Add(item);
            }
            Stir(item);
        }

        if(itemsInBoiler.Count>0)
        {
            tempTime-=Time.deltaTime;
            if (timeRemaining != null)
            {
                timeRemaining.value = Doneness;
            }
            if (boilParticles != null)
            {
                var emission = boilParticles.emission;
                emission.rateOverTime = Mathf.Lerp(boilParticleRateRange.x, boilParticleRateRange.y, Doneness);
            }

            if(tempTime<=0){
                // Left in too long: burnt. Pulled out automatically so ingredients aren't lost.
                TakeOut(1f);
            }
        }else
        {
            tempTime = time;
        }

    }
}
