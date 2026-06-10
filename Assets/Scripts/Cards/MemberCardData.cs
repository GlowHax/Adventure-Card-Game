using UnityEngine;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "New Member", menuName = "Cards/Member")]
    public class MemberCardData : CardData
    {
        public int honorCost;
        public int baseSpeed;
        public int baseStrength;
        public string abilityDescription;
        
        [Header("Abilities")]
        public int bonusStrengthOnMiss;

        [Header("Equipment")]
        public System.Collections.Generic.List<EquipmentCardData> startingEquipment = new System.Collections.Generic.List<EquipmentCardData>();
    }
}
