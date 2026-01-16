using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Animation {
    public class PlayerAnimatorController : MonoBehaviour {
        [SerializeField, Required] Animator animator;
        
        public void UpdateAnimatorSpeed(float speed) {
            animator.SetFloat(AnimationParameters.MovementSpeed, speed);
        }
        /// <summary>
        /// Can be used to check if the animation is finished playing
        /// </summary>
        /// <returns>
        /// True if the animation is not the current one or if it's almost finished playing
        /// </returns>
        public bool IsAnimationFinished(int stateHashName, float endThreshold) {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(AnimationParameters.GetAnimationLayer(stateHashName));
            return stateInfo.shortNameHash != stateHashName || stateInfo.normalizedTime >= endThreshold;
        }
        public bool IsCurrentAnimationFinished(float endThreshold, int layer) {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.normalizedTime >= endThreshold;
        }

        public void SetRootMotion(bool enabled) {
            animator.applyRootMotion = enabled;
        }

        public bool IsInTransition(int layer) {
            return animator.IsInTransition(layer);
        }
        
        /// <param name="forceChange">Transition into an animation even if it's already playing</param>
        public void ChangeAnimationState(int stateHashName) {
            animator.CrossFade(
                stateHashName, 
                AnimationParameters.GetAnimationDuration(stateHashName), 
                AnimationParameters.GetAnimationLayer(stateHashName));
        }
        public float GetAnimatorFloat(int parameter) => animator.GetFloat(parameter);
    }
}
