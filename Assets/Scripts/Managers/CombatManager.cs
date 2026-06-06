using System.Collections;
using UnityEngine;
using AdventureCardGame.Cards;
using AdventureCardGame.Mechanics;

namespace AdventureCardGame.Managers
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }
        
        [Header("Prefabs")]
        public GameObject dicePrefab;
        
        private bool isCombatRunning = false;
        private bool hitResult = false;
        private int lastDamageDealt = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ResolveCombat(GameObject attackerCard, GameObject defenderCard)
        {
            if (isCombatRunning) 
            {
                Debug.LogWarning("Combat is already running, attack ignored.");
                return;
            }
            
            var attackerDisplay = attackerCard.GetComponent<Cards.CardDisplay>();
            var defenderDisplay = defenderCard.GetComponent<Cards.CardDisplay>();
            
            if (attackerDisplay == null || defenderDisplay == null) return;
            
            if (attackerDisplay.cardData is Cards.MemberCardData member && defenderDisplay.cardData is Cards.MonsterCardData monster)
            {
                if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Combat);
                StartCoroutine(CombatRoutine(attackerCard, member, defenderCard, monster));
            }
        }

        private IEnumerator CombatRoutine(GameObject memberCard, Cards.MemberCardData member, GameObject monsterCard, Cards.MonsterCardData monster)
        {
            isCombatRunning = true;
            
            bool playerAttacksFirst;
            if (member.baseSpeed > monster.speed)
                playerAttacksFirst = true;
            else if (member.baseSpeed < monster.speed)
                playerAttacksFirst = false;
            else
                playerAttacksFirst = UnityEngine.Random.value > 0.5f; // Zufall bei Gleichstand

            if (playerAttacksFirst)
            {
                Debug.Log($"Kampf beginnt: {member.cardName} greift zuerst an (Speed: {member.baseSpeed} vs {monster.speed})");
                yield return StartCoroutine(HandlePlayerAttack(memberCard, member, monsterCard, monster));
                
                if (monsterCard == null || monsterCard.GetComponent<Cards.CardDisplay>().currentHealth <= 0) 
                {
                    isCombatRunning = false;
                    yield break;
                }

                yield return StartCoroutine(HandleMonsterAttack(memberCard, member, monsterCard, monster));
            }
            else
            {
                Debug.Log($"Kampf beginnt: {monster.cardName} greift zuerst an (Speed: {monster.speed} vs {member.baseSpeed})");
                yield return StartCoroutine(HandleMonsterAttack(memberCard, member, monsterCard, monster));
                
                if (memberCard == null || !memberCard.GetComponent<Collider>().enabled) 
                {
                    isCombatRunning = false;
                    yield break;
                }

                yield return StartCoroutine(HandlePlayerAttack(memberCard, member, monsterCard, monster));
                
                if (monsterCard == null || monsterCard.GetComponent<Cards.CardDisplay>().currentHealth <= 0) 
                {
                    isCombatRunning = false;
                    yield break;
                }
            }

            isCombatRunning = false;
            // Bleibe in der ActionPhase, damit der Spieler weiter angreifen kann (wenn der State nicht schon Idle ist)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Idle) 
                GameManager.Instance.ChangeState(GameState.ActionPhase);
        }

        private IEnumerator HandlePlayerAttack(GameObject memberCard, Cards.MemberCardData member, GameObject monsterCard, Cards.MonsterCardData monster)
        {
            yield return StartCoroutine(ExecuteAttack(member.cardName, member.baseStrength, monster.cardName, monster.strength, true));
            bool playerHit = hitResult;
            
            // Zurück zur Combat-Kamera, um das Ergebnis zu sehen!
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.ActionPhase);
            yield return new WaitForSeconds(1.5f); // Kamerafahrt abwarten
            
            if (playerHit)
            {
                var monsterDisplay = monsterCard.GetComponent<Cards.CardDisplay>();
                monsterDisplay.currentHealth -= lastDamageDealt;
                monsterDisplay.UpdateDisplay();
                
                Debug.Log($"{monster.cardName} nimmt {lastDamageDealt} Schaden! Verbleibend: {monsterDisplay.currentHealth}");
                
                yield return new WaitForSeconds(1.5f); // Dem Spieler Zeit geben, den Schaden zu sehen
                
                if (monsterDisplay.currentHealth <= 0)
                {
                    Debug.Log($"{monster.cardName} wurde besiegt!");
                    
                    // Kurze Pause einlegen, bevor die Token-Animation startet, damit man das Ergebnis wahrnehmen kann
                    yield return new WaitForSeconds(0.5f);

                    if (RewardManager.Instance != null)
                    {
                        // Token von der Mitte der Monsterkarte spawnen lassen
                        RewardManager.Instance.SpawnHonorToken(monsterCard.transform.position + new Vector3(0, 0.5f, 0));
                    }
                    
                    // Warten, bis das Token langsam geflogen ist (Animation dauert ca 2s)
                    yield return new WaitForSeconds(2.2f);
                    
                    var tableLayout = FindAnyObjectByType<TableLayoutManager>();
                    if (tableLayout != null && Mechanics.CardAnimator.Instance != null)
                    {
                        var interactable = monsterCard.GetComponent<Cards.CardInteractable>();
                        if (interactable != null) Destroy(interactable);
                        
                        var col = monsterCard.GetComponent<Collider>();
                        if (col != null) col.enabled = false;

                        Vector3 targetPos = tableLayout.GetNextDiscardPosition();
                        // Add slight random rotation around Z to make the discard pile look messy on the table
                        Quaternion randomOffset = Quaternion.Euler(0, 180f, UnityEngine.Random.Range(-7f, 7f));
                        Quaternion targetRot = tableLayout.discardSlot.rotation * randomOffset;
                        
                        yield return StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCard(monsterCard.transform, targetPos, targetRot, 1.0f));
                        
                        tableLayout.AddToDiscardPile();
                        tableLayout.ClearCurrentEncounter();
                        
                        Canvas canvas = monsterCard.GetComponentInChildren<Canvas>();
                        if (canvas != null) canvas.enabled = false;
                        
                        // Destroy the card if we already have enough cards visually in the discard pile
                        if (tableLayout.discardedCardsCount > 4)
                        {
                            Destroy(monsterCard);
                        }
                        else
                        {
                            monsterCard.transform.SetParent(tableLayout.discardSlot);
                        }
                    }
                    else
                    {
                        Destroy(monsterCard);
                    }

                    if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Idle); // Zurück zum Encounter View
                }
            }
        }

        private IEnumerator HandleMonsterAttack(GameObject memberCard, Cards.MemberCardData member, GameObject monsterCard, Cards.MonsterCardData monster)
        {
            Debug.Log($"{monster.cardName} greift an!");
            yield return StartCoroutine(ExecuteAttack(monster.cardName, monster.strength, member.cardName, member.baseStrength, false));
            bool monsterHit = hitResult;
            
            // Zurück zur Combat-Kamera
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.ActionPhase);
            yield return new WaitForSeconds(2.5f); // Länger warten, damit die Kamerafahrt (1s) sicher beendet ist und der Spieler die Karte noch sieht

            if (monsterHit)
            {
                Debug.Log($"{member.cardName} wurde besiegt!");
                KillMember(memberCard);
                yield return new WaitForSeconds(2.0f); // Dem Spieler Zeit geben, den Verlust zu sehen
            }
        }
        private IEnumerator ExecuteAttack(string attackerName, int attackerStrength, string defenderName, int defenderDefense, bool isPlayerAttacking)
        {
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Combat);
            
            hitResult = false;
            
            if (dicePrefab == null)
            {
                // Fallback to random if no dice prefab
                int fallbackRoll = Random.Range(1, 7);
                hitResult = (fallbackRoll + attackerStrength) > defenderDefense;
                yield break;
            }

            // Find table center
            Vector3 centerPos = Vector3.zero;
            var tableLayout = FindAnyObjectByType<TableLayoutManager>();
            if (tableLayout != null && tableLayout.encounterSlot != null)
            {
                centerPos = tableLayout.encounterSlot.position;
            }

            // Spawn Dice
            GameObject dice = Instantiate(dicePrefab);
            
            // Color the dice using pre-baked materials from Resources
            MeshRenderer renderer = dice.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                string matName = isPlayerAttacking ? "DicePlayerMat" : "DiceMonsterMat";
                Material diceMat = Resources.Load<Material>(matName);
                if (diceMat != null)
                {
                    renderer.material = diceMat;
                }
            }

            Mechanics.DiceRoller roller = dice.GetComponent<Mechanics.DiceRoller>();
            
            // Spawn offset from center and throw towards center
            Vector3 spawnPos = centerPos + new Vector3(isPlayerAttacking ? -1.5f : 1.5f, 0.8f, -1.0f);
            Vector3 throwDir = new Vector3(isPlayerAttacking ? 1f : -1f, -0.2f, 1f).normalized;
            
            // Make the Dice Camera track the dice!
            var diceCamObj = GameObject.Find("CM_DiceRollView");
            if (diceCamObj != null)
            {
                var vcam = diceCamObj.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (vcam != null) vcam.LookAt = dice.transform;
            }

            roller.Roll(spawnPos, throwDir, 3f);
            
            while (roller.IsRolling())
            {
                yield return null;
            }
            
            int roll = roller.GetResult();
            int totalAttack = roll + attackerStrength;

            Debug.Log($"{attackerName} greift an! Würfel: {roll} + Stärke: {attackerStrength} = {totalAttack} (Ziel: {defenderDefense})");

            if (totalAttack > defenderDefense)
            {
                lastDamageDealt = totalAttack - defenderDefense;
                Debug.Log($"-> Treffer! Schaden: {lastDamageDealt}");
                hitResult = true;
            }
            else
            {
                lastDamageDealt = 0;
                Debug.Log("-> Verfehlt / Abgewehrt!");
                hitResult = false;
            }
            
            // Clean up dice after a short pause
            Destroy(dice, 1.5f);
            yield return new WaitForSeconds(1.5f);
        }

        private void KillMember(GameObject memberCard)
        {
            // Disable interaction
            Collider col = memberCard.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Animate Flip
            if (Mechanics.CardAnimator.Instance != null)
            {
                StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCardWithFlip(
                    memberCard.transform,
                    memberCard.transform.position, // stay in place
                    memberCard.transform.parent.rotation,
                    memberCard.transform.parent.rotation,
                    0f,
                    180f,
                    1.0f
                ));
            }
            else
            {
                memberCard.transform.rotation = memberCard.transform.parent.rotation * Quaternion.Euler(0, 180, 0);
            }
            
            // Disable Canvas slightly delayed so it doesn't disappear instantly before the flip hides it.
            // Flip takes 1.0f, so at 0.5f the card is exactly 90 degrees turned (invisible edge-on).
            StartCoroutine(DisableCanvasDelayed(memberCard, 0.5f));
        }

        private IEnumerator DisableCanvasDelayed(GameObject memberCard, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (memberCard != null)
            {
                Canvas canvas = memberCard.GetComponentInChildren<Canvas>();
                if (canvas != null) canvas.enabled = false;
            }
        }
    }
}
