using UnityEngine;

namespace AdventureCardGame.Managers
{
    public class TableLayoutManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject cardPrefab;
        public GameObject monsterPrefab;
        public GameObject memberPrefab;

        [Header("Test Data")]
        public Cards.CardData[] testMembers;
        public Cards.CardData[] testItems;
        public Cards.CardData[] testMonsters;

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
            SpawnTestCards(memberSlots, "Member_", testMembers);
            SpawnTestCards(itemSlots, "Item_", testItems);
            
            // Deck Stack (Face down, 5 cards thick)
            SpawnTestCard(deckSlot, "Deck_Stack", null, true, 5, 0);

            // Discard Stack (Face up, 3 cards thick)
            SpawnTestCard(discardSlot, "Discard_Stack", null, false, 3, 0);
            
            // Encounter Card (On top of discard pile)
            Cards.CardData encounterData = (testMonsters != null && testMonsters.Length > 0) ? testMonsters[0] : null;
            SpawnTestCard(discardSlot, "Encounter_Active", encounterData, false, 1, 3);
            
            SpawnTestCards(shopSlots, "Shop_Offer_", testMembers);
        }

        private void SpawnTestCards(Transform[] slots, string prefix, Cards.CardData[] possibleData = null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Cards.CardData data = (possibleData != null && possibleData.Length > 0) ? possibleData[i % possibleData.Length] : null;
                SpawnTestCard(slots[i], prefix + i, data);
            }
        }

        private void SpawnTestCard(Transform slot, string name, Cards.CardData data = null, bool faceDown = false, int stackCount = 1, int startingHeightIndex = 0)
        {
            if (slot == null) return;
            
            for (int i = 0; i < stackCount; i++)
            {
                // Simulate physical thickness
                Vector3 positionOffset = new Vector3(0, (startingHeightIndex + i) * 0.005f, 0);
                
                // Flip 180 around local X to face down (instead of Z which spun it like a wheel)
                Quaternion rotationOffset = faceDown ? Quaternion.Euler(180, 0, 0) : Quaternion.identity;
                if (data == null && cardPrefab == null) return;

                GameObject prefabToUse = cardPrefab;
                if (data != null)
                {
                    if (data is Cards.MonsterCardData && monsterPrefab != null) prefabToUse = monsterPrefab;
                    else if (data is Cards.MemberCardData && memberPrefab != null) prefabToUse = memberPrefab;
                }

                GameObject card = Instantiate(prefabToUse, slot.position + positionOffset, slot.rotation * rotationOffset, slot);
                card.name = "TestCard_" + name + (stackCount > 1 ? "_" + i : "");

                // Only assign data if we provided any, and generally we only care about the top card visually
                if (data != null && i == stackCount - 1)
                {
                    var display = card.GetComponent<Cards.CardDisplay>();
                    if (display != null)
                    {
                        display.Setup(data);
                    }
                }
                
                // Hide canvas completely if face down (since we have a card back now)
                if (faceDown)
                {
                    Canvas canvas = card.GetComponentInChildren<Canvas>();
                    if (canvas != null) canvas.enabled = false;
                }
            }
        }
    }
}
