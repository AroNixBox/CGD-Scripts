using Core.Runtime.Backend;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class UserHubEntryController : MonoBehaviour {
        [SerializeField, Required] UserHubEntryView view;

        public void Initialize(UserData userData) {
            view.SetUserIcon(userData.UserIcon);
            view.SetUsername(userData.Username);
        }
    }
}
