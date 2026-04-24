using UnityEngine;
using System.Collections.Generic;

public class MushroomSpawningManager : MonoBehaviour
{
    [SerializeField] private GameObject pickupPrefab;
    private List<MushroomItem> collectibleMushrooms;
    [SerializeField] private List<ForagingProp> foragingProps = new List<ForagingProp>();
    [SerializeField] private List<ForagingProp> ground = new List<ForagingProp>();

    private void Start()
    {
        collectibleMushrooms = GameManager.Instance.mushroomManager.GetAllMushrooms();
        LoadMushrooms();
    }

    // currently just flat memorized 
    private void LoadMushrooms()
    {
        int numMushrooms = GameManager.Instance.dayManager.GetDay() * 12;   //TODO: replace with real algorithm lol
        for (int i = 0; i < numMushrooms; i++)
        {
            int index = (int) (Random.value * collectibleMushrooms.Count);
            MushroomItem mushroomData = collectibleMushrooms[index];
            Debug.Log(mushroomData.name);
            Mushroom mushroom;

            if (mushroomData.lookalikeName != "")
            {
                if (Random.value >= 0.4)   // hardcoded atm, can be adjusted idk help T_T
                {
                    //regualr
                    mushroom = new Mushroom(mushroomData, false);
                }
                else
                {
                    //lookalike
                    mushroom = new Mushroom(mushroomData, true);
                }
            }
            else
            {
                mushroom = new Mushroom(mushroomData, false);
            }

            //needs to differentiate from tree guys and non tree guys
            (Transform posTransform, int sortingOrder) = GetSpawnSpot(mushroomData.spawnType);
            if (posTransform != null)
            {
                GameObject mushroomObj = Instantiate(pickupPrefab, posTransform.position, Quaternion.identity);
                mushroomObj.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
                mushroomObj.GetComponent<MushroomPickup>().SetPickupItem(mushroom);
            }
        }
    }

    private (Transform, int) GetSpawnSpot(SpawnType spawnType)
    {
        List<ForagingProp> props;
        if (spawnType == SpawnType.Ground)
        {
            props = ground;
        }
        else if (spawnType == SpawnType.Wood)
        {
            props = foragingProps;
        }
        else
        {
            List<ForagingProp> combined = new List<ForagingProp>();
            combined.AddRange(ground);
            combined.AddRange(foragingProps);
            props = combined;

        }
        // loop through props and determine a spot
        int propIndex = Random.Range(0, props.Count);
        Transform spot = props[propIndex].GetOpenSpot();
        return (spot, props[propIndex].GetComponent<SpriteRenderer>().sortingOrder + 1);
        
    }
}
