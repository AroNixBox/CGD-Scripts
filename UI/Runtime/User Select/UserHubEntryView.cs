using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime {
    public class UserHubEntryView : MonoBehaviour {
        [SerializeField, Required] TMP_Text usernameTextField;
        [SerializeField, Required] Image userIconImage;
        [SerializeField, Required] Button removeButton;
        public event Action OnRemoveClicked = delegate { };
        void OnEnable() => removeButton.onClick.AddListener(RemoveClicked);
        void OnDisable() => removeButton.onClick.RemoveListener(RemoveClicked);

        void RemoveClicked() => OnRemoveClicked.Invoke();
        public void SetUserIcon(Sprite sprite) => userIconImage.sprite = sprite;
        public void SetUsername(string text) => usernameTextField.text = text;
    }
}
