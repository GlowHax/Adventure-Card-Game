using UnityEngine;

namespace AdventureCardGame.Managers
{
    public class TableLayoutManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject cardPrefab;

        [Header("Player Area")]
        public Transform[] memberSlots;
        public Transform[] itemSlots;

        [Header("Center Area")]
        public Transform encounterSlot;

        [Header("Deck Area")]
        public Transform deckSlot;
        public Transform discardSlot;

        [Header("Shop Area")]
        public Transform[] shopSlots;

        private void Start()
        {
            if (cardPrefab == null) return;

            // Spawn test cards to visualize the layout
            SpawnTestCards(memberSlots, "Member_");
            SpawnTestCards(itemSlots, "Item_");
            SpawnTestCard(encounterSlot, "Encounter_Active");
            SpawnTestCard(deckSlot, "Deck_Stack");
            SpawnTestCard(discardSlot, "Discard_Stack");
            SpawnTestCards(shopSlots, "Shop_Offer_");
        }

        private void SpawnTestCards(Transform[] slots, string prefix)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                SpawnTestCard(slots[i], prefix + i);
            }
        }

        private void SpawnTestCard(Transform slot, string name)
        {
            if (slot == null) return;
            GameObject card = Instantiate(cardPrefab, slot.position, slot.rotation, slot);
            card.name = "TestCard_" + name;
        }
    }
}
