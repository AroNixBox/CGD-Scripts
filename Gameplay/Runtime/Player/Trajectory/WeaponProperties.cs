using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    public struct WeaponProperties {
        public Vector3 ShootDirection { get; }
        public Vector3 MuzzlePosition { get; }

        public WeaponProperties(Vector3 shootDirection, Vector3 muzzlePosition) {
            ShootDirection = shootDirection;
            MuzzlePosition = muzzlePosition;
        }
    }
}