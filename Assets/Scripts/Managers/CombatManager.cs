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
        
        public bool IsCombatRunning { get; private set; } = false;
        private bool hitResult = false;
        private int lastDamageDealt = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ResolveCombat(GameObject attackerCard, GameObject defenderCard)
        {
            if (IsCombatRunning) 
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
            IsCombatRunning = true;
            
            var memberDisplay = memberCard.GetComponent<Cards.CardDisplay>();
            var monsterDisplay = monsterCard.GetComponent<Cards.CardDisplay>();
            
            int pSpeed = memberDisplay != null ? memberDisplay.currentSpeed : member.baseSpeed;
            int mSpeed = monsterDisplay != null ? monsterDisplay.currentSpeed : monster.speed;
            
            int playerAttackCount = 1;
            int monsterAttackCount = 1;
            
            if (pSpeed > 0 && pSpeed >= mSpeed * 2)
                playerAttackCount = 2;
            else if (mSpeed > 0 && mSpeed >= pSpeed * 2)
                monsterAttackCount = 2;

            bool playerAttacksFirst;
            if (pSpeed > mSpeed)
                playerAttacksFirst = true;
            else if (pSpeed < mSpeed)
                playerAttacksFirst = false;
            else
                playerAttacksFirst = UnityEngine.Random.value > 0.5f; // Zufall bei Gleichstand

            if (playerAttacksFirst)
            {
                Debug.Log($"Kampf beginnt: {member.cardName} greift zuerst an (Speed: {pSpeed} vs {mSpeed}). Angriffe: {playerAttackCount}");
                
                for (int i = 0; i < playerAttackCount; i++)
                {
                    yield return StartCoroutine(HandlePlayerAttack(memberCard, member, monsterCard, monster));
                    if (monsterCard == null || monsterCard.GetComponent<Cards.CardDisplay>().currentHealth <= 0) 
                    {
                        IsCombatRunning = false;
                        yield break;
                    }
                }

                for (int i = 0; i < monsterAttackCount; i++)
                {
                    yield return StartCoroutine(HandleMonsterAttack(memberCard, member, monsterCard, monster));
                    if (memberCard == null || memberCard.GetComponent<Cards.CardDisplay>().currentHealth <= 0) 
                    {
                        IsCombatRunning = false;
                        yield break;
                    }
                }
            }
            else
            {
                Debug.Log($"Kampf beginnt: {monster.cardName} greift zuerst an (Speed: {mSpeed} vs {pSpeed}). Angriffe: {monsterAttackCount}");
                
                for (int i = 0; i < monsterAttackCount; i++)
                {
                    yield return StartCoroutine(HandleMonsterAttack(memberCard, member, monsterCard, monster));
                    if (memberCard == null || !memberCard.GetComponent<Collider>().enabled) 
                    {
                        IsCombatRunning = false;
                        yield break;
                    }
                }

                for (int i = 0; i < playerAttackCount; i++)
                {
                    yield return StartCoroutine(HandlePlayerAttack(memberCard, member, monsterCard, monster));
                    if (monsterCard == null || monsterCard.GetComponent<Cards.CardDisplay>().currentHealth <= 0) 
                    {
                        IsCombatRunning = false;
                        yield break;
                    }
                }
            }

            IsCombatRunning = false;
            // Bleibe in der ActionPhase, damit der Spieler weiter angreifen kann (wenn der State nicht schon Idle ist)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Idle) 
                GameManager.Instance.ChangeState(GameState.ActionPhase);
        }

        private IEnumerator HandlePlayerAttack(GameObject memberCard, Cards.MemberCardData member, GameObject monsterCard, Cards.MonsterCardData monster)
        {
            var monsterDisplay = monsterCard.GetComponent<Cards.CardDisplay>();
            var memberDisplay = memberCard.GetComponent<Cards.CardDisplay>();
            
            // The defense value is simply the current strength (which includes any previously earned permanent buffs)
            int defenseTarget = monsterDisplay.currentStrength;

            yield return StartCoroutine(ExecuteAttack(member.cardName, memberDisplay.currentStrength, monster.cardName, defenseTarget, true, monster));
            bool playerHit = hitResult;
            
            // Reset member strength back to base (removes the buff after the attack finishes)
            if (memberDisplay.currentStrength > member.baseStrength)
            {
                memberDisplay.currentStrength = member.baseStrength;
                memberDisplay.UpdateDisplay();
            }
            
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.ActionPhase);
            yield return new WaitForSeconds(1.5f); // Wait for camera to finish moving back to cards
            
            if (playerHit)
            {
                monsterDisplay.currentHealth -= lastDamageDealt;
                monsterDisplay.UpdateDisplay();
                Debug.Log($"{monster.cardName} nimmt {lastDamageDealt} Schaden! Verbleibend: {monsterDisplay.currentHealth}");
                
                yield return new WaitForSeconds(1.5f); 
                
                if (monsterDisplay.currentHealth <= 0)
                {
                    Debug.Log($"{monster.cardName} wurde besiegt!");
                    yield return new WaitForSeconds(0.5f);

                    if (RewardManager.Instance != null)
                    {
                        if (monster.rewardCoinInsteadOfHonor)
                        {
                            Mechanics.CoinManager.Instance.SpawnCoins(1);
                        }
                        else
                        {
                            RewardManager.Instance.SpawnHonorToken(monsterCard.transform.position + new Vector3(0, 0.5f, 0));
                        }
                    }
                    
                    yield return new WaitForSeconds(2.2f);
                    
                    var tableLayout = FindAnyObjectByType<TableLayoutManager>();
                    
                    if (monster.rewardTreasureOnDefeat > 0 && tableLayout != null)
                    {
                        CameraManager.Instance.SwitchToTreasure();
                        tableLayout.CanDrawTreasure = true;
                        
                        while (tableLayout.CanDrawTreasure) yield return null;
                        while (tableLayout.IsDrawingTreasure) yield return null;
                    }
                    
                    if (tableLayout != null && Mechanics.CardAnimator.Instance != null)
                    {
                        var interactable = monsterCard.GetComponent<Cards.CardInteractable>();
                        if (interactable != null) Destroy(interactable);
                        
                        var col = monsterCard.GetComponent<Collider>();
                        if (col != null) col.enabled = false;

                        Vector3 targetPos = tableLayout.GetNextDiscardPosition();
                        Quaternion randomOffset = Quaternion.Euler(0, 180f, UnityEngine.Random.Range(-7f, 7f));
                        Quaternion targetRot = tableLayout.discardSlot.rotation * randomOffset;
                        
                        yield return StartCoroutine(Mechanics.CardAnimator.Instance.AnimateCard(monsterCard.transform, targetPos, targetRot, 1.0f));
                        
                        tableLayout.AddToDiscardPile();
                        tableLayout.ClearCurrentEncounter();
                        
                        Canvas canvas = monsterCard.GetComponentInChildren<Canvas>();
                        if (canvas != null) canvas.enabled = false;
                        
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

                    if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Idle);
                }
            }
            
            // If the monster is still alive (either missed or survived the damage)
            if (monsterDisplay.currentHealth > 0)
            {
                bool triggeredDefenseEffect = false;
                
                if (monster.bonusStrengthOnDefense > 0)
                {
                    monsterDisplay.currentStrength += monster.bonusStrengthOnDefense;
                    triggeredDefenseEffect = true;
                }
                if (monster.healOnSuccessfulDefense > 0)
                {
                    monsterDisplay.currentHealth += monster.healOnSuccessfulDefense;
                    triggeredDefenseEffect = true;
                }
                if (monster.bonusSpeedOnSuccessfulDefense > 0)
                {
                    monsterDisplay.currentSpeed += monster.bonusSpeedOnSuccessfulDefense;
                    triggeredDefenseEffect = true;
                }
                
                if (triggeredDefenseEffect)
                {
                    monsterDisplay.UpdateDisplay();
                    yield return new WaitForSeconds(1.0f); // Brief pause to let the player notice the stat increases
                }
            }

            // Member Ability: bonusStrengthOnMiss
            // This ONLY triggers if the player missed the attack.
            if (!playerHit && member.bonusStrengthOnMiss > 0)
            {
                memberDisplay.currentStrength += member.bonusStrengthOnMiss;
                memberDisplay.UpdateDisplay();
                yield return new WaitForSeconds(1.0f); // Brief pause to let player notice the angry peasant's buff
            }
        }

        private IEnumerator HandleMonsterAttack(GameObject memberCard, Cards.MemberCardData member, GameObject monsterCard, Cards.MonsterCardData monster)
        {
            var monsterDisplay = monsterCard.GetComponent<Cards.CardDisplay>();
            var memberDisplay = memberCard.GetComponent<Cards.CardDisplay>();
            
            Debug.Log($"{monster.cardName} greift an!");
            
            if (monster.healOnAttack > 0)
            {
                if (monsterDisplay != null)
                {
                    monsterDisplay.currentHealth += monster.healOnAttack;
                    monsterDisplay.UpdateDisplay();
                    // Kein WaitForSeconds hier, damit die Würfel-Animation genauso schnell startet wie beim Spieler
                }
            }

            int mStrength = monsterDisplay != null ? monsterDisplay.currentStrength : monster.strength;
            int pStrength = memberDisplay != null ? memberDisplay.currentStrength : member.baseStrength;

            // Add equipment buffs that apply ONLY on defense
            int tempStrengthBuff = 0;
            if (memberDisplay != null)
            {
                foreach (var item in memberDisplay.equippedItems)
                {
                    if (item.onlyOnDefense)
                    {
                        tempStrengthBuff += item.strengthBuff;
                    }
                }

                if (tempStrengthBuff > 0)
                {
                    memberDisplay.currentStrength += tempStrengthBuff;
                    pStrength += tempStrengthBuff;
                    memberDisplay.UpdateDisplay();
                    memberDisplay.HighlightStrengthText();
                    // Kein WaitForSeconds hier, damit der Würfelwurf sofort startet (Highlight ist während des Wurfes sichtbar)
                }
            }

            yield return StartCoroutine(ExecuteAttack(monster.cardName, mStrength, member.cardName, pStrength, false, monster));
            bool monsterHit = hitResult;
            
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.ActionPhase);
            // Längere Spannungspause, bevor der Schaden tatsächlich "einschlägt"
            yield return new WaitForSeconds(3.5f); 

            if (monsterHit)
            {
                Debug.Log($"Der Spieler nimmt durch den Angriff von {monster.cardName} Schaden!");
                var hm = FindObjectOfType<HealthLightManager>();
                if (hm != null) hm.TakeDamage();
                
                // Große Pause NACH dem Einschlag, damit das Erdbeben und der Lichtausfall wirken können
                yield return new WaitForSeconds(5.0f); 
            }

            // Revert temporary visual buffs
            if (memberDisplay != null && tempStrengthBuff > 0)
            {
                memberDisplay.currentStrength -= tempStrengthBuff;
                memberDisplay.UpdateDisplay();
            }
        }
        
        private IEnumerator ExecuteAttack(string attackerName, int attackerStrength, string defenderName, int defenderDefense, bool isPlayerAttacking, Cards.MonsterCardData monsterData = null)
        {
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Combat);
            
            hitResult = false;
            
            if (dicePrefab == null)
            {
                int fallbackRoll = Random.Range(1, 7);
                int fallbackAttack = fallbackRoll + attackerStrength;
                if (fallbackAttack == defenderDefense && monsterData != null && monsterData.winsTies)
                {
                    hitResult = !isPlayerAttacking;
                }
                else
                {
                    hitResult = fallbackAttack > defenderDefense;
                }
                lastDamageDealt = hitResult ? fallbackRoll : 0;
                yield break;
            }

            Vector3 centerPos = Vector3.zero;
            var tableLayout = FindAnyObjectByType<TableLayoutManager>();
            if (tableLayout != null && tableLayout.encounterSlot != null)
            {
                centerPos = tableLayout.encounterSlot.position;
            }

            GameObject dice = Instantiate(dicePrefab);
            MeshRenderer renderer = dice.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                string matName = isPlayerAttacking ? "DicePlayerMat" : "DiceMonsterMat";
                Material diceMat = Resources.Load<Material>(matName);
                if (diceMat != null) renderer.material = diceMat;
            }

            Mechanics.DiceRoller roller = dice.GetComponent<Mechanics.DiceRoller>();
            roller.isPlayerAttacking = isPlayerAttacking;
            
            Vector3 spawnPos = centerPos + new Vector3(isPlayerAttacking ? -1.5f : 1.5f, 0.8f, -1.0f);
            Vector3 throwDir = new Vector3(isPlayerAttacking ? 1f : -1f, -0.2f, 1f).normalized;
            
            var diceCamObj = GameObject.Find("CM_DiceRollView");
            if (diceCamObj != null)
            {
                var vcam = diceCamObj.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (vcam != null) vcam.LookAt = dice.transform;
            }

            roller.Roll(spawnPos, throwDir, 3f);
            
            while (roller.IsRolling()) yield return null;
            
            int roll = roller.GetResult();
            int totalAttack = roll + attackerStrength;

            if (totalAttack == defenderDefense && monsterData != null && monsterData.winsTies)
            {
                hitResult = !isPlayerAttacking;
                Debug.Log($"Gleichstand! {monsterData.cardName} gewinnt dank Fähigkeit.");
            }
            else
            {
                hitResult = totalAttack > defenderDefense;
            }
            
            lastDamageDealt = hitResult ? (totalAttack - defenderDefense) : 0;
            
            yield return StartCoroutine(roller.ShowResultTextRoutine(hitResult));
            
            yield return new WaitForSeconds(1.5f);
            Destroy(dice);
        }


    }
}
