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
            int nudgeAttempts = 0;
            
            while (true)
            {
                if (rb.linearVelocity.sqrMagnitude <= 0.01f && rb.angularVelocity.sqrMagnitude <= 0.01f)
                {
                    // It has stopped physically. Let's check if it's on an edge
                    if (IsOnEdge())
                    {
                        if (nudgeAttempts < 2)
                        {
                            nudgeAttempts++;
                            
                            // Nudge towards center (0,0,0)
                            Vector3 dirToCenter = (Vector3.zero - transform.position).normalized;
                            dirToCenter.y = 1.0f; // Add upward bounce
                            dirToCenter.Normalize();
                            
                            rb.AddForce(dirToCenter * 2.5f, ForceMode.Impulse);
                            rb.AddTorque(Random.insideUnitSphere * 2.5f, ForceMode.Impulse);
                            
                            // Wait a bit for the nudge to take effect
                            yield return new WaitForSeconds(0.5f);
                            continue; // Re-evaluate loop
                        }
                        else
                        {
                            // Stuck for too long, just snap it
                            SnapToNearestFace();
                            break;
                        }
                    }
                    else
                    {
                        // Safely flat
                        break;
                    }
                }

                elapsed += Time.deltaTime;
                if (elapsed >= timeout)
                {
                    // Timeout hit. Snap it if it's on an edge.
                    if (IsOnEdge()) SnapToNearestFace();
                    break;
                }
                
                yield return null;
            }
            
            // Dice stopped
            isRolling = false;
            CalculateResult();
        }

        private bool IsOnEdge()
        {
            float maxDot = -Mathf.Infinity;
            Vector3[] directions = { transform.up, -transform.up, transform.right, -transform.right, transform.forward, -transform.forward };
            foreach (var dir in directions)
            {
                float dot = Vector3.Dot(dir, Vector3.up);
                if (dot > maxDot) maxDot = dot;
            }
            
            // If the max dot product is less than 0.95 (approx 18 degrees off), it's leaning or on an edge
            return maxDot < 0.95f;
        }

        private void SnapToNearestFace()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90f) * 90f;
            euler.y = Mathf.Round(euler.y / 90f) * 90f;
            euler.z = Mathf.Round(euler.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(euler);
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
