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

        public int discardedCardsCount = 0;
        private const int INITIAL_DISCARD_CARDS = 0;

        private void Start()
        {
            if (cardPrefab == null) return;

            // Spawn test cards to visualize the layout
            SpawnTestCards(memberSlots, "Member_", testMembers);
            SpawnTestCards(itemSlots, "Item_", testItems);
            
            // Deck Stack (Face down, 5 cards thick)
            SpawnTestCard(deckSlot, "Deck_Stack", null, true, 5, 0);

            
            // Encounter Card (will be drawn when clicking deck)
            // Cards.CardData encounterData = (testMonsters != null && testMonsters.Length > 0) ? testMonsters[0] : null;
            // SpawnTestCard(discardSlot, "Encounter_Active", encounterData, false, 1, 3);
            
            // Shop cards should only spawn when a Shop encounter is active
            // SpawnTestCards(shopSlots, "Shop_Offer_", testMembers);
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
                
                // Add slight randomness for deck and discard piles so they look like messy stacks
                float randomRotZ = 0f;
                if (slot == discardSlot || slot == deckSlot) randomRotZ = Random.Range(-4f, 4f);
                
                // Flip 180 around local Y to face down. Rotate around Z to spin on table.
                Quaternion rotationOffset = faceDown ? Quaternion.Euler(0, 180f, randomRotZ) : Quaternion.Euler(0, 0, randomRotZ);
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

                    // If it's the top card of the deck, make it clickable
                    if (name == "Deck_Stack" && i == stackCount - 1)
                    {
                        card.AddComponent<Mechanics.ClickableDeck>();
                    }
                }
            }
        }

        private GameObject currentEncounterCard;

        public void DrawEncounter()
        {
            if (currentEncounterCard != null) return; // Already have an encounter

            Cards.CardData encounterData = null;
            if (testMonsters != null && testMonsters.Length > 0)
            {
                encounterData = testMonsters[Random.Range(0, testMonsters.Length)];
            }
            
            if (encounterData == null) return;

            StartCoroutine(DrawEncounterRoutine(encounterData));
        }

        private System.Collections.IEnumerator DrawEncounterRoutine(Cards.CardData encounterData)
        {
            GameObject prefabToUse = monsterPrefab != null ? monsterPrefab : cardPrefab;
            
            // Spawn at the top of the deck stack (face down)
            Vector3 spawnPos = deckSlot.position + new Vector3(0, 5 * 0.005f, 0);
            Quaternion spawnRot = deckSlot.rotation * Quaternion.Euler(0, 180, 0);
            
            currentEncounterCard = Instantiate(prefabToUse, spawnPos, spawnRot, deckSlot);
            currentEncounterCard.name = "TestCard_Encounter_Active";
            
            var display = currentEncounterCard.GetComponent<Cards.CardDisplay>();
            if (display != null) display.Setup(encounterData);

            // Animate to encounter slot (face up)
            if (Mechanics.CardAnimator.Instance != null)
            {
                yield return StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCardWithFlip(
                    currentEncounterCard.transform, 
                    GetEncounterPosition(), 
                    deckSlot.rotation,
                    encounterSlot.rotation,
                    180f, // Start with 180 degrees around local Y
                    0f,    // End at 0 degrees
                    1.0f
                ));
            }
            else
            {
                currentEncounterCard.transform.position = GetEncounterPosition();
                currentEncounterCard.transform.rotation = encounterSlot.rotation;
            }

            // Move parent to encounter slot
            currentEncounterCard.transform.SetParent(encounterSlot);

            if (GameManager.Instance != null && encounterData is Cards.MonsterCardData)
            {
                GameManager.Instance.ChangeState(GameState.ActionPhase);
            }
        }

        public void AddToDiscardPile()
        {
            discardedCardsCount++;
        }

        public void ClearCurrentEncounter()
        {
            currentEncounterCard = null;
        }

        public Vector3 GetNextDiscardPosition()
        {
            // Limit the visual height so the stack doesn't grow infinitely
            int visualCount = Mathf.Min(discardedCardsCount, 4);
            float yOffset = (INITIAL_DISCARD_CARDS + visualCount) * 0.005f;
            return discardSlot.position + new Vector3(0, yOffset, 0);
        }

        public Vector3 GetEncounterPosition()
        {
            // Limit the visual height of the discard pile underneath it
            int visualCount = Mathf.Min(discardedCardsCount, 4);
            // Encounter card floats slightly above the highest discard card
            float yOffset = (INITIAL_DISCARD_CARDS + visualCount) * 0.005f + 0.01f;
            return encounterSlot.position + new Vector3(0, yOffset, 0);
        }
    }
}
