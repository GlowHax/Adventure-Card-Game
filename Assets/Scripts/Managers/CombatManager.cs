using UnityEngine;
using AdventureCardGame.Cards;

namespace AdventureCardGame.Managers
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void ResolveCombat(MonsterCardData monster)
        {
            // Combat logic:
            // 1. Compare speeds
            // 2. Roll dice
            // 3. Apply damage
            // 4. Handle member death (flip to B/W instead of destroy)
        }
    }
}
