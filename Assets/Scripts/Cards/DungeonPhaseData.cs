using System.Collections.Generic;
using UnityEngine;
using AdventureCardGame.Cards;

namespace AdventureCardGame.Managers
{
    [System.Serializable]
    public class CardPoolConfig
    {
        public CardData[] specificCards; // Wenn bestimmte Karten zwingend rein sollen
        public List<CardData> randomPool; // Pool aus dem zufällig gezogen wird
        public int randomDrawCount; // Wie viele aus dem randomPool gezogen werden
    }

    [System.Serializable]
    public class SubStackConfig
    {
        public string subStackName;
        public List<CardPoolConfig> cardPools; // Aus diesen Pools wird dieser Teil-Stapel gebildet
        public bool shuffle = true; // Sollen die Karten dieses Teil-Stapels untereinander gemischt werden?
    }

    [CreateAssetMenu(fileName = "NewDungeonPhase", menuName = "AdventureCardGame/Dungeon Phase")]
    public class DungeonPhaseData : ScriptableObject
    {
        public string phaseName;
        [Tooltip("Der erste Eintrag liegt ganz UNTEN im Stapel, der letzte Eintrag liegt ganz OBEN auf dem Deck.")]
        public List<SubStackConfig> subStacks = new List<SubStackConfig>();
    }
}
