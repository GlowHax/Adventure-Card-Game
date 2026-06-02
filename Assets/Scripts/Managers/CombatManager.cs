using UnityEngine;
using AdventureCardGame.Cards;

namespace AdventureCardGame.Managers
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ResolveCombat(MemberCardData playerMember, MonsterCardData monster)
        {
            Debug.Log($"Kampf beginnt: {playerMember.cardName} vs {monster.cardName}");

            bool playerGoesFirst = playerMember.baseSpeed >= monster.speed;

            if (playerGoesFirst)
            {
                // Spieler greift an
                if (ExecuteAttack(playerMember.cardName, playerMember.baseStrength, monster.cardName, monster.strength))
                {
                    Debug.Log($"{monster.cardName} wurde besiegt!");
                    return; // Kampf vorbei
                }

                // Monster überlebt und greift zurück an
                if (ExecuteAttack(monster.cardName, monster.strength, playerMember.cardName, playerMember.baseStrength))
                {
                    Debug.Log($"{playerMember.cardName} wurde besiegt!");
                }
            }
            else
            {
                // Monster greift zuerst an
                if (ExecuteAttack(monster.cardName, monster.strength, playerMember.cardName, playerMember.baseStrength))
                {
                    Debug.Log($"{playerMember.cardName} wurde besiegt!");
                    return;
                }

                // Spieler überlebt und greift zurück an
                if (ExecuteAttack(playerMember.cardName, playerMember.baseStrength, monster.cardName, monster.strength))
                {
                    Debug.Log($"{monster.cardName} wurde besiegt!");
                }
            }
        }

        private bool ExecuteAttack(string attackerName, int attackerStrength, string defenderName, int defenderDefense)
        {
            int roll = Random.Range(1, 7); // W6
            int totalAttack = roll + attackerStrength;

            Debug.Log($"{attackerName} greift {defenderName} an! Wurf: {roll} + Stärke: {attackerStrength} = {totalAttack} (Ziel-Verteidigung: {defenderDefense})");

            if (totalAttack > defenderDefense)
            {
                Debug.Log("-> Treffer!");
                return true;
            }
            else
            {
                Debug.Log("-> Verfehlt / Abgewehrt!");
                return false;
            }
        }
    }
}
