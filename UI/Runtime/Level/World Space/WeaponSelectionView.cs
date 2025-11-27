using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI.Runtime.Level.World_Space {
    public class WeaponSelectionView : MonoBehaviour {
        [SerializeField, Required] RectTransform weaponSelectionMenu;
        [SerializeField, Required] ScrollRect weaponSelectionScroll;
        [SerializeField, Required] WeaponSelectionEntry weaponEntryPrefab;
        [SerializeField, Required] HorizontalLayoutGroup weaponEntriesParent;
        readonly List<WeaponSelectionEntry> _weaponEntries = new();
        public void SpawnWeaponSelectionEntry(Sprite iconImage) {
            var weaponEntryInstance = Instantiate(weaponEntryPrefab, weaponEntriesParent.transform);
            weaponEntryInstance.SetIconImage(iconImage);
            _weaponEntries.Add(weaponEntryInstance);
        }

        public void EnableWeaponEntryOutline(int index) {
            // Disable all other outliones
            _weaponEntries.ForEach(e => e.DeactivateOutline());
            ScrollToElement(index);
            _weaponEntries[index].ActivateOutline();
        }

        void ScrollToElement(int index) {
            // TODO
        }

        public void HideUI() => weaponSelectionMenu.gameObject.SetActive(false);
        public void ShowUI() => weaponSelectionMenu.gameObject.SetActive(true);
    }
}
