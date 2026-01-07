using System;
using UnityEngine;

namespace Core.Runtime.Backend {
    [Serializable]
    public struct UserData : IEquatable<UserData> {
        [SerializeField] string username;
        public string Username {
            get => username;
            set => username = value;
        }

        [SerializeField] Sprite userIcon;
        public Sprite UserIcon {
            get => userIcon;
            set => userIcon = value;
        }
        public bool Equals(UserData other) {
            return username == other.username && Equals(userIcon, other.userIcon);
        }

        public override bool Equals(object obj) {
            return obj is UserData other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(username, userIcon);
        }
    }
}