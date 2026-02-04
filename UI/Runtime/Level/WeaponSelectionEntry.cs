using Gameplay.Runtime.Player.Combat;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class WeaponSelectionEntry : MonoBehaviour {
        // TODO On spawn zero out all positions
        [SerializeField, Required] Image iconImage;
        public WeaponData Data { get; private set; }

        public void Init(WeaponData data) {
            Data = data;
            SetIconImage(data.MenuIcon);
        }

        void SetIconImage(Sprite sprite) => iconImage.sprite = sprite;
    }
}
