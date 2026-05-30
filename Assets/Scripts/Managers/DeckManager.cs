using UnityEngine;
using AdventureCardGame.Cards;
using System.Collections.Generic;

namespace AdventureCardGame.Managers
{
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

        public List<CardData> CurrentDeck = new List<CardData>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        // Method to construct deck based on Phase configuration
        // e.g. BuildPhase(PhaseData data)
    }
}
