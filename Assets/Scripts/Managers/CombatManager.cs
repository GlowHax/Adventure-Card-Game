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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ResolveCombat(GameObject attackerCard, GameObject defenderCard)
        {
            if (isCombatRunning) return;
            
            var attackerDisplay = attackerCard.GetComponent<CardDisplay>();
            var defenderDisplay = defenderCard.GetComponent<CardDisplay>();
            
            if (attackerDisplay == null || defenderDisplay == null) return;
            
            if (attackerDisplay.cardData is MemberCardData member && defenderDisplay.cardData is MonsterCardData monster)
            {
                if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Combat);
                StartCoroutine(CombatRoutine(attackerCard, member, defenderCard, monster));
            }
        }

        private IEnumerator CombatRoutine(GameObject memberCard, MemberCardData member, GameObject monsterCard, MonsterCardData monster)
        {
            isCombatRunning = true;
            Debug.Log($"Kampf beginnt: {member.cardName} greift {monster.cardName} an!");

            // 1. Spieler greift an
            yield return StartCoroutine(ExecuteAttack(member.cardName, member.baseStrength, monster.cardName, monster.strength, true));
            bool playerHit = hitResult;
            
            if (playerHit)
            {
                var monsterDisplay = monsterCard.GetComponent<CardDisplay>();
                monsterDisplay.currentHealth -= 1;
                monsterDisplay.UpdateDisplay();
                
                Debug.Log($"{monster.cardName} nimmt 1 Schaden! Verbleibend: {monsterDisplay.currentHealth}");
                
                if (monsterDisplay.currentHealth <= 0)
                {
                    Debug.Log($"{monster.cardName} wurde besiegt!");
                    Destroy(monsterCard);
                    isCombatRunning = false;
                    if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Idle);
                    yield break;
                }
            }

            // 2. Monster schlägt zurück
            Debug.Log($"{monster.cardName} schlägt zurück!");
            yield return StartCoroutine(ExecuteAttack(monster.cardName, monster.strength, member.cardName, member.baseStrength, false));
            bool monsterHit = hitResult;

            if (monsterHit)
            {
                Debug.Log($"{member.cardName} wurde besiegt!");
                KillMember(memberCard);
            }

            isCombatRunning = false;
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Idle);
        }
        
        private bool hitResult = false;

        private IEnumerator ExecuteAttack(string attackerName, int attackerStrength, string defenderName, int defenderDefense, bool isPlayerAttacking)
        {
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
            DiceRoller roller = dice.GetComponent<DiceRoller>();
            
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
                Debug.Log("-> Treffer!");
                hitResult = true;
            }
            else
            {
                Debug.Log("-> Verfehlt / Abgewehrt!");
                hitResult = false;
            }
            
            // Clean up dice after 2 seconds
            Destroy(dice, 2f);
            yield return new WaitForSeconds(2f);
        }

        private void KillMember(GameObject memberCard)
        {
            // Flip the card over
            memberCard.transform.rotation = memberCard.transform.parent.rotation * Quaternion.Euler(180, 0, 0);
            
            // Disable interaction
            Collider col = memberCard.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            // Disable Canvas so it doesn't show through the back
            Canvas canvas = memberCard.GetComponentInChildren<Canvas>();
            if (canvas != null) canvas.enabled = false;
            
            // TODO: Optional: Add a skull icon on the back
        }
    }
}
