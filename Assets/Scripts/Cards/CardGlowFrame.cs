using UnityEngine;
using AdventureCardGame.Mechanics;

namespace AdventureCardGame.Cards
{
    public class CardGlowFrame : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private Light pointLight;

        void Awake()
        {
            // Destroy all old frames to prevent cloning bugs
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("GlowFrame"))
                {
                    Destroy(child.gameObject);
                }
            }

            GameObject frameObj = new GameObject("GlowFrame");
            frameObj.transform.SetParent(transform, false);
            frameObj.transform.localPosition = Vector3.zero;
            frameObj.transform.localRotation = Quaternion.identity;
            
            lineRenderer = frameObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = 4;
            
            float width = 1f;
            float height = 1.4f;
            
            // Use Canvas3DQuad for perfectly accurate visual size
            Transform quad = transform.Find("Canvas3DQuad");
            if (quad != null)
            {
                width = quad.localScale.x;
                height = quad.localScale.y;
            }
            else
            {
                var box = GetComponent<BoxCollider>();
                if (box != null)
                {
                    width = box.size.x;
                    height = box.size.y;
                }
            }

            float x = width * 0.5f + 0.005f;
            float y = height * 0.5f + 0.005f;
            
            lineRenderer.startWidth = 0.005f; 
            lineRenderer.endWidth = 0.005f;
            lineRenderer.alignment = LineAlignment.TransformZ; 

            lineRenderer.SetPosition(0, new Vector3(-x, y, 0));
            lineRenderer.SetPosition(1, new Vector3(x, y, 0));
            lineRenderer.SetPosition(2, new Vector3(x, -y, 0));
            lineRenderer.SetPosition(3, new Vector3(-x, -y, 0));

            Material litGlow = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            litGlow.SetColor("_BaseColor", Color.black);
            litGlow.EnableKeyword("_EMISSION");
            litGlow.SetColor("_EmissionColor", Color.cyan * 1.2f); 
            litGlow.SetFloat("_Cull", 0f); 
            
            lineRenderer.sharedMaterial = litGlow;
            lineRenderer.enabled = false;
        }

        void OnEnable()
        {
            HealthLightManager.OnHealthLevelChanged += HandleHealthChanged;
            var hm = FindObjectOfType<HealthLightManager>();
            if (hm != null && hm.currentHealth <= 1) 
            {
                SnapToGlow();
            }
        }

        void OnDisable()
        {
            HealthLightManager.OnHealthLevelChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(int health)
        {
            if (health == 1)
            {
                StartCoroutine(FadeInGlowRoutine());
            }
            else if (health > 1 && lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        private void SnapToGlow()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                Material mat = lineRenderer.sharedMaterial;
                mat.SetColor("_EmissionColor", Color.cyan * 1.2f);
            }
        }

        private System.Collections.IEnumerator FadeInGlowRoutine()
        {
            // Warte bis das Licht aus ist und die Schadens-Animation vorbei ist (1.5s)
            // + 2.0s dramatische Pause in völliger Dunkelheit = 3.5s Gesamtverzögerung
            yield return new WaitForSeconds(3.5f);

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                Material mat = lineRenderer.sharedMaterial;
                
                float duration = 1.5f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    
                    mat.SetColor("_EmissionColor", Color.cyan * (1.2f * t));
                    yield return null;
                }
                mat.SetColor("_EmissionColor", Color.cyan * 1.2f);
            }
        }
    }
}
