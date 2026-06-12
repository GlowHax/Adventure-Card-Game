using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventureCardGame.Cards
{
    public class CardDisplay : MonoBehaviour
    {
        public CardData cardData;
        public int currentHealth;
        public int currentSpeed;
        public int currentStrength;

        [Header("UI Elements")]
        public Image artworkImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        
        [Header("Stat Elements")]
        public GameObject speedContainer;
        public TextMeshProUGUI speedText;
        
        public GameObject strengthContainer;
        public TextMeshProUGUI strengthText;
        
        public GameObject healthContainer;
        public TextMeshProUGUI healthText;
        
        public GameObject costContainer;
        public TextMeshProUGUI costText;

        public System.Collections.Generic.List<EquipmentCardData> equippedItems = new System.Collections.Generic.List<EquipmentCardData>();

        private CanvasToRenderTexture c2rt;
        private TMPro.TextMeshProUGUI hiddenEffectText;

        void Awake()
        {
            c2rt = GetComponentInChildren<CanvasToRenderTexture>();

            if (descriptionText != null && hiddenEffectText == null)
            {
                GameObject qObj = new GameObject("HiddenEffectText");
                qObj.transform.SetParent(descriptionText.transform, false);
                hiddenEffectText = qObj.AddComponent<TMPro.TextMeshProUGUI>();
                
                hiddenEffectText.rectTransform.anchorMin = Vector2.zero;
                hiddenEffectText.rectTransform.anchorMax = Vector2.one;
                hiddenEffectText.rectTransform.offsetMin = Vector2.zero;
                hiddenEffectText.rectTransform.offsetMax = Vector2.zero;
                
                hiddenEffectText.text = "?";
                hiddenEffectText.enableAutoSizing = true;
                hiddenEffectText.fontSizeMin = 40f;
                hiddenEffectText.fontSizeMax = 500f;
                hiddenEffectText.alignment = TMPro.TextAlignmentOptions.Center;
                hiddenEffectText.fontStyle = TMPro.FontStyles.Bold;
                hiddenEffectText.color = new Color(1f, 1f, 1f, 0f); 
                hiddenEffectText.gameObject.SetActive(false);
            }

            // Dynamically add the RenderTexture converter to ensure all UI elements receive 3D lighting
            // without modifying prefabs manually.
            if (GetComponent<CanvasToRenderTexture>() == null)
            {
                gameObject.AddComponent<CanvasToRenderTexture>();
            }
            if (GetComponent<CardGlowFrame>() == null)
            {
                gameObject.AddComponent<CardGlowFrame>();
            }
        }

        public void Setup(CardData data)
        {
            cardData = data;
            if (data is MonsterCardData monster)
            {
                currentHealth = monster.healthPoints;
                currentSpeed = monster.speed;
                currentStrength = monster.strength;
            }
            else if (data is MemberCardData member)
            {
                currentHealth = 1; // Members don't have HP visually yet, but good to init
                currentSpeed = member.baseSpeed;
                currentStrength = member.baseStrength;
                
                // Copy starting equipment and apply permanent buffs
                equippedItems.Clear();
                if (member.startingEquipment != null)
                {
                    foreach (var item in member.startingEquipment)
                    {
                        equippedItems.Add(item);
                        if (!item.onlyOnDefense)
                        {
                            currentStrength += item.strengthBuff;
                            currentSpeed += item.speedBuff;
                        }

                        // Spawn physical representation
                        #if UNITY_EDITOR
                        GameObject eqPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardPrefab_Equipment.prefab");
                        if (eqPrefab != null)
                        {
                            GameObject eqObj = Instantiate(eqPrefab, transform);
                            // Set rotation to match the parent card exactly
                            eqObj.transform.localRotation = Quaternion.identity;
                            // Offset it so it peeks out from underneath the member (reduced distance)
                            eqObj.transform.localPosition = new Vector3(0.04f * equippedItems.Count, -0.04f * equippedItems.Count, 0.01f * equippedItems.Count);
                            // Set its visual display
                            var eqDisplay = eqObj.GetComponent<CardDisplay>();
                            if (eqDisplay != null) eqDisplay.Setup(item);
                            
                            // Make it non-interactable so it doesn't mess with dragging the parent card
                            var eqInteract = eqObj.GetComponent<CardInteractable>();
                            if (eqInteract != null) Destroy(eqInteract);
                            var eqCollider = eqObj.GetComponent<Collider>();
                            if (eqCollider != null) Destroy(eqCollider);
                        }
                        #endif
                    }
                }
            }
            UpdateDisplay();
        }



        public void UpdateDisplay()
        {
            if (cardData == null) return;

            if (nameText != null) nameText.text = cardData.cardName;
            if (descriptionText != null) descriptionText.text = cardData.description;
            if (artworkImage != null && cardData.artwork != null) artworkImage.sprite = cardData.artwork;

            // Reset all stats first
            if (speedContainer != null) speedContainer.SetActive(false);
            if (strengthContainer != null) strengthContainer.SetActive(false);
            if (healthContainer != null) healthContainer.SetActive(false);
            if (costContainer != null) costContainer.SetActive(false);

            if (cardData is MemberCardData member)
            {
                if (descriptionText != null) descriptionText.text = member.abilityDescription;
                if (speedContainer != null) { speedContainer.SetActive(true); if (speedText != null) { speedText.text = currentSpeed.ToString(); speedText.color = currentSpeed > member.baseSpeed ? new Color(1f, 0.8f, 0f) : Color.white; } }
                if (strengthContainer != null) { strengthContainer.SetActive(true); if (strengthText != null) { strengthText.text = currentStrength.ToString(); strengthText.color = currentStrength > member.baseStrength ? new Color(1f, 0.8f, 0f) : Color.white; } }
                if (costContainer != null) { costContainer.SetActive(true); if (costText != null) costText.text = member.honorCost.ToString(); }
            }
            else if (cardData is MonsterCardData monster)
            {
                if (descriptionText != null) descriptionText.text = monster.passiveEffectDescription;
                if (speedContainer != null) { speedContainer.SetActive(true); if (speedText != null) { speedText.text = currentSpeed.ToString(); speedText.color = currentSpeed > monster.speed ? new Color(1f, 0.8f, 0f) : Color.white; } }
                if (strengthContainer != null) { strengthContainer.SetActive(true); if (strengthText != null) { strengthText.text = currentStrength.ToString(); strengthText.color = currentStrength > monster.strength ? new Color(1f, 0.8f, 0f) : Color.white; } }
                if (healthContainer != null) { healthContainer.SetActive(true); if (healthText != null) { healthText.text = currentHealth.ToString(); healthText.color = currentHealth > monster.healthPoints ? new Color(1f, 0.8f, 0f) : Color.white; } }
            }
            else if (cardData is ItemCardData item)
            {
                if (speedContainer != null) { speedContainer.SetActive(true); if (speedText != null) speedText.text = "+" + item.speedBonus.ToString(); }
                if (strengthContainer != null) { strengthContainer.SetActive(true); if (strengthText != null) strengthText.text = "+" + item.strengthBonus.ToString(); }
            }
            else if (cardData is EventCardData ev)
            {
                // Events mostly just have text and artwork
            }
            else if (cardData is TreasureCardData treasure)
            {
                if (descriptionText != null) 
                {
                    if (treasure.goldAmount > 0)
                        descriptionText.text = "+" + treasure.goldAmount + " Gold";
                    else if (treasure.itemReward != null)
                        descriptionText.text = treasure.itemReward.cardName;
                    else
                        descriptionText.text = treasure.description;
                }
            }
            else if (cardData is ObjectCardData obj)
            {
                if (descriptionText != null) descriptionText.text = obj.passiveEffectDescription;
            }
            else if (cardData is EquipmentCardData eq)
            {
                if (costContainer != null) { costContainer.SetActive(true); if (costText != null) costText.text = eq.cost.ToString(); }
                if (strengthContainer != null && eq.strengthBuff > 0) { strengthContainer.SetActive(true); if (strengthText != null) strengthText.text = "+" + eq.strengthBuff; }
                if (speedContainer != null && eq.speedBuff > 0) { speedContainer.SetActive(true); if (speedText != null) speedText.text = "+" + eq.speedBuff; }
            }

            if (hiddenEffectText != null && descriptionText != null)
            {
                bool hasEffect = !string.IsNullOrWhiteSpace(descriptionText.text);
                hiddenEffectText.gameObject.SetActive(hasEffect);
                
                var hm = FindObjectOfType<HealthLightManager>();
                if (hm != null && hm.currentHealth <= 1)
                {
                    hiddenEffectText.color = new Color(1f, 1f, 1f, 1f); // Stay visible in dark
                }
                else
                {
                    hiddenEffectText.color = new Color(1f, 1f, 1f, 0f); // Stay hidden in light
                }
            }
        }
        public void HighlightEffectText()
        {
            if (descriptionText != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(HighlightRoutine());
            }
        }

        private System.Collections.IEnumerator HighlightRoutine()
        {
            Color originalColor = descriptionText.color;
            Vector3 originalScale = descriptionText.transform.localScale;
            
            descriptionText.color = new Color(1f, 0.8f, 0f); // Gold/Yellow
            descriptionText.transform.localScale = originalScale * 1.2f;
            
            yield return new WaitForSeconds(1.5f);
            
            descriptionText.color = originalColor;
            descriptionText.transform.localScale = originalScale;
        }

        public void HighlightStrengthText()
        {
            if (strengthText != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(HighlightStatRoutine(strengthText));
            }
        }

        private System.Collections.IEnumerator HighlightStatRoutine(TextMeshProUGUI textComp)
        {
            Color originalColor = textComp.color;
            Vector3 originalScale = textComp.transform.localScale;
            
            textComp.color = new Color(1f, 0.8f, 0f); // Gold/Yellow
            textComp.transform.localScale = originalScale * 1.2f;
            
            yield return new WaitForSeconds(1.5f);
            
            if (textComp != null)
            {
                textComp.color = originalColor;
                textComp.transform.localScale = originalScale;
            }
        }

        void OnEnable()
        {
            HealthLightManager.OnHealthLevelChanged += HandleHealthChanged;
            
            // Check if we spawned into a critical health state
            var hm = FindObjectOfType<HealthLightManager>();
            if (hm != null && hm.currentHealth <= 1)
            {
                StartCoroutine(FadeToBlackRoutine(0f)); 
            }
        }

        void OnDisable()
        {
            HealthLightManager.OnHealthLevelChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(int currentHealth)
        {
            if (currentHealth == 1)
            {
                StartCoroutine(FadeToBlackRoutine(1.5f));
            }
        }

        private System.Collections.IEnumerator FadeToBlackRoutine(float duration)
        {
            float elapsed = 0f;
            
            Image bgImage = GetComponent<Image>();
            if (bgImage == null) bgImage = GetComponentInChildren<Image>();
            
            Color startBgColor = bgImage != null ? bgImage.color : Color.white;
            Color targetBgColor = Color.black; // PERFECTLY black so it emits NO light and respects 3D lighting!
            
            // Holen der Container-Hintergründe, damit diese auch unsichtbar werden und NUR die Zahlen leuchten
            Image[] containerImages = new Image[4];
            if (speedContainer != null) containerImages[0] = speedContainer.GetComponent<Image>();
            if (strengthContainer != null) containerImages[1] = strengthContainer.GetComponent<Image>();
            if (healthContainer != null) containerImages[2] = healthContainer.GetComponent<Image>();
            if (costContainer != null) containerImages[3] = costContainer.GetComponent<Image>();

            // Turn the card's 3D Quad into a light emitter!
            var c2rt = GetComponent<CanvasToRenderTexture>();
            // We NO LONGER enable emission here, we wait until the card is black!
            
            while(duration > 0f && elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                if (descriptionText != null) descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, 1f - t);
                if (nameText != null) nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, 1f - t);
                if (artworkImage != null) artworkImage.color = new Color(artworkImage.color.r, artworkImage.color.g, artworkImage.color.b, 1f - t);
                if (bgImage != null && bgImage != artworkImage) bgImage.color = Color.Lerp(startBgColor, targetBgColor, t);
                
                for(int i = 0; i < 4; i++) {
                    if (containerImages[i] != null) containerImages[i].color = new Color(containerImages[i].color.r, containerImages[i].color.g, containerImages[i].color.b, 1f - t);
                }

                yield return null;
            }

            // Snap to final values
            if (descriptionText != null) descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, 0f);
            if (nameText != null) nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, 0f);
            if (artworkImage != null) artworkImage.color = new Color(artworkImage.color.r, artworkImage.color.g, artworkImage.color.b, 0f);
            if (bgImage != null && bgImage != artworkImage) bgImage.color = targetBgColor;
            
            for(int i = 0; i < 4; i++) {
                if (containerImages[i] != null) containerImages[i].color = new Color(containerImages[i].color.r, containerImages[i].color.g, containerImages[i].color.b, 0f);
            }

            if (duration > 0f)
            {
                yield return new WaitForSeconds(2.0f); // Dramatische Pause in völliger Dunkelheit!
            }

            // NOW: Slowly fade in the emission OR snap instantly
            if (c2rt != null)
            {
                float glowDuration = duration > 0f ? 1.5f : 0f;
                float glowElapsed = 0f;
                
                while (glowDuration > 0f && glowElapsed < glowDuration)
                {
                    glowElapsed += Time.deltaTime;
                    float t = glowElapsed / glowDuration;
                    c2rt.SetEmissionColor(Color.white * (2.5f * t));
                    
                    if (hiddenEffectText != null && hiddenEffectText.gameObject.activeSelf)
                    {
                        hiddenEffectText.color = new Color(1f, 1f, 1f, t);
                    }
                    
                    yield return null;
                }
                c2rt.SetEmissionColor(Color.white * 2.5f);
                
                if (hiddenEffectText != null && hiddenEffectText.gameObject.activeSelf)
                {
                    hiddenEffectText.color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
    }
}
