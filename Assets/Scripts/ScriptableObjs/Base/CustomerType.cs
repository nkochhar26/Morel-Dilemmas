using UnityEngine;

[CreateAssetMenu(fileName = "CustomerType", menuName = "CustomerType", order = 0)]
public class CustomerType : ScriptableObject
{
    public float starsGiven;
    public MushroomItem mushroom;

    [Header("Wallet (looted when their body is hidden)")]
    [Tooltip("Money in their wallet, as a multiple of the meal they ordered. 2 = twice what the meal would have paid.")]
    public float walletMultiplier = 2f;
    [Tooltip("Extra flat money on top, e.g. for a rich customer type.")]
    public int walletBonus = 0;
}
