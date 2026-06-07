using UnityEngine;

namespace AdventureCardGame.Mechanics
{
    public class ClickableEventCard : MonoBehaviour
    {
        public bool isClicked = false;

        public void OnPointerDown()
        {
            OnClick();
        }

        public void OnClick()
        {
            isClicked = true;
        }
    }
}
