using System;
using System.Collections.Generic;
using Gameplay.Runtime.Player;
using Gameplay.Runtime.Player.Combat;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    // TODO: Might need rework if weapons pickable in runtime
    public class WeaponSelectionController : MonoBehaviour {
        [SerializeField, Required] PlayerController controller;
        [SerializeField, Required] PlayerWeaponStash weaponStash;
        [SerializeField, Required] WeaponSelectionView view;
        readonly List<WeaponData> _weaponDatas = new();

        void OnEnable() {
            weaponStash.OnWeaponDataAdded += AddWeapon;
            weaponStash.OnWeaponDataSelected += SelectWeaponInUI;
            weaponStash.OnAmmoChanged += UpdateWeaponAmmo;
            controller.OnCombatStanceStateEntered += view.ShowUI;
            controller.OnCombatStanceStateExited += view.HideUI;
        }

        // By default UI is off
        void Start() => view.HideUI();

        void OnDisable() {
            weaponStash.OnWeaponDataAdded -= AddWeapon;
            weaponStash.OnWeaponDataSelected -= SelectWeaponInUI;
            weaponStash.OnAmmoChanged -= UpdateWeaponAmmo;
            controller.OnCombatStanceStateEntered -= view.ShowUI;
            controller.OnCombatStanceStateExited -= view.HideUI;
        }

        void AddWeapon(WeaponData weaponData) {
            view.SpawnWeaponSelectionEntry(weaponData.MenuIcon);
            _weaponDatas.Add(weaponData);
        }

        void UpdateWeaponAmmo(WeaponData weaponData, int ammo) {
            if (!_weaponDatas.Contains(weaponData))
                throw new ArgumentOutOfRangeException(weaponData.name, "Tried changing the Ammo on a weapon that is not in the UI");
            
            view.UpdateWeaponAmmo(_weaponDatas.IndexOf(weaponData), ammo);
        }

        void SelectWeaponInUI(WeaponData weaponData) {
            if (!_weaponDatas.Contains(weaponData))
                throw new NullReferenceException("Weapon was not initialized in WeaponUI");
            
            view.EnableWeaponEntryOutline(_weaponDatas.IndexOf(weaponData));
        }
    }
}
