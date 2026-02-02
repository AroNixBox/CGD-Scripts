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

        readonly Dictionary<ProjectileData.ProjectileCategory, WeaponSelectionCategory> _categories = new();

        public void AddWeapon(ProjectileData.ProjectileCategory categoryName, WeaponData data) {
            // 1. Get or Create Category
            if (!_categories.TryGetValue(categoryName, out var category)) {
                category = Instantiate(weaponCategoryPrefab, weaponCategoriesParent);
                category.DisableOutline(); // Default state
                _categories.Add(categoryName, category);
            }

            // 2. Add entry to category
            category.AddEntry(weaponEntryPrefab, data);
        }

        public void UpdateCurrentAmmoDisplay(int amount) => SetCurrentAmmoText(amount);
        
        public void SelectWeapon(ProjectileData.ProjectileCategory categoryName, WeaponData data) {
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
        
            var content = weaponCategoriesParent;
            var viewport = weaponSelectionScroll.viewport;
            
            // Check if target is already visible
            var targetViewportPos = viewport.InverseTransformPoint(target.position);
            var targetRect = target.rect;
            var viewportRect = viewport.rect;
            
            var targetLeftEdge = targetViewportPos.x + targetRect.xMin;
            var targetRightEdge = targetViewportPos.x + targetRect.xMax;
            var viewportLeftEdge = viewportRect.xMin;
            var viewportRightEdge = viewportRect.xMax;
            
            // Element is already fully visible, don't scroll
            if (targetLeftEdge >= viewportLeftEdge && targetRightEdge <= viewportRightEdge) {
                return;
            }
            
            // Calculate normalized position (0 = left, 1 = right)
            var contentWidth = content.rect.width;
            var viewportWidth = viewport.rect.width;
            var scrollableWidth = contentWidth - viewportWidth;
            
            // If content fits in viewport, no scrolling needed
            if (scrollableWidth <= 0) {
                return;
            }
            
            // Get target position in content space
            var targetPosInContent = target.localPosition.x;
            var targetWidth = target.rect.width;
            
            // Calculate where to scroll to center the target
            var desiredContentPos = -targetPosInContent + (viewportWidth - targetWidth) * 0.5f;
            
            // Clamp to valid scroll range
            desiredContentPos = Mathf.Clamp(desiredContentPos, -scrollableWidth, 0);
            
            // Apply normalized position
            var normalizedPos = Mathf.Abs(desiredContentPos) / scrollableWidth;
            weaponSelectionScroll.horizontalNormalizedPosition = normalizedPos;
        }


        public void HideUI() => weaponSelectionMenu.gameObject.SetActive(false);
        public void ShowUI() => weaponSelectionMenu.gameObject.SetActive(true);

        public void UpdateShootingPower(float percent) {
            shootingPowerText.text = $"{percent:F0}%";
        }

        void SetCurrentAmmoText(int amount) {
            if (amount > 100) {
                currentWeaponAmmoText.text = "Ammo: ∞";
            } else {
                currentWeaponAmmoText.text = "Ammo: " + amount;
            }
        }
    }
}
