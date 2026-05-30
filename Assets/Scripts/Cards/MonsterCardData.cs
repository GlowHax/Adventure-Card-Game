using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "New Monster", menuName = "Cards/Monster")]
    public class MonsterCardData : CardData
    {
        public int healthPoints;
        public int speed;
        public int strength;
        public string passiveEffectDescription;
    }
}
