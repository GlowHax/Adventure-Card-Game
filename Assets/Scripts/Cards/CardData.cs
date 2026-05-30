using UnityEngine;

namespace AdventureCardGame.Cards
{
    public abstract class CardData : ScriptableObject
    {
        public string cardName;
        [TextArea(3, 5)]
        public string description;
        public Sprite artwork;
    }
}
