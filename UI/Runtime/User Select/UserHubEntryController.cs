using Core.Runtime;
using Core.Runtime.Backend;
using Core.Runtime.Service;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class UserHubEntryController : MonoBehaviour {
        [SerializeField, Required] UserHubEntryView view;
        UserData _userData;
        
        public UserData UserData => _userData;

        public void Initialize(UserData userData) {
            view.SetUserIcon(userData.UserIcon);
            view.SetUsername(userData.Username);

            _userData = userData;

            view.OnRemoveClicked += RemoveUser;
        }

        void RemoveUser() {
            if (ServiceLocator.TryGet(out GameManager gameManager)) {
                gameManager.RemoveUserData(_userData);
            }
            
            Destroy(gameObject);
        }
        
        void OnDestroy() {
            view.OnRemoveClicked -= RemoveUser;
        }
    }
}
