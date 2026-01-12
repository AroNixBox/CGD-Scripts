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
                weaponStash.OnProjectileForceChanged += UpdateShootingPower;
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
                weaponStash.OnProjectileForceChanged -= UpdateShootingPower;
            }
            if (controller != null) {
                controller.OnCombatStanceStateEntered -= view.ShowUI;
                controller.OnCombatStanceStateExited -= view.HideUI;
            }
        }

        void AddWeapon(string category, WeaponData weaponData) {
            view.AddWeapon(category, weaponData);
        }

        // Necessary to update ammo count while shooting (live update)
        void UpdateWeaponAmmo(string category, WeaponData weaponData, int amount) {
            view.UpdateWeaponAmmo(category, weaponData, amount);
            
            if (weaponStash.GetCurrentWeaponData() == weaponData) {
                view.UpdateCurrentAmmoDisplay(amount);
            }
        }

        void SelectWeaponInUI(string category, WeaponData weaponData, int ammo) {
            view.UpdateCurrentAmmoDisplay(ammo); // Set the current ammo display
            view.SelectWeapon(category, weaponData);
        }
        
        void UpdateShootingPower(float percent) {
            view.UpdateShootingPower(percent);
        }
    }
}
