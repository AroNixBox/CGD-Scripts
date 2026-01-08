using Gameplay.Runtime.Player;
using Gameplay.Runtime.Player.Combat;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    // Controller spawns entries and categories, so he can create a category when theres a "first entry"
    public class WeaponSelectionController : MonoBehaviour {
        [SerializeField, Required] PlayerController controller;
        [SerializeField, Required] PlayerWeaponStash weaponStash;
        [SerializeField, Required] WeaponSelectionView view;
        
        void OnEnable() {
            if (weaponStash != null) {
                weaponStash.OnWeaponDataAdded += AddWeapon;
                weaponStash.OnWeaponDataSelected += SelectWeaponInUI;
                weaponStash.OnAmmoChanged += UpdateWeaponAmmo;
            }
            if (controller != null) {
                controller.OnCombatStanceStateEntered += view.ShowUI;
                controller.OnCombatStanceStateExited += view.HideUI;
            }
        }

        // By default UI is off
        void Start() => view.HideUI();

        void OnDisable() {
            if (weaponStash != null) {
                weaponStash.OnWeaponDataAdded -= AddWeapon;
                weaponStash.OnWeaponDataSelected -= SelectWeaponInUI;
                weaponStash.OnAmmoChanged -= UpdateWeaponAmmo;
            }
            if (controller != null) {
                controller.OnCombatStanceStateEntered -= view.ShowUI;
                controller.OnCombatStanceStateExited -= view.HideUI;
            }
        }

        void AddWeapon(string category, WeaponData weaponData) {
            view.AddWeapon(category, weaponData);
        }

        void UpdateWeaponAmmo(string category, WeaponData weaponData, int amount) {
            view.UpdateWeaponAmmo(category, weaponData, amount);
        }

        void SelectWeaponInUI(string category, WeaponData weaponData) {
            view.SelectWeapon(category, weaponData);
        }
    }
}
