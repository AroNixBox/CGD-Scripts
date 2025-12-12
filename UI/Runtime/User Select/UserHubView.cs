using System;
using Core.Runtime.Backend;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime {
    public class UserHubView : MonoBehaviour {
        [SerializeField, Required] VerticalLayoutGroup userEntriesParent;
        [SerializeField, Required] RectTransform content;
        [SerializeField, Required] UserHubEntryController userHubEntryPrefab;
        [SerializeField, Required] Button addUserButton;
        [SerializeField, Required] Button startGameButton;
        
        public event Action OnUserAddPressed = delegate { };
        public event Action OnStartGamePressed = delegate { };

        void OnEnable() {
            addUserButton.onClick.AddListener(ForwardAdd);
            startGameButton.onClick.AddListener(ForwardPressed);
        }

        void OnDisable() {
            addUserButton.onClick.RemoveListener(ForwardAdd);
            startGameButton.onClick.RemoveListener(ForwardPressed);
        }

        void ForwardAdd() => OnUserAddPressed.Invoke();
        void ForwardPressed() => OnStartGamePressed.Invoke();

        public void Show() => content.gameObject.SetActive(true);
        public void Hide() => content.gameObject.SetActive(false);

        public void SpawnHubEntry(UserData user) {
            if (userEntriesParent == null || userHubEntryPrefab == null) return;

            var parentTransform = userEntriesParent.transform;
            var childCount = parentTransform.childCount;

            // Last element is a button? Leave it at last
            var insertIndex = childCount;
            if (childCount > 0) {
                var lastChild = parentTransform.GetChild(childCount - 1);
                if (lastChild != null && lastChild.GetComponent<Button>() != null) {
                    insertIndex = childCount - 1; // add before button
                }
            }

            var entryInstance = Instantiate(userHubEntryPrefab, parentTransform);
            entryInstance.transform.SetSiblingIndex(insertIndex);
            entryInstance.Initialize(user);
        }

        public void DespawnHubEntries() {
            if (userEntriesParent == null) return;

            // Iterate backwards
            for (var i = userEntriesParent.transform.childCount - 1; i >= 0; i--) {
                var child = userEntriesParent.transform.GetChild(i);
                // Dont delete button
                if (child != null && child.GetComponent<Button>() == null) {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
