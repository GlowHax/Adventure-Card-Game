using System.Collections;
using UnityEngine;

namespace AdventureCardGame.Mechanics
{
    public class DiceRoller : MonoBehaviour
    {
        private Rigidbody rb;
        private bool isRolling;
        private int result;
        
        // This is a simple mapping for a standard Unity Cube where axes align with faces.
        // We will define:
        // +Y = 1, -Y = 6
        // +X = 2, -X = 5
        // +Z = 3, -Z = 4
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        }

        public void Roll(Vector3 spawnPosition, Vector3 forceDirection, float forceMagnitude)
        {
            transform.position = spawnPosition;
            transform.rotation = Random.rotation;
            
            rb.isKinematic = false;
            rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * forceMagnitude, ForceMode.Impulse);
            
            isRolling = true;
            result = 0;
            StartCoroutine(WaitAndCalculateResult());
        }

        private IEnumerator WaitAndCalculateResult()
        {
            // Give it some initial time to start moving
            yield return new WaitForSeconds(0.5f);
            
            float timeout = 4.0f;
            float elapsed = 0f;
            
            while (rb.linearVelocity.sqrMagnitude > 0.01f || rb.angularVelocity.sqrMagnitude > 0.01f)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= timeout) break;
                yield return null;
            }
            
            // Dice stopped
            isRolling = false;
            CalculateResult();
        }

        private void CalculateResult()
        {
            float maxDot = -Mathf.Infinity;
            Vector3 upDir = Vector3.up;

            // Check which local axis is pointing most towards World Up
            Vector3[] directions = {
                transform.up, -transform.up,
                transform.right, -transform.right,
                transform.forward, -transform.forward
            };

            int[] values = { 1, 6, 2, 5, 3, 4 };

            for (int i = 0; i < directions.Length; i++)
            {
                float dot = Vector3.Dot(directions[i], upDir);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    result = values[i];
                }
            }
            
            Debug.Log($"Dice rolled: {result}");
        }

        public bool IsRolling() => isRolling;
        
        public int GetResult() => result;
    }
}
