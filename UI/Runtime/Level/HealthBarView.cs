using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class HealthBarView : MonoBehaviour {
        [SerializeField, Required] TMP_Text label;
        [SerializeField, Required] Image image;
        [SerializeField, Required] Slider uiHealthBar;

        public void UpdateHealthBar(float currentHealth) {
            uiHealthBar.value = Mathf.Clamp(currentHealth, 0, uiHealthBar.maxValue);
        }

        public void InitializeHealthBar(float maxHealth, string labelText, Sprite sprite) {
            if (uiHealthBar == null)
                throw new NullReferenceException("Healthbar not assigend properly in the Inspector");
            
            uiHealthBar.maxValue = maxHealth;
            uiHealthBar.value = maxHealth;
            
            if(label != null && label.text != string.Empty)
                label.text = labelText;
            if(image != null && sprite != null)
                image.sprite = sprite;

            // TODO:
            // Already in LayoutGroup, so can just reset the index of image and textalign of TMP to re-order
            // SetHealthBarOrder(healthBarOrder);
        }
    }
}
