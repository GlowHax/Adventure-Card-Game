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
    }
}
