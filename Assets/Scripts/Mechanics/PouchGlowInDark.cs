using UnityEngine;

namespace AdventureCardGame.Mechanics
{
    public class PouchGlowInDark : MonoBehaviour
    {
        private Light pouchLight;
        private float originalIntensity;
        private float originalRange;

        void Start()
        {
            // Find the point light child
            pouchLight = GetComponentInChildren<Light>();
            if (pouchLight != null)
            {
                originalIntensity = pouchLight.intensity;
                originalRange = pouchLight.range;
            }

            var hm = FindObjectOfType<HealthLightManager>();
            if (hm != null && hm.currentHealth <= 1) 
            {
                SnapToGlow();
            }
        }

        void OnEnable()
        {
            HealthLightManager.OnHealthLevelChanged += HandleHealth;
        }

        void OnDisable()
        {
            HealthLightManager.OnHealthLevelChanged -= HandleHealth;
        }

        void HandleHealth(int health)
        {
            if (health == 1)
            {
                StartCoroutine(FadeInRoutine());
            }
        }

        private void SnapToGlow()
        {
            if (pouchLight != null)
            {
                pouchLight.intensity = originalIntensity * 3.5f; // Deutlich heller
                pouchLight.range = originalRange * 2.0f; // Größerer Leuchtradius
            }
        }

        private System.Collections.IEnumerator FadeInRoutine()
        {
            yield return new WaitForSeconds(3.5f); // Sync with dark pause
            
            if (pouchLight == null) yield break;

            float duration = 1.5f;
            float elapsed = 0f;

            float targetIntensity = originalIntensity * 3.5f;
            float targetRange = originalRange * 2.0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                pouchLight.intensity = Mathf.Lerp(originalIntensity, targetIntensity, t);
                pouchLight.range = Mathf.Lerp(originalRange, targetRange, t);
                
                yield return null;
            }
        }
    }
}
