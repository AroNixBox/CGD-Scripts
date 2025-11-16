using Gameplay.Runtime.Player.Trajectory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class Weapon : MonoBehaviour {
        [Tooltip("Bullet Spawn Point")]
        [SerializeField, Required] public Transform muzzlePoint;

        public WeaponProperties GetWeaponProperties() {
            Debug.DrawLine(muzzlePoint.position, muzzlePoint.position + Vector3.up * 5f);
            
            return new WeaponProperties(
                muzzlePoint.forward,
                muzzlePoint.position
            );
        }
        
        // TODO:
        // Needs to be able to be rotated with the Camera
        // Or should this rather be done with the entire arm via IK
    }
}