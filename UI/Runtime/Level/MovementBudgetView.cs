using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class MovementBudgetView : MonoBehaviour {
        [SerializeField, Required] Slider slider;
        [SerializeField, Required] RectTransform content;

        public void UpdateBudget(float currentTime, float maxTime) {
            if (slider == null) return;
            
            slider.maxValue = maxTime;
            slider.value = currentTime;
        }

        public void Show() => content.gameObject.SetActive(true);
        public void Hide() => content.gameObject.SetActive(false);
    }
}

