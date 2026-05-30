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
        // Event logic handler will interpret the event based on its type or custom logic ID
        public string eventLogicID;
    }
}
