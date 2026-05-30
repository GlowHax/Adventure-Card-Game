using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "New Object", menuName = "Cards/General Object")]
    public class ObjectCardData : CardData
    {
        public string passiveEffectDescription;
        // Placed in player's 3 general slots
    }
}
