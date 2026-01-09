using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UI.Runtime.Level {
    /// <summary>
    ///     Handles the visual display of the turn timer.
    /// </summary>
    public class TurnTimerView : MonoBehaviour {
        [SerializeField, Required]
        private TMP_Text timerText;
        [SerializeField, Required]
        private RectTransform content;
        [SerializeField]
        private string timerFormat = "{0:F1}s";

        [BoxGroup("Colors"), SerializeField]
        
        private Color normalColor = Color.white;
        [BoxGroup("Colors"), SerializeField]
        
        private Color warningColor = Color.yellow;
        [BoxGroup("Colors"), SerializeField]
        
        private Color criticalColor = Color.red;

        [BoxGroup("Thresholds"), SerializeField, Tooltip("Time in seconds below which warning color is shown")]
        private float warningThreshold = 10f;
        [BoxGroup("Thresholds"), SerializeField, Tooltip("Time in seconds below which critical color is shown")]
        private float criticalThreshold = 5f;

        private float _maxTime;

        /// <summary>
        ///     Initializes the timer display with the maximum turn duration.
        /// </summary>
        /// <param name="maxTime">Maximum turn time in seconds</param>
        public void Initialize(float maxTime) {
            _maxTime = maxTime;
            UpdateDisplay(maxTime);
        }

        /// <summary>
        ///     Updates the timer display with the current remaining time.
        /// </summary>
        /// <param name="remainingTime">Remaining time in seconds</param>
        public void UpdateDisplay(float remainingTime) {
            if (timerText != null) {
                timerText.text = string.Format(timerFormat, Mathf.Max(0, remainingTime));
                timerText.color = GetTimerColor(remainingTime);
            }
        }

        /// <summary>
        ///     Called when the timer expires.
        /// </summary>
        public void OnTimerExpired() {
            UpdateDisplay(0);
        }

        private Color GetTimerColor(float remainingTime) {
            // Smoothly fade from normal -> warning -> critical
            if (remainingTime <= criticalThreshold) {
                return criticalColor;
            }
            if (remainingTime <= warningThreshold) {
                // Lerp between warning and critical
                var t = (remainingTime - criticalThreshold) / (warningThreshold - criticalThreshold);
                return Color.Lerp(criticalColor, warningColor, t);
            }
            // Lerp between normal and warning
            var t2 = (remainingTime - warningThreshold) / (_maxTime - warningThreshold);
            return Color.Lerp(warningColor, normalColor, t2);
        }

        /// <summary>
        ///     Shows or hides the timer display.
        /// </summary>
        public void SetVisible(bool visible) {
            content.gameObject.SetActive(visible);
        }
    }
}