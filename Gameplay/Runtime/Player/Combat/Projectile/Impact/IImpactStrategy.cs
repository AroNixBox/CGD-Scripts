using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public interface IImpactStrategy {
        public void OnImpact(Vector3 impactPosition);
    }
}