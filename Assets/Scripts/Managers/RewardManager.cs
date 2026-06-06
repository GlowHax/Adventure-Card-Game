using UnityEngine;

namespace AdventureCardGame.Managers
{
    public class RewardManager : MonoBehaviour
    {
        public static RewardManager Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject honorTokenPrefab;

        [Header("Spawn Settings")]
        public Transform honorBowl; // Reference to the wooden bowl
        public Vector3 spawnOffset = new Vector3(0, 0.3f, 0); // Spawns just 0.3 units above the bowl

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Auto-find references if not set
            if (honorBowl == null)
            {
                var bowlObj = GameObject.Find("HonorBowl");
                if (bowlObj != null) honorBowl = bowlObj.transform;
            }

            if (honorTokenPrefab == null)
            {
                honorTokenPrefab = Resources.Load<GameObject>("HonorToken"); // If we placed it in Resources
                if (honorTokenPrefab == null)
                {
                    // Fallback to searching the project using AssetDatabase is not possible at runtime,
                    // so the prefab must be assigned in the inspector or placed in a Resources folder.
                    // Let's assume we assign it via a small MCP script after creating this.
                }
            }
        }

        public void SpawnHonorToken(Vector3 startPos)
        {
            if (honorTokenPrefab == null)
            {
                Debug.LogWarning("HonorTokenPrefab is not assigned in RewardManager!");
                return;
            }

            Vector3 targetPos = Vector3.zero;
            if (honorBowl != null)
            {
                targetPos = honorBowl.position + spawnOffset;
            }
            else
            {
                // Fallback position if bowl is missing
                targetPos = new Vector3(5.5f, 4.0f, 1.54f);
            }

            // Add a little randomness so they don't stack perfectly vertically and topple unnaturally
            targetPos += new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

            // Start animation
            StartCoroutine(AnimateTokenRoutine(startPos, targetPos));
            
            Debug.Log("Ehrenpunkt (Honor Token) Animation gestartet!");
        }

        private System.Collections.IEnumerator AnimateTokenRoutine(Vector3 startPos, Vector3 targetPos)
        {
            // Spawn the token at the starting position (e.g. monster card)
            GameObject token = Instantiate(honorTokenPrefab, startPos, Random.rotation);
            
            // Disable physics while flying
            Rigidbody rb = token.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            float duration = 2.0f; // 2 seconds flight time (slower)
            float time = 0;

            // Very slight arc instead of a high curve
            Vector3 middlePos = Vector3.Lerp(startPos, targetPos, 0.5f) + new Vector3(0, 0.3f, 0);

            while (time < duration)
            {
                if (token == null) yield break;

                time += Time.deltaTime;
                float t = time / duration;
                
                // Ease in out
                float smoothedT = t * t * (3f - 2f * t);

                // Quadratic Bezier Curve for an arc
                Vector3 m1 = Vector3.Lerp(startPos, middlePos, smoothedT);
                Vector3 m2 = Vector3.Lerp(middlePos, targetPos, smoothedT);
                token.transform.position = Vector3.Lerp(m1, m2, smoothedT);

                // Spin nicely (slower to match the slower flight)
                token.transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);
                token.transform.Rotate(Vector3.right, 90f * Time.deltaTime, Space.Self);

                yield return null;
            }

            // End position
            if (token != null)
            {
                token.transform.position = targetPos;

                // Enable physics so it drops into the bowl
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Prevent tunneling through the bowl
                    
                    // Add a tiny random force so it spreads out slightly, but NO downward impulse!
                    rb.AddForce(new Vector3(Random.Range(-0.02f, 0.02f), 0f, Random.Range(-0.02f, 0.02f)), ForceMode.Impulse);
                }
            }
        }
    }
}
