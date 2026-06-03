using UnityEngine;

namespace AdventureCardGame.Mechanics
{
    public class ClickableDeck : MonoBehaviour
    {
        public void OnClick()
        {
            if (Managers.GameManager.Instance != null && Managers.GameManager.Instance.CurrentState == Managers.GameState.Idle)
            {
                var table = FindAnyObjectByType<Managers.TableLayoutManager>();
                if (table != null)
                {
                    table.DrawEncounter();
                }
            }
        }
    }
}
