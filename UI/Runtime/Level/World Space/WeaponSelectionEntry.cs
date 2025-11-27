using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level.World_Space {
    public class WeaponSelectionEntry : MonoBehaviour {
        [SerializeField, Required] Image iconImage;
        [SerializeField, Required] Outline backgroundOutline;
        public void ActivateOutline() => backgroundOutline.enabled = true;
        public void DeactivateOutline() => backgroundOutline.enabled = false;
        public void SetIconImage(Sprite sprite) => iconImage.sprite = sprite;
    }
}
