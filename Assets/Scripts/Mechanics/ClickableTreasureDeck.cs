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
            var table = FindAnyObjectByType<TableLayoutManager>();
            if (table != null && table.CanDrawTreasure)
            {
                table.DrawTreasure();
            }
        }
    }
}
