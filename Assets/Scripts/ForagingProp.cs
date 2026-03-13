using UnityEngine;
using System.Collections.Generic;

public class ForagingProp : MonoBehaviour
{
    public Dictionary<Transform, bool> spawnSpots = new Dictionary<Transform, bool>();

    public void Awake()
    {
        foreach (Transform child in transform)
        {
            spawnSpots.Add(child, false);
        }
    }

    public Transform GetOpenSpot()
    {
        foreach(Transform spot in spawnSpots.Keys)
        {
            if (spawnSpots[spot] == false)
            {
                spawnSpots[spot] = true;
                return spot.transform;
            }
        }
        return null;
    }
}
