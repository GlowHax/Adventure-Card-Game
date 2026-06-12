using UnityEngine;

namespace AdventureCardGame.Mechanics
{
    public class ItemGlowInDark : MonoBehaviour
    {
        public bool isCoin;
        private Light pointLight;
        private Material originalMat;
        private Material glowMat;
        
        void Start()
        {
            if (isCoin) return; // Coins do nothing in the dark now

            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            if (mr != null)
            {
                originalMat = mr.sharedMaterial;
                // Honor points glow blue
                glowMat = new Material(originalMat);
                glowMat.EnableKeyword("_EMISSION");
                glowMat.SetColor("_EmissionColor", new Color(0f, 0.1f, 1f) * 0.8f); // Dunkles Blau, schwaches Leuchten
            }
            
            pointLight = gameObject.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(0f, 0.1f, 1f); // Dunkelblau
            pointLight.range = 0.4f;
            pointLight.intensity = 0f; // Start at 0
            pointLight.shadows = LightShadows.None;
            pointLight.enabled = false;

            var hm = FindObjectOfType<HealthLightManager>();
            if (hm != null && hm.currentHealth <= 1) 
            {
                SnapToGlow();
            }
        }
        
        void OnEnable()
        {
            if (isCoin) return;
            HealthLightManager.OnHealthLevelChanged += HandleHealth;
        }
        
        void OnDisable()
        {
            if (isCoin) return;
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
            if (pointLight != null)
            {
                pointLight.enabled = true;
                pointLight.intensity = 0.2f;
            }
            if (glowMat != null)
            {
                MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
                if (mr != null) mr.material = glowMat;
            }
        }
        
        private System.Collections.IEnumerator FadeInRoutine()
        {
            yield return new WaitForSeconds(3.5f); // Sync with cards
            
            if (pointLight != null)
            {
                pointLight.enabled = true;
            }
            
            if (glowMat != null)
            {
                MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
                if (mr != null) mr.material = glowMat;
            }

            float duration = 1.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (pointLight != null) pointLight.intensity = 0.2f * t;
                
                if (glowMat != null)
                {
                    glowMat.SetColor("_EmissionColor", new Color(0f, 0.1f, 1f) * (0.8f * t));
                }
                
                yield return null;
            }
        }
    }
}
