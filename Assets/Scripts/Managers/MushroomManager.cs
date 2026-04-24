using UnityEngine;
using System.Collections.Generic;

public class MushroomManager : MonoBehaviour
{
    [SerializeField] private List<MushroomItem> allMushrooms = new List<MushroomItem>();

    public List<MushroomItem> GetAllMushrooms()
    {
        return allMushrooms;
    }
    public MushroomItem GetMushroomAtIndex(int index)
    {
        return allMushrooms[index];
    }
}
