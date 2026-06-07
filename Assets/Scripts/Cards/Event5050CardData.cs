using UnityEngine;
using System.Collections;
using AdventureCardGame.Managers;

namespace AdventureCardGame.Cards
{
    [CreateAssetMenu(fileName = "Event_50_50", menuName = "Cards/Events/50_50")]
    public class Event5050CardData : EventCardData
    {
        public override IEnumerator ExecuteEvent(GameObject eventCardObject)
        {
            // Add clickable component
            var clickable = eventCardObject.AddComponent<AdventureCardGame.Mechanics.ClickableEventCard>();
            
            // Wait until clicked
            while (!clickable.isClicked)
            {
                yield return null;
            }
            
            Destroy(clickable);

            // Short pause before rolling
            yield return new WaitForSeconds(0.5f);

            // Roll blue dice (player dice)
            int diceResult = 1; // Default
            
            if (CombatManager.Instance != null && CombatManager.Instance.dicePrefab != null)
            {
                // Spawn Dice
                GameObject dice = Instantiate(CombatManager.Instance.dicePrefab);
                
                // Color the dice blue (player)
                MeshRenderer renderer = dice.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material diceMat = Resources.Load<Material>("DicePlayerMat");
                    if (diceMat != null) renderer.material = diceMat;
                }

                AdventureCardGame.Mechanics.DiceRoller roller = dice.GetComponent<AdventureCardGame.Mechanics.DiceRoller>();
                
                Vector3 centerPos = Vector3.zero;
                var tableLayout = FindAnyObjectByType<TableLayoutManager>();
                if (tableLayout != null && tableLayout.encounterSlot != null) centerPos = tableLayout.encounterSlot.position;

                Vector3 spawnPos = centerPos + new Vector3(-1.5f, 0.8f, -1.0f);
                Vector3 throwDir = (centerPos - spawnPos).normalized + Vector3.up * 0.5f;
                
                // Track dice with camera
                if (CameraManager.Instance != null && CameraManager.Instance.camDiceRoll != null)
                {
                    CameraManager.Instance.camDiceRoll.LookAt = dice.transform;
                    CameraManager.Instance.SwitchToDiceRoll();
                }
                
                roller.Roll(spawnPos, throwDir, 5f);
                
                // Wait for the roll to finish
                while (roller.IsRolling() || roller.GetResult() == 0)
                {
                    yield return null;
                }
                
                diceResult = roller.GetResult();
                Destroy(dice, 1.5f); // Clean up dice
                
                if (CameraManager.Instance != null && CameraManager.Instance.camDiceRoll != null)
                {
                    CameraManager.Instance.camDiceRoll.LookAt = null;
                }
            }

            // Wait a moment to see the result
            yield return new WaitForSeconds(1.0f);

            // Execute logic
            if (diceResult >= 1 && diceResult <= 3)
            {
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.SwitchToPlayerView();
                }
                
                yield return new WaitForSeconds(1.0f);
                
                // Lose 2 coins
                PlayerManager.Instance.AddGold(-2);
                
                yield return new WaitForSeconds(1.5f);
                
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.SwitchToEncounter();
                }
            }
            else if (diceResult >= 4 && diceResult <= 6)
            {
                // Switch camera to the treasure deck so the player knows to draw
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.SwitchToTreasure();
                }

                // Draw treasure manually
                var tlm = FindAnyObjectByType<TableLayoutManager>();
                if (tlm != null)
                {
                    tlm.CanDrawTreasure = true;
                    // Wait until player clicks the treasure deck to start the draw
                    while (tlm.CanDrawTreasure)
                    {
                        yield return null;
                    }
                    
                    // Wait until the complete treasure sequence (draw, click, apply effect) is finished
                    while (tlm.IsDrawingTreasure)
                    {
                        yield return null;
                    }
                }
            }
            
            // Wait a bit more before discarding event
            yield return new WaitForSeconds(1.5f);
        }
    }
}
