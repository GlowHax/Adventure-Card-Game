using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdventureCardGame.Managers;

namespace AdventureCardGame.Mechanics
{
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }

        public GameObject coinPrefab;
        public Transform coinBoxTransform;

        private List<GameObject> activeCoins = new List<GameObject>();

        public int CurrentCoins => activeCoins.Count;
        public static event System.Action<int> OnCoinsChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SpawnCoins(int amount)
        {
            if (coinPrefab == null || coinBoxTransform == null) return;

            StartCoroutine(SpawnCoinsRoutine(amount));
        }

        private IEnumerator SpawnCoinsRoutine(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                // Spawn above the box
                Vector3 spawnPos = coinBoxTransform.position + new Vector3(Random.Range(-0.2f, 0.2f), 1f, Random.Range(-0.2f, 0.2f));
                GameObject coin = Instantiate(coinPrefab, spawnPos, Random.rotation);
                activeCoins.Add(coin);
                OnCoinsChanged?.Invoke(CurrentCoins);
                yield return new WaitForSeconds(0.1f); // Stagger spawns
            }
        }

        public void RemoveCoins(int amount)
        {
            StartCoroutine(RemoveCoinsRoutine(amount));
        }

        private IEnumerator RemoveCoinsRoutine(int amount)
        {
            int removed = 0;
            for (int i = activeCoins.Count - 1; i >= 0 && removed < amount; i--)
            {
                GameObject coin = activeCoins[i];
                if (coin != null)
                {
                    activeCoins.RemoveAt(i);
                    removed++;
                    OnCoinsChanged?.Invoke(CurrentCoins);
                    // Flash and dissolve effect
                    StartCoroutine(FlashAndDissolve(coin));
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }

        private IEnumerator FlashAndDissolve(GameObject coin)
        {
            // Pop up and shrink effect
            float t = 0;
            Vector3 origScale = coin.transform.localScale;
            Vector3 popScale = origScale * 1.5f;
            
            // Pop up
            while (t < 1f)
            {
                t += Time.deltaTime * 10f; // Fast pop
                coin.transform.localScale = Vector3.Lerp(origScale, popScale, t);
                yield return null;
            }
            
            // Shrink down
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f; // Slower shrink
                coin.transform.localScale = Vector3.Lerp(popScale, Vector3.zero, t);
                yield return null;
            }
            
            Destroy(coin);
        }
    }
}
