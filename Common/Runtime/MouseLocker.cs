using UnityEngine;

namespace Common.Runtime {
    public class MouseLocker : MonoBehaviour {
        private void Awake() {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
