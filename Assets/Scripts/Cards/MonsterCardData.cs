using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "New Monster", menuName = "Cards/Monster")]
    public class MonsterCardData : CardData
    {
        [Header("Phase Settings")]
        public int phase;

        [Header("Stats")]
        public int healthPoints;
        public int speed;
        public int strength;
        public string passiveEffectDescription;

        [Header("Effects")]
        public bool winsTies;
        public int bonusStrengthOnDefense;
        public int healOnSuccessfulDefense;
        public int bonusSpeedOnSuccessfulDefense;
        public int healOnAttack;
        public bool rewardCoinInsteadOfHonor;
        public int rewardTreasureOnDefeat;
    }
}
