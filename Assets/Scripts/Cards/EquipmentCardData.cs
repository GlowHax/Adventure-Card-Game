using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "AdventureCardGame/Cards/Equipment")]
    public class EquipmentCardData : CardData
    {
        [Header("Shop Settings")]
        public int cost = 5;
        public int sellValue = 2;

        [Header("Equipment Buffs")]
        public int strengthBuff;
        public int speedBuff;
        [Tooltip("If true, the strength buff only applies during defense (when attacked by a monster).")]
        public bool onlyOnDefense;
    }
}
