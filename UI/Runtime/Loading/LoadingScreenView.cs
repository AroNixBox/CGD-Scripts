using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime {
    public class LoadingScreenView : MonoBehaviour {
        [SerializeField, Required] RectTransform content;
        [SerializeField, Required] Slider loadingSlider;
        [SerializeField] TMP_Text percentText;

        public void Show() => content.gameObject.SetActive(true);
        public void Hide() => content.gameObject.SetActive(false);

        // progress range: 0..1
        public void SetProgress(float progress) {
            progress = Mathf.Clamp01(progress);
            if (loadingSlider != null) loadingSlider.value = progress;
            if (percentText != null) percentText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }
    }
}