using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class HealthBarView : MonoBehaviour {
        [SerializeField, Required] Slider uiHealthBar;

        public void UpdateHealthBar(float currentHealth) {
            uiHealthBar.value = Mathf.Clamp(currentHealth, 0, uiHealthBar.maxValue);
        }

        public void InitializeHealthBar(float maxHealth) {
            uiHealthBar.maxValue = maxHealth;
            uiHealthBar.value = maxHealth;
        }
    }
}
