using UnityEngine;

public class Mushroom
{
    public MushroomItem mushroomItem;
    public bool isLookalike;

    public Mushroom(MushroomItem mushroomItem, bool isLookalike)
    {
        this.mushroomItem = mushroomItem;
        this.isLookalike = isLookalike;
    }
    public Mushroom() : this(null, false) {}

    public Mushroom(MushroomItem mushroomItem) : this(mushroomItem, false) {}
}
