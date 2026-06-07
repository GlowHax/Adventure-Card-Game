using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "NewTreasureCard", menuName = "Adventure/Cards/Treasure Card")]
    public class TreasureCardData : CardData
    {
        [Header("Treasure Effects")]
        public int goldAmount = 0;
        
        // In the future, this could be a reference to an ItemCardData if the treasure gives an item instead.
        // For now, if goldAmount is 0, we can assume it gives an item (which we can add later).
        public ItemCardData itemReward;
    }
}
