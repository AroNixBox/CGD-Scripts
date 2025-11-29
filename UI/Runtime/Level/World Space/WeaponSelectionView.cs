using System;
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
        [SerializeField, Required] RectTransform weaponEntriesParent;
        readonly List<WeaponSelectionEntry> _weaponEntries = new();
        public void SpawnWeaponSelectionEntry(Sprite iconImage) {
            var weaponEntryInstance = Instantiate(weaponEntryPrefab, weaponEntriesParent.transform);
            weaponEntryInstance.SetIconImage(iconImage);
            _weaponEntries.Add(weaponEntryInstance);
        }

        public void EnableWeaponEntryOutline(int index) {
            var selectedEntry = _weaponEntries[index];
            
            // Outline
            _weaponEntries.ForEach(e => e.DeactivateOutline());
            selectedEntry.ActivateOutline();
            
            // Go To
            var targetRect = selectedEntry.GetComponent<RectTransform>();
            if (targetRect == null) throw new NotSupportedException("WeaponSelectionEntry has no Rect?!");
            ScrollToElement(targetRect);
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
        
            // Viewport-Grenzen
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

            weaponEntriesParent.localPosition = new Vector2(
                weaponEntriesParent.localPosition.x + offset,
                weaponEntriesParent.localPosition.y
            );
        }


        public void HideUI() => weaponSelectionMenu.gameObject.SetActive(false);
        public void ShowUI() => weaponSelectionMenu.gameObject.SetActive(true);
    }
}
