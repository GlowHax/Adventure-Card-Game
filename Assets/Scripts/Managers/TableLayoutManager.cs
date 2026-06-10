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

        [Header("Deck Configuration")]
        [Tooltip("The actual playable deck. Cards are drawn from index 0 (top) downwards.")]
        public System.Collections.Generic.List<Cards.CardData> encounterDeck;
        
        [Tooltip("If true, the encounter deck will be shuffled at the start of the game.")]
        public bool shuffleEncounterDeckOnStart = true;

        [Tooltip("The actual playable treasure deck. Cards are drawn from index 0 (top) downwards.")]
        public System.Collections.Generic.List<Cards.TreasureCardData> treasureDeck;
        
        [Tooltip("If true, the treasure deck will be shuffled at the start of the game.")]
        public bool shuffleTreasureDeckOnStart = true;

        [Header("Player Area")]
        public Transform[] memberSlots;
        public Transform[] itemSlots;

        [Header("Center Area")]
        public Transform encounterSlot;
        public Transform activeTreasureSlot;

        [Header("Deck Area")]
        public Transform deckSlot;
        public Transform discardSlot;

        [Header("Shop Area")]
        public Transform[] shopSlots;

        public int discardedCardsCount = 0;
        private const int INITIAL_DISCARD_CARDS = 0;
        [HideInInspector]
        public bool CanDrawTreasure = false;

        [Header("Active Members")]
        public System.Collections.Generic.List<GameObject> activeMembers = new System.Collections.Generic.List<GameObject>();

        private void Start()
        {
            if (shuffleEncounterDeckOnStart && encounterDeck != null)
            {
                ShuffleList(encounterDeck);
            }
            
            if (shuffleTreasureDeckOnStart && treasureDeck != null)
            {
                ShuffleList(treasureDeck);
            }

            if (cardPrefab == null) return;

            // Spawn test members using dynamic layout
            if (testMembers != null)
            {
                for (int i = 0; i < testMembers.Length; i++)
                {
                    AddMember(testMembers[i]);
                }
            }
            
            SpawnTestCards(itemSlots, "Item_", testItems);
            
            // Deck Stack (Face down, up to 5 cards thick depending on deck size)
            int initialDeckCount = encounterDeck != null ? Mathf.Min(encounterDeck.Count, 5) : 5;
            // If the deck is completely empty at the start, still spawn at least 1 test card as fallback (so you can click it)
            if (initialDeckCount == 0 && (encounterDeck == null || encounterDeck.Count == 0)) initialDeckCount = 1;
            
            SpawnTestCard(deckSlot, "Deck_Stack", null, true, initialDeckCount, 0);

            // Treasure Deck Stack (Face down)
            Transform tDeck = transform.Find("TreasureDeckSlot");
            if (tDeck != null)
            {
                SpawnTestCard(tDeck, "TreasureDeck_Stack", null, true, 3, 0);
            }
            
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
                
                // Add slight randomness for any stack so they look like messy piles
                float randomRotZ = 0f;
                if (stackCount > 1 || slot == discardSlot || slot == deckSlot || name.Contains("Deck")) 
                {
                    randomRotZ = Random.Range(-4f, 4f);
                }
                
                // Flip 180 around local Y to face down. Rotate around Z to spin on table.
                Quaternion rotationOffset = faceDown ? Quaternion.Euler(0, 180f, randomRotZ) : Quaternion.Euler(0, 0, randomRotZ);
                if (data == null && cardPrefab == null) return;

                GameObject prefabToUse = cardPrefab;
                if (name.Contains("Treasure"))
                {
                    #if UNITY_EDITOR
                    GameObject tPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardPrefab_Treasure.prefab");
                    if (tPrefab != null) prefabToUse = tPrefab;
                    #endif
                }
                else if (data != null)
                {
                    if (data is Cards.MonsterCardData && monsterPrefab != null) prefabToUse = monsterPrefab;
                    else if (data is Cards.MemberCardData && memberPrefab != null) prefabToUse = memberPrefab;
                    else if (data is Cards.EquipmentCardData)
                    {
                        #if UNITY_EDITOR
                        GameObject eqPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardPrefab_Equipment.prefab");
                        if (eqPrefab != null) prefabToUse = eqPrefab;
                        #endif
                    }
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
                    else if (name == "TreasureDeck_Stack" && i == stackCount - 1)
                    {
                        card.AddComponent<Mechanics.ClickableTreasureDeck>();
                    }
                }
            }
        }

        private GameObject currentEncounterCard;

        public void DrawEncounter()
        {
            if (currentEncounterCard != null) return; // Already have an encounter

            Cards.CardData encounterData = null;
            
            // Draw from configured deck if available
            if (encounterDeck != null && encounterDeck.Count > 0)
            {
                encounterData = encounterDeck[0];
                encounterDeck.RemoveAt(0); // Remove the drawn card from the top of the deck
                UpdateVisualDeckStack();
            }
            // Fallback to random test monsters if deck is empty
            else if (testMonsters != null && testMonsters.Length > 0)
            {
                Debug.LogWarning("Encounter Deck is empty! Drawing random test monster instead.");
                encounterData = testMonsters[Random.Range(0, testMonsters.Length)];
            }
            
            if (encounterData == null) return;

            StartCoroutine(DrawEncounterRoutine(encounterData));
        }

        private void UpdateVisualDeckStack()
        {
            if (deckSlot == null) return;
            
            int targetVisualCount = encounterDeck != null ? Mathf.Min(encounterDeck.Count, 5) : 5;
            
            System.Collections.Generic.List<GameObject> visualCards = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in deckSlot)
            {
                if (child.name.StartsWith("TestCard_Deck_Stack"))
                {
                    visualCards.Add(child.gameObject);
                }
            }
            
            if (visualCards.Count > targetVisualCount)
            {
                int toRemove = visualCards.Count - targetVisualCount;
                for (int i = 0; i < toRemove; i++)
                {
                    GameObject cardToRemove = visualCards[visualCards.Count - 1 - i];
                    Destroy(cardToRemove);
                }
                
                if (targetVisualCount > 0)
                {
                    GameObject newTopCard = visualCards[targetVisualCount - 1];
                    if (newTopCard.GetComponent<Mechanics.ClickableDeck>() == null)
                    {
                        newTopCard.AddComponent<Mechanics.ClickableDeck>();
                    }
                }
            }
        }

        private System.Collections.IEnumerator DrawEncounterRoutine(Cards.CardData encounterData)
        {
            GameObject prefabToUse = cardPrefab;
            if (encounterData is Cards.MonsterCardData && monsterPrefab != null) prefabToUse = monsterPrefab;
            else if (encounterData is Cards.EventCardData)
            {
                GameObject eventPrefabLoad = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardPrefab_Event.prefab");
                if (eventPrefabLoad != null) prefabToUse = eventPrefabLoad;
            }
            
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

            if (GameManager.Instance != null)
            {
                if (encounterData is Cards.MonsterCardData)
                {
                    GameManager.Instance.ChangeState(GameState.ActionPhase);
                }
                else if (encounterData is Cards.EventCardData eventData)
                {
                    GameManager.Instance.ChangeState(GameState.Event);
                    
                    var clickable = currentEncounterCard.AddComponent<Mechanics.ClickableEventCard>();
                    while (!clickable.isClicked)
                    {
                        yield return null;
                    }
                    Destroy(clickable);

                    yield return StartCoroutine(eventData.ExecuteEvent(currentEncounterCard));
                    
                    // Switch camera back to encounter just in case the event changed it (e.g. Ausgleich)
                    if (CameraManager.Instance != null)
                    {
                        CameraManager.Instance.SwitchToEncounter();
                    }
                    
                    // Pause briefly so the player can re-focus on the card before it gets discarded
                    yield return new WaitForSeconds(1.0f);
                    
                    // Keep a reference for the discard animation
                    GameObject cardToDiscard = currentEncounterCard;
                    
                    // Discard the event card after execution
                    ClearCurrentEncounter();
                    AddToDiscardPile();
                    
                    if (Mechanics.CardAnimator.Instance != null && cardToDiscard != null)
                    {
                        StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCardWithFlip(
                            cardToDiscard.transform, 
                            GetNextDiscardPosition(), 
                            encounterSlot.rotation,
                            discardSlot.rotation,
                            0f, 180f, 1.0f // Flip 180 degrees to land face down
                        ));
                    }
                    else if (cardToDiscard != null)
                    {
                        cardToDiscard.transform.position = GetNextDiscardPosition();
                        cardToDiscard.transform.rotation = discardSlot.rotation * Quaternion.Euler(0, 180f, 0);
                    }
                    
                    if (cardToDiscard != null)
                    {
                        cardToDiscard.transform.SetParent(discardSlot);
                        var interactable = cardToDiscard.GetComponent<Cards.CardInteractable>();
                        if (interactable != null) Destroy(interactable);
                    }
                    
                    // Delay slightly to let the discard animation finish
                    yield return new WaitForSeconds(1.0f);
                    
                    // If no monsters are active, go to Idle
                    GameManager.Instance.ChangeState(GameState.Idle);
                }
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

        public void AddMember(Cards.CardData memberData)
        {
            if (activeMembers.Count >= memberSlots.Length) return; // Full

            GameObject memberCard = Instantiate(cardPrefab);
            memberCard.name = "Member_" + activeMembers.Count;
            var display = memberCard.GetComponent<Cards.CardDisplay>();
            if (display != null && memberData != null)
            {
                display.Setup(memberData);
            }
            activeMembers.Add(memberCard);
            
            UpdateMemberLayout();
        }

        public void RemoveMember(GameObject memberCard)
        {
            if (activeMembers.Contains(memberCard))
            {
                activeMembers.Remove(memberCard);
                Destroy(memberCard);
                UpdateMemberLayout();
            }
        }

        public void UpdateMemberLayout()
        {
            int count = activeMembers.Count;
            if (count == 0) return;

            Transform[] targetSlots = new Transform[count];
            if (count == 1)
            {
                targetSlots[0] = memberSlots[1]; // Mitte
            }
            else if (count == 2)
            {
                targetSlots[0] = memberSlots[0]; // Links
                targetSlots[1] = memberSlots[1]; // Mitte
            }
            else if (count == 3)
            {
                targetSlots[0] = memberSlots[0]; // Links
                targetSlots[1] = memberSlots[1]; // Mitte
                targetSlots[2] = memberSlots[2]; // Rechts
            }
            else
            {
                // Fallback for more than 3
                for (int i = 0; i < count; i++)
                    targetSlots[i] = memberSlots[i % memberSlots.Length];
            }

            for (int i = 0; i < count; i++)
            {
                GameObject member = activeMembers[i];
                Transform targetSlot = targetSlots[i];
                
                member.transform.SetParent(targetSlot);
                
                if (Mechanics.CardAnimator.Instance != null)
                {
                    StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCard(member.transform, targetSlot.position, targetSlot.rotation, 0.5f));
                }
                else
                {
                    member.transform.position = targetSlot.position;
                    member.transform.rotation = targetSlot.rotation;
                }
            }
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

        [HideInInspector]
        public bool IsDrawingTreasure = false;

        public void DrawTreasure()
        {
            if (!CanDrawTreasure) return;
            CanDrawTreasure = false;
            IsDrawingTreasure = true;
            StartCoroutine(DrawTreasureRoutine());
        }

        private System.Collections.IEnumerator DrawTreasureRoutine()
        {
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SwitchToTreasure();
                // Wait for the camera to actually swing over to the treasure deck
                yield return new WaitForSeconds(0.8f);
            }

            Transform treasureDeckSlot = transform.Find("TreasureDeckSlot");
            if (treasureDeckSlot == null) {
                IsDrawingTreasure = false;
                yield break;
            }

            GameObject tPrefab = cardPrefab;
            // Assuming Treasure Prefab was mapped here, or we load it directly
            GameObject tPrefabLoad = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardPrefab_Treasure.prefab");
            if (tPrefabLoad != null) tPrefab = tPrefabLoad;

            // Spawn at treasure deck face down
            Vector3 spawnPos = treasureDeckSlot.position + new Vector3(0, 0.05f, 0);
            Quaternion spawnRot = treasureDeckSlot.rotation * Quaternion.Euler(0, 180, 0);
            
            GameObject treasureCard = Instantiate(tPrefab, spawnPos, spawnRot, treasureDeckSlot);
            treasureCard.name = "Treasure_Active";

            // Pull from treasureDeck if available
            Cards.TreasureCardData tData = null;
            if (treasureDeck != null && treasureDeck.Count > 0)
            {
                tData = treasureDeck[0];
                treasureDeck.RemoveAt(0);
            }
            else
            {
                // Determine if it's gold or an item. For now, random test logic:
                Debug.LogWarning("Treasure Deck is empty! Generating random test treasure.");
                tData = ScriptableObject.CreateInstance<Cards.TreasureCardData>();
                tData.cardName = "Schatz";
                if (Random.value > 0.5f) {
                    tData.goldAmount = 2; // Gives 2 gold
                } else {
                    if (testItems != null && testItems.Length > 0)
                        tData.itemReward = testItems[Random.Range(0, testItems.Length)] as Cards.ItemCardData;
                    else
                        tData.goldAmount = 1;
                }
            }

            var display = treasureCard.GetComponent<Cards.CardDisplay>();
            if (display != null) display.Setup(tData);

            // Animate to activeTreasureSlot while staying on Treasure camera
            Transform targetSlot = activeTreasureSlot != null ? activeTreasureSlot : transform;
            
            if (Mechanics.CardAnimator.Instance != null)
            {
                yield return StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCardWithFlip(
                    treasureCard.transform, 
                    targetSlot.position, // Land exactly on slot
                    treasureDeckSlot.rotation,
                    targetSlot.rotation,
                    180f, 0f, 1.0f
                ));
            }
            else
            {
                treasureCard.transform.position = targetSlot.position;
                treasureCard.transform.rotation = targetSlot.rotation;
            }

            // Wait for player to click the treasure card
            var clickable = treasureCard.AddComponent<Mechanics.ClickableEventCard>();
            while (!clickable.isClicked)
            {
                yield return null;
            }
            Destroy(clickable);

            // Apply effect
            if (tData.goldAmount > 0)
            {
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.SwitchToPlayerView();
                }
                
                yield return new WaitForSeconds(0.5f);
                
                PlayerManager.Instance.AddGold(tData.goldAmount);
                
                yield return new WaitForSeconds(1.5f);
            }
            else if (tData.itemReward != null)
            {
                // In a real game, move it to the item slot. Here we just destroy the treasure wrapper.
                // For now, assume player gets the item logic later.
            }

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SwitchToEncounter();
            }

            // Dissolve or discard treasure card
            Destroy(treasureCard);
            
            IsDrawingTreasure = false;
        }

        private void ShuffleList<T>(System.Collections.Generic.List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
