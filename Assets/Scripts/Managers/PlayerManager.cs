using UnityEngine;
using AdventureCardGame.Cards;
using System.Collections.Generic;

namespace AdventureCardGame.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        public int Gold { get; private set; }
        public int Honor { get; private set; }

        // Max 3 members
        public List<MemberCardData> ActiveMembers = new List<MemberCardData>();
        
        // Max 3 general objects
        public List<ObjectCardData> GeneralObjects = new List<ObjectCardData>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            if (Gold < 0) Gold = 0; // Prevent negative gold
            
            if (AdventureCardGame.Mechanics.CoinManager.Instance != null)
            {
                if (amount > 0) AdventureCardGame.Mechanics.CoinManager.Instance.SpawnCoins(amount);
                else if (amount < 0) AdventureCardGame.Mechanics.CoinManager.Instance.RemoveCoins(-amount);
            }
        }
        public void AddHonor(int amount) => Honor += amount;
        
        // Methods for managing members and items
    }
}
