using UnityEngine;

namespace AdventureCardGame.Cards
{
    public enum EventType
    {
        Reward,
        Penalty,
        Special
    }

    [CreateAssetMenu(fileName = "New Event", menuName = "Cards/Event")]
    public class EventCardData : CardData
    {
        public EventType eventType;
        
        // Base method for event logic. Derived classes should override this.
        public virtual System.Collections.IEnumerator ExecuteEvent(GameObject eventCardObject)
        {
            // Default behavior: just wait 2 seconds and finish
            yield return new WaitForSeconds(2f);
        }
    }
}
