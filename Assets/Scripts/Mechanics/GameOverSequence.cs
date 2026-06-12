using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameOverSequence : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip painSound;
    public AudioClip tableThudSound;

    [Header("UI")]
    public CanvasGroup blackScreenCanvasGroup;
    public CanvasGroup gameOverTextGroup;
    public CanvasGroup restartButtonGroup;
    public UnityEngine.UI.Button restartButton; // Reference to wire up the click at runtime
    
    [Header("Animation Settings")]
    public float fallDuration = 2.5f;
    public float blackFadeDuration = 2.0f;
    public float textFadeDuration = 1.5f;
    public float buttonDelay = 1.0f;
    public float buttonFadeDuration = 1.0f;
    public Transform cameraTransform;

    void Awake()
    {
        if (restartButton == null)
        {
            // Fallback: search the hierarchy dynamically just in case the reference was lost
            var canvas = GameObject.Find("GameOverCanvas");
            if (canvas != null)
            {
                var btn = canvas.transform.Find("BlackScreen/ButtonGroup/RestartButton");
                if (btn != null) restartButton = btn.GetComponent<UnityEngine.UI.Button>();
            }
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void Start()
    {
        // Ensure UI is completely hidden and unclickable at start
        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.alpha = 0f;
            blackScreenCanvasGroup.blocksRaycasts = false;
            blackScreenCanvasGroup.interactable = false;
        }
        
        if (gameOverTextGroup != null)
        {
            gameOverTextGroup.alpha = 0f;
            gameOverTextGroup.blocksRaycasts = false;
        }
        
        if (restartButtonGroup != null)
        {
            restartButtonGroup.alpha = 0f;
            restartButtonGroup.blocksRaycasts = false;
            restartButtonGroup.interactable = false;
        }
    }

    [ContextMenu("Test Game Over")]
    public void TriggerGameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        // 1. Play Pain Sound
        if (audioSource != null && painSound != null)
        {
            audioSource.PlayOneShot(painSound);
        }

        // Disable Cinemachine if it exists so we can move the camera manually
        var brain = cameraTransform.GetComponent("CinemachineBrain") as MonoBehaviour;
        if (brain != null) brain.enabled = false;

        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        // The target is falling with forehead onto the table. Table is assumed around y=0.
        Vector3 finalPos = startPos + cameraTransform.forward * 0.5f;
        finalPos.y = 0.05f; // Hit the table
        
        // Forehead hits table -> Looking mostly straight down (pitch ~85), slight tilt
        Quaternion finalRot = Quaternion.Euler(85f, startRot.eulerAngles.y + 10f, 5f);

        // Anticipation position (stagger up and back)
        Vector3 staggerPos = startPos + Vector3.up * 0.4f - cameraTransform.forward * 0.15f;
        Quaternion staggerRot = Quaternion.Euler(startRot.eulerAngles.x - 25f, startRot.eulerAngles.y, startRot.eulerAngles.z);

        float elapsed = 0f;
        bool screenIsBlack = false;

        if (gameOverTextGroup != null)
        {
            gameOverTextGroup.alpha = 0f;
            gameOverTextGroup.gameObject.SetActive(true);
        }
        if (restartButtonGroup != null)
        {
            restartButtonGroup.alpha = 0f;
            restartButtonGroup.gameObject.SetActive(true);
            restartButtonGroup.interactable = false;
            restartButtonGroup.blocksRaycasts = false;
        }

        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.alpha = 0f;
            // Activate the gameobject if it was disabled
            blackScreenCanvasGroup.gameObject.SetActive(true);
        }

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            Vector3 currentPos;
            Quaternion currentRot;

            if (t < 0.25f)
            {
                // Stagger up (ease out)
                float st = t / 0.25f;
                float ease = Mathf.Sin(st * Mathf.PI * 0.5f);
                currentPos = Vector3.Lerp(startPos, staggerPos, ease);
                currentRot = Quaternion.Slerp(startRot, staggerRot, ease);
            }
            else
            {
                // Plummet down (ease in)
                float ft = (t - 0.25f) / 0.75f;
                float ease = ft * ft * ft; // gravity curve
                currentPos = Vector3.Lerp(staggerPos, finalPos, ease);
                currentRot = Quaternion.Slerp(staggerRot, finalRot, ease);
            }

            // Make the movement a slow, dizzy stagger instead of a hectic jitter
            float shakeIntensity = (1f - t) * 0.15f; // Slightly wider sway
            float shakeX = (Mathf.PerlinNoise(Time.time * 2.5f, 0f) - 0.5f) * shakeIntensity;
            float shakeY = (Mathf.PerlinNoise(0f, Time.time * 2.5f) - 0.5f) * shakeIntensity;

            cameraTransform.position = currentPos + new Vector3(shakeX, shakeY, 0);
            cameraTransform.rotation = currentRot;

            // Blackout fades using its own explicit duration timer!
            if (blackScreenCanvasGroup != null)
            {
                // Slow fade based on absolute seconds (elapsed), not percentage of fall
                blackScreenCanvasGroup.alpha = Mathf.InverseLerp(0.0f, blackFadeDuration, elapsed);
                if (elapsed > blackFadeDuration) screenIsBlack = true;
            }

            yield return null;
        }

        cameraTransform.position = finalPos;
        cameraTransform.rotation = finalRot;

        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.alpha = 1f;
            blackScreenCanvasGroup.blocksRaycasts = true; // IMPORTANT: Allow children to be clicked!
            blackScreenCanvasGroup.interactable = true; // CRITICAL: If false, it disables all child buttons!
        }

        // The head has physically hit the table exactly NOW (t=1.0). Play the Thud sound!
        if (audioSource != null && tableThudSound != null)
        {
            audioSource.PlayOneShot(tableThudSound);
        }

        // Slowly fade in "GAME OVER" text
        if (gameOverTextGroup != null)
        {
            float textElapsed = 0f;
            while (textElapsed < textFadeDuration)
            {
                textElapsed += Time.deltaTime;
                gameOverTextGroup.alpha = textElapsed / textFadeDuration;
                yield return null;
            }
            gameOverTextGroup.alpha = 1f;
        }

        // Wait before showing button
        yield return new WaitForSeconds(buttonDelay);

        // Slowly fade in Restart Button
        if (restartButtonGroup != null)
        {
            restartButtonGroup.interactable = true;
            restartButtonGroup.blocksRaycasts = true;
            
            if (restartButton != null) restartButton.interactable = true;

            float btnElapsed = 0f;
            while (btnElapsed < buttonFadeDuration)
            {
                btnElapsed += Time.deltaTime;
                restartButtonGroup.alpha = btnElapsed / buttonFadeDuration;
                yield return null;
            }
            restartButtonGroup.alpha = 1f;
        }
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame wurde geklickt!");
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (currentScene.buildIndex >= 0) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.buildIndex);
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
        }
    }
}
