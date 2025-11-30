using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class WeaponSelectionEntry : MonoBehaviour {
        [SerializeField, Required] Image iconImage;
        [SerializeField, Required] Outline backgroundOutline;
        [SerializeField, Required] TMP_Text ammoAmountLabel;
        public void SetAmmo(int amount) => ammoAmountLabel.text = $"AMMO: {amount}";
        public void ActivateOutline() => backgroundOutline.enabled = true;
        public void DeactivateOutline() => backgroundOutline.enabled = false;
        public void SetIconImage(Sprite sprite) => iconImage.sprite = sprite;
    }
}
