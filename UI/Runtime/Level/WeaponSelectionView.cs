using System;
using System.Collections.Generic;
using Gameplay.Runtime.Player.Combat;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class WeaponSelectionView : MonoBehaviour {
        [SerializeField, Required] RectTransform weaponSelectionMenu;
        [SerializeField, Required] ScrollRect weaponSelectionScroll;
        [SerializeField, Required] RectTransform weaponCategoriesParent;
        
        [SerializeField, Required] WeaponSelectionCategory weaponCategoryPrefab;
        [SerializeField, Required] WeaponSelectionEntry weaponEntryPrefab;
        [SerializeField, Required] TMP_Text currentWeaponNameText;
        [SerializeField, Required] TMP_Text currentWeaponAmmoText;
        [SerializeField, Required] TMP_Text shootingPowerText;

        readonly Dictionary<string, WeaponSelectionCategory> _categories = new();

        public void AddWeapon(string categoryName, WeaponData data) {
            // 1. Get or Create Category
            if (!_categories.TryGetValue(categoryName, out var category)) {
                category = Instantiate(weaponCategoryPrefab, weaponCategoriesParent);
                category.DisableOutline(); // Default state
                _categories.Add(categoryName, category);
            }

            // 2. Add entry to category
            category.AddEntry(weaponEntryPrefab, data);
        }

        public void UpdateWeaponAmmo(string categoryName, WeaponData data, int amount) {
            if (!_categories.TryGetValue(categoryName, out var category)) {
                Debug.LogError("[WeaponSelectionView] Could not find category to update ammo: " + categoryName);
                return;
            }
            
            category.UpdateAmmo(data, amount);
        }

        public void UpdateCurrentAmmoDisplay(int amount) => SetCurrentAmmoText(amount);
        
        public void SelectWeapon(string categoryName, WeaponData data) {
            if (!_categories.TryGetValue(categoryName, out var category)) return;

            // Update current weapon name display
            currentWeaponNameText.text = data.name;

            // Deselect all
            foreach (var cat in _categories.Values) {
                cat.DisableOutline();
            }
            // Select target category
            category.EnableOutline();
                
            // Select specific entry in category (to show on screen, reorder)
            category.SelectEntry(data);
                
            // Scroll to category
            ScrollToElement(category.RectTransform);
        }
        
        void ScrollToElement(RectTransform target) {
            Canvas.ForceUpdateCanvases();
        
            var viewport = weaponSelectionScroll.viewport;
            var viewportRect = viewport.rect;
            
            // Borders
            var targetViewportPos = viewport.InverseTransformPoint(target.position);
            var targetRect = target.rect;
            var targetLeftEdge = targetViewportPos.x + targetRect.xMin;
            var targetRightEdge = targetViewportPos.x + targetRect.xMax;
        
            // Viewport-Edges
            var viewportLeftEdge = viewportRect.xMin;
            var viewportRightEdge = viewportRect.xMax;
        
            float offset;

            // Target is:
            if (targetLeftEdge < viewportLeftEdge) {
                // Left out of bounds
                offset = viewportLeftEdge - targetLeftEdge;
            }
            else if (targetRightEdge > viewportRightEdge) {
                // Right out of bounds
                offset = -(targetRightEdge - viewportRightEdge);
            }
            else {
                // Target is already fully visible
                return;
            }

            weaponCategoriesParent.localPosition = new Vector2(
                weaponCategoriesParent.localPosition.x + offset,
                weaponCategoriesParent.localPosition.y
            );
        }


        public void HideUI() => weaponSelectionMenu.gameObject.SetActive(false);
        public void ShowUI() => weaponSelectionMenu.gameObject.SetActive(true);

        public void UpdateShootingPower(float percent) {
            shootingPowerText.text = $"{percent:F0}%";
        }

        void SetCurrentAmmoText(int amount) {
            currentWeaponAmmoText.text = amount.ToString();
        }
    }
}
