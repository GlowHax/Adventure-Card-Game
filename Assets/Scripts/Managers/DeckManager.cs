using System.Collections.Generic;
using UnityEngine;
using AdventureCardGame.Cards;

namespace AdventureCardGame.Managers
{
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

        public Stack<CardData> currentDeck = new Stack<CardData>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Generiert den Stapel basierend auf den Phasen-Daten.
        /// </summary>
        public void GenerateDeck(DungeonPhaseData phaseData)
        {
            currentDeck.Clear();
            List<CardData> finalDeckBuilder = new List<CardData>();

            // Wir gehen die Sub-Stacks in der Reihenfolge durch.
            // Der erste Eintrag in subStacks soll ganz UNTEN liegen, 
            // also fügen wir ihn zuerst zur Liste hinzu (Index 0).
            foreach (var subStack in phaseData.subStacks)
            {
                List<CardData> tempStack = new List<CardData>();

                foreach (var poolConfig in subStack.cardPools)
                {
                    // 1. Spezifische Karten hinzufügen
                    if (poolConfig.specificCards != null)
                    {
                        tempStack.AddRange(poolConfig.specificCards);
                    }

                    // 2. Zufällige Karten aus dem Pool ziehen
                    if (poolConfig.randomPool != null && poolConfig.randomDrawCount > 0)
                    {
                        List<CardData> poolCopy = new List<CardData>(poolConfig.randomPool);
                        int count = Mathf.Min(poolConfig.randomDrawCount, poolCopy.Count);
                        
                        for (int i = 0; i < count; i++)
                        {
                            int randomIndex = Random.Range(0, poolCopy.Count);
                            tempStack.Add(poolCopy[randomIndex]);
                            poolCopy.RemoveAt(randomIndex);
                        }
                    }
                }

                // Teil-Stapel mischen falls gewünscht
                if (subStack.shuffle)
                {
                    ShuffleList(tempStack);
                }

                // Auf den Gesamt-Stapel legen (vorherige Karten rücken nach 'unten')
                finalDeckBuilder.AddRange(tempStack);
            }

            // finalDeckBuilder hat jetzt Index 0 = Unterste Karte, letzter Index = Oberste Karte.
            // Wenn wir sie in einen Stack pushen, müssen wir von 0 bis Ende gehen.
            foreach (var card in finalDeckBuilder)
            {
                currentDeck.Push(card);
            }

            Debug.Log($"Deck für Phase '{phaseData.phaseName}' generiert! Insgesamt {currentDeck.Count} Karten.");
        }

        public CardData DrawCard()
        {
            if (currentDeck.Count > 0)
            {
                return currentDeck.Pop();
            }
            return null;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
