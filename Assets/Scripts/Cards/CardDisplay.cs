using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventureCardGame.Cards
{
    public class CardDisplay : MonoBehaviour
    {
        public CardData cardData;

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
                if (speedContainer != null) { speedContainer.SetActive(true); if (speedText != null) speedText.text = member.baseSpeed.ToString(); }
                if (strengthContainer != null) { strengthContainer.SetActive(true); if (strengthText != null) strengthText.text = member.baseStrength.ToString(); }
                if (costContainer != null) { costContainer.SetActive(true); if (costText != null) costText.text = member.honorCost.ToString(); }
            }
            else if (cardData is MonsterCardData monster)
            {
                if (descriptionText != null) descriptionText.text = monster.passiveEffectDescription;
                if (speedContainer != null) { speedContainer.SetActive(true); if (speedText != null) speedText.text = monster.speed.ToString(); }
                if (strengthContainer != null) { strengthContainer.SetActive(true); if (strengthText != null) strengthText.text = monster.strength.ToString(); }
                if (healthContainer != null) { healthContainer.SetActive(true); if (healthText != null) healthText.text = monster.healthPoints.ToString(); }
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
            else if (cardData is ObjectCardData obj)
            {
                if (descriptionText != null) descriptionText.text = obj.passiveEffectDescription;
            }
        }
    }
}
