using Gameplay.Runtime.Player;
using UnityEngine;

namespace Gameplay.Runtime
{
    public class SlowDownHazard : ConstantHazard
    {
        [SerializeField] private float slowDownFactor = 0.5f;
        
        protected override void EnterEffect(GameObject target, HazardData hazardData) {
            if (!target.TryGetComponent(out PlayerController playerController)) return;
            playerController.SetSpeedMultiplier(slowDownFactor);
        }
        protected override void ExitEffect(GameObject target, HazardData hazardData) {
            if (!target.TryGetComponent(out PlayerController playerController)) return;
            playerController.ResetSpeedMultiplier(slowDownFactor);
        }

    }
}
