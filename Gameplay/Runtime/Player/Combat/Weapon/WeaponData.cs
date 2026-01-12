using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject {
        [HorizontalGroup("MetaData", Width = 100)]
        [VerticalGroup("MetaData/Left")]
        [PreviewField(90, ObjectFieldAlignment.Left)]
        [Title("Menu Icon")]
        [HideLabel]
        [SerializeField, Required] Sprite menuIcon;
        public Sprite MenuIcon => menuIcon;

        [VerticalGroup("MetaData/Right")]
        [ShowInInspector, PreviewField(90, ObjectFieldAlignment.Left)]
        [Tooltip("Physical Weapon Prefab")]
        [Title("Weapon Prefab")]
        [AssetsOnly, HideLabel]
        [ValidateInput("HasWeaponComponent", "Prefab must have a 'Weapon' component!")]
        [PropertyOrder(-1)]
        GameObject WeaponPrefab {
            get => Weapon != null ? Weapon.gameObject : null;
            set {
                if (value == null) Weapon = null;
                else if (value.TryGetComponent(out Weapon w)) Weapon = w;
            }
        }

        [field: SerializeField, HideInInspector] 
        public Weapon Weapon { get; private set; }

        bool HasWeaponComponent(GameObject go) {
            if (go == null) return true;
            return go.GetComponent<Weapon>() != null;
        }

        // TODO: Specific Animation for this Weapon for Aiming & Shooting, alternatively IK
        
        [Title("Shared Configuration")]
        [GUIColor(0.9f, 1f, 0.9f)]
        [SerializeField, Required]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)] 
        public ProjectileData projectileData;
        public ProjectileData ProjectileData => projectileData;
        
        [GUIColor(0.9f, 1f, 0.9f)]
        [SerializeField, Required] 
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public GlobalWeaponData globalWeaponData;
        public GlobalWeaponData GlobalWeaponData => globalWeaponData;
    }
}