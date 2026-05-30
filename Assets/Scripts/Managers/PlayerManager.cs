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

        public void AddGold(int amount) => Gold += amount;
        public void AddHonor(int amount) => Honor += amount;
        
        // Methods for managing members and items
    }
}
