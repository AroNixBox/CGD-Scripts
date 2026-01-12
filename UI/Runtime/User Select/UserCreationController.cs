using System;
using Core.Runtime;
using Core.Runtime.Backend;
using Core.Runtime.Data;
using Core.Runtime.Service;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class UserCreationController : MonoBehaviour {
        [SerializeField, Required] UserCreationView view;
        [SerializeField, Required][InlineEditor] CharacterDatabase characterDatabase;
        public event Action OnSubmitUser = delegate { };
        int _currentSpriteIndex = -1;
        string _currentUsername;

        void OnEnable() {
            view.OnNextIcon += OnNextIcon;
            view.OnPreviousIcon += PrevIcon;
            view.OnUsernameChanged += ChangeOnUsername;
            view.OnSubmitUser += Submit;

            InitializeIcon();

            return;
            
            void InitializeIcon() {
                if (characterDatabase != null && characterDatabase.Count > 0) {
                    _currentSpriteIndex = 0;
                    view.SetIcon(characterDatabase.GetIconAtIndex(_currentSpriteIndex));
                } else {
                    Debug.LogWarning("[UserCreationController] no availableIcons assigned");
                }
            }
        }

        void OnDisable() {
            view.OnNextIcon -= OnNextIcon;
            view.OnPreviousIcon -= PrevIcon;
            view.OnUsernameChanged -= ChangeOnUsername;
            view.OnSubmitUser -= Submit;
        }
        public void Show() => view.Show();
        public void Hide() => view.Hide();
        void OnNextIcon() => ChangeIndex(1);
        void PrevIcon() => ChangeIndex(-1);
        void ChangeOnUsername(string username) => _currentUsername = username;

        void Submit() {
            // invalid data? Noop
            if (string.IsNullOrWhiteSpace(_currentUsername)) {
                Debug.LogWarning("[UserCreationController] Submit aborted: username empty");
                return;
            }

            if (_currentSpriteIndex < 0 || characterDatabase == null || _currentSpriteIndex >= characterDatabase.Count) {
                Debug.LogWarning("[UserCreationController] Submit aborted: no sprite selected");
                return;
            }

            if (!ServiceLocator.TryGet(out GameManager gameManager)) {
                Debug.LogError("[UserCreationController] Submit aborted: GameManager not found in ServiceLocator");
                return;
            }
            
            var newUser = new UserData {
                UserIcon = characterDatabase.GetIconAtIndex(_currentSpriteIndex),
                Username = _currentUsername
            };
            gameManager.AddUserData(newUser);
            
            OnSubmitUser.Invoke();
            view.Hide();
        }

        void ChangeIndex(int delta) {
            if (characterDatabase == null || characterDatabase.Count == 0) return;

            if (_currentSpriteIndex < 0) {
                _currentSpriteIndex = delta > 0 ? 0 : characterDatabase.Count - 1;
            } else {
                int n = characterDatabase.Count;
                _currentSpriteIndex = (_currentSpriteIndex + delta + n) % n;
            }

            var sprite = characterDatabase.GetIconAtIndex(_currentSpriteIndex);
            view?.SetIcon(sprite);
        }
    }
}