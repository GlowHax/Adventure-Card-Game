using UnityEngine;
using AdventureCardGame.Managers;

namespace AdventureCardGame.Mechanics
{
    public class ClickableTreasureDeck : MonoBehaviour
    {
        public void OnPointerDown()
        {
            OnClick();
        }

        public void OnClick()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Event)
            {
                var table = FindAnyObjectByType<TableLayoutManager>();
                if (table != null && table.CanDrawTreasure)
                {
                    table.DrawTreasure();
                }
            }
        }
    }
}
