using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class HealthBarView : MonoBehaviour {
        [SerializeField, Required] TMP_Text label;
        [SerializeField, Required] Image image;
        [SerializeField, Required] Slider uiHealthBar;
        [SerializeField, Required] Color flickeringHealthBarColor = new(0f, 0f, 0f, 1f); // flickering color
        [SerializeField, Required] float flickerSpeed = 25f; // fast flickering

        public void UpdateHealthBar(float currentHealth) {
            if (uiHealthBar ==  null) return;
            
            AnimateHealthBarChange(currentHealth).Forget();
        }

        async UniTaskVoid AnimateHealthBarChange(float targetHealth) {
            var fill = uiHealthBar.fillRect;
            if (fill == null) return;

            if (!fill.TryGetComponent(out Image fillImage)) return;
            
            // Clamp
            targetHealth = Mathf.Clamp(targetHealth, 0f, uiHealthBar.maxValue);
            
            var normalColor = fillImage.color;
            
            var currentValue = uiHealthBar.value;
            var duration = 0.6f;
            var flashDuration = 0.1f;
            
            // Flash to black when recieving damage
            var elapsed = 0f;
            while (elapsed < flashDuration) {
                elapsed += Time.deltaTime;
                var t = elapsed / flashDuration;
                // EaseOutQuad für schnellen Impact
                var eased = 1f - (1f - t) * (1f - t);
                fillImage.color = Color.Lerp(normalColor, flickeringHealthBarColor, eased);
                await UniTask.Yield();
            }
            
            // Back to normal color
            elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                
                // Smooth
                var eased = 1f - Mathf.Pow(1f - t, 3f);
                uiHealthBar.value = Mathf.Lerp(currentValue, targetHealth, eased);
                
                // fast flickering
                var flicker = Mathf.Sin(elapsed * flickerSpeed) * 0.5f + 0.5f;
                var flickerColor = Color.Lerp(flickeringHealthBarColor, normalColor, flicker);
                
                // back to normal
                fillImage.color = Color.Lerp(flickerColor, normalColor, eased);
                
                await UniTask.Yield();
            }
            
            // Final value
            uiHealthBar.value = targetHealth;
            fillImage.color = normalColor;
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
