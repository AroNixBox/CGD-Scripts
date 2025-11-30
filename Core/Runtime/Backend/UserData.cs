using UnityEngine;

namespace Core.Runtime.Backend {
    [System.Serializable]
    public struct UserData {
        [SerializeField] string username;
        public string Username => username;
        [SerializeField] Sprite userIcon;
        public Sprite UserIcon => userIcon;
    }
}