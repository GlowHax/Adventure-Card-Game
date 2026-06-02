using UnityEngine;

namespace AdventureCardGame.Cards
{
    [RequireComponent(typeof(BoxCollider))]
    public class CardInteractable : MonoBehaviour
    {
        private CardDisplay display;

        private void Awake()
        {
            display = GetComponent<CardDisplay>();
        }

        public void OnClick()
        {
            if (display != null && display.cardData != null)
            {
                // Wenn wir auf dem verdeckten Stapel liegen (Canvas aus), nicht inspizieren!
                Canvas c = GetComponentInChildren<Canvas>();
                if (c != null && !c.enabled) return;

                if (Managers.InspectionManager.Instance != null)
                {
                    Managers.InspectionManager.Instance.InspectCard(this.gameObject);
                }
            }
        }
    }
}
