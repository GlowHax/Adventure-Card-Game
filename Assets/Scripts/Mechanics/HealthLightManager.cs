using UnityEngine;
using System.Collections;

public class HealthLightManager : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 3;
    public int currentHealth = 3;

    [Header("Light References")]
    public Light mainTableLight;
    public GameOverSequence gameOverSequence;

    [Header("Light Settings")]
    public float level3Intensity = 6f;
    public float level3SpotAngle = 96f;

    public float level2Intensity = 4f;
    public float level2SpotAngle = 85f;

    public float level1Intensity = 0f;
    public float level1SpotAngle = 50f;
    
    public float transitionDuration = 1.0f;
    public float damageShakeMagnitude = 0.25f; // Reduced default for softer shake

    private Coroutine ambientFlickerCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
        if (mainTableLight != null)
        {
            mainTableLight.intensity = level3Intensity;
            mainTableLight.spotAngle = level3SpotAngle;
        }
        ambientFlickerCoroutine = StartCoroutine(AmbientFlickerRoutine());
    }

    public static event System.Action<int> OnHealthLevelChanged;

    [ContextMenu("Take Damage")]
    public void TakeDamage()
    {
        if (currentHealth <= 0) return;
        
        currentHealth--;
        
        OnHealthLevelChanged?.Invoke(currentHealth);
        
        if (currentHealth > 0)
        {
            StartCoroutine(DamageImpactRoutine());
        }
        else
        {
            if (mainTableLight != null) mainTableLight.intensity = 0f;
            if (gameOverSequence != null) gameOverSequence.TriggerGameOver();
            if (ambientFlickerCoroutine != null) StopCoroutine(ambientFlickerCoroutine);
        }
    }

    private IEnumerator AmbientFlickerRoutine()
    {
        while (true)
        {
            // Only ambient flicker if we took damage but are not dead
            if (currentHealth < maxHealth && currentHealth > 0)
            {
                // Wait for a random time (e.g. 4 to 12 seconds)
                yield return new WaitForSeconds(Random.Range(4f, 12f));
                
                if (mainTableLight != null)
                {
                    float targetIntensity = currentHealth == 2 ? level2Intensity : level1Intensity;
                    
                    // Quick stutter
                    mainTableLight.intensity = targetIntensity * Random.Range(0.2f, 0.5f);
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    
                    if (Random.value > 0.5f) // 50% chance for a double stutter
                    {
                        mainTableLight.intensity = targetIntensity * Random.Range(1.1f, 1.4f);
                        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                        mainTableLight.intensity = targetIntensity * Random.Range(0.1f, 0.3f);
                        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    }
                    
                    mainTableLight.intensity = targetIntensity;
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator DamageImpactRoutine()
    {
        Camera cam = Camera.main;
        CameraOffsetter offsetter = null;
        if (cam != null)
        {
            offsetter = cam.GetComponent<CameraOffsetter>();
            if (offsetter == null) offsetter = cam.gameObject.AddComponent<CameraOffsetter>();
        }

        float targetIntensity = currentHealth == 2 ? level2Intensity : level1Intensity;
        float targetAngle = currentHealth == 2 ? level2SpotAngle : level1SpotAngle;
        
        float startIntensity = mainTableLight != null ? mainTableLight.intensity : 0f;
        float startAngle = mainTableLight != null ? mainTableLight.spotAngle : 0f;

        float elapsed = 0f;
        float duration = 1.5f; 
        
        if (mainTableLight != null) mainTableLight.intensity = 0f;

        float seed = Random.Range(0f, 100f);
        // Deutlich sanfteres Wackeln bei Level 3->2 (Faktor 0.2 statt 1.0)
        float currentDamageMultiplier = currentHealth == 2 ? 0.2f : 2.0f; 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float invT = 1f - t; 
            
            float flickerDecay = Mathf.Pow(invT, 0.5f); 

            if (offsetter != null)
            {
                // Softened shake
                float currentShake = (damageShakeMagnitude * 0.7f) * currentDamageMultiplier * invT;
                float shakeX = (Mathf.PerlinNoise(seed + elapsed * 20f, 0f) - 0.5f) * 2f * currentShake;
                float shakeY = (Mathf.PerlinNoise(0f, seed + elapsed * 20f) - 0.5f) * 2f * currentShake;
                offsetter.positionOffset = new Vector3(shakeX, shakeY, 0f);
            }

            if (mainTableLight != null)
            {
                float lightT = t;
                if (currentHealth == 1)
                {
                    // Erst ein bisschen wackeln lassen, dann plötzlich sehr schnell verdunkeln
                    float fadeStart = 0.35f; // Startet bei 35% der Animation (ca. 0.5 Sekunden)
                    float fadeLength = 0.3f; // Dauert 30% der Animation (ca. 0.45 Sekunden)
                    
                    if (t < fadeStart) lightT = 0f;
                    else lightT = Mathf.Clamp01((t - fadeStart) / fadeLength);
                }
                
                float baseIntensity = Mathf.Lerp(startIntensity, targetIntensity, lightT);
                float baseAngle = Mathf.Lerp(startAngle, targetAngle, lightT);
                
                // Lower frequency (12f instead of 45f) for "scattered", isolated flickers
                float noise = Mathf.PerlinNoise(seed + 100f, elapsed * 12f);
                
                float flickerMultiplier = 1f;
                // Only flicker if noise hits extreme highs/lows, creating distinct gaps
                if (noise < 0.25f * flickerDecay) flickerMultiplier = 0f; 
                else if (noise > (1f - 0.25f * flickerDecay)) flickerMultiplier = Random.Range(1.5f, 2.5f); 

                float currentIntensity = baseIntensity * Mathf.Lerp(1f, flickerMultiplier, flickerDecay);
                
                mainTableLight.intensity = Mathf.Max(0f, currentIntensity);
                mainTableLight.spotAngle = baseAngle;
            }

            yield return null;
        }

        if (offsetter != null) offsetter.positionOffset = Vector3.zero;
        if (mainTableLight != null)
        {
            mainTableLight.intensity = targetIntensity;
            mainTableLight.spotAngle = targetAngle;
        }

        if (currentHealth == 1)
        {
            Debug.Log("Light Level 1 reached! Card texts should fade out now.");
        }
    }
}
