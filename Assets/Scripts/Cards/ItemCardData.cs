using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Cards/Equipment Item")]
    public class ItemCardData : CardData
    {
        public int strengthBonus;
        public int speedBonus;
        // This is placed on a member
    }
}
