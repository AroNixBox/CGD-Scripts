using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Runtime {
    public class UserCreationView : MonoBehaviour {
        [SerializeField, Required] RectTransform content;
        [SerializeField, Required] Image iconImage;
        [SerializeField, Required] Button nextIconButton;
        [SerializeField, Required] Button prevIconButton;
        [SerializeField, Required] TMP_InputField inputField;
        [SerializeField, Required] Button submitUserButton;

        public event Action OnNextIcon = delegate { };
        void ForwardNext() => OnNextIcon.Invoke();
        public event Action OnPreviousIcon = delegate { };
        void ForwardPrev() => OnPreviousIcon.Invoke();
        public event Action<string> OnUsernameChanged = delegate { };
        void ForwardUsernameChanged(string s) => OnUsernameChanged.Invoke(s);
        public event Action OnSubmitUser = delegate { };
        void ForwardAdd() => OnSubmitUser.Invoke();

        void OnEnable() {
            nextIconButton.onClick.AddListener(ForwardNext);
            prevIconButton.onClick.AddListener(ForwardPrev);
            inputField.onValueChanged.AddListener(ForwardUsernameChanged);
            submitUserButton.onClick.AddListener(ForwardAdd);
        }
        
        public void Show() => content.gameObject.SetActive(true);
        public void Hide() {
            // Clear Inputfield
            inputField.text = string.Empty;
            
            content.gameObject.SetActive(false);
        }

        void OnDisable() {
            nextIconButton.onClick.RemoveListener(ForwardNext);
            prevIconButton.onClick.RemoveListener(ForwardPrev);
            inputField.onValueChanged.RemoveListener(ForwardUsernameChanged);
            submitUserButton.onClick.RemoveListener(ForwardAdd);
        }

        public void SetIcon(Sprite sprite) {
            iconImage.sprite = sprite;
        }
    }
}
