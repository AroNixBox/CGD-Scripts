using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Animation {
    public class PlayerAnimatorController : MonoBehaviour {
        [SerializeField, Required] Animator animator;
        
        public void ChangeAnimationClipSpeed(int speedMultiplierParam, float newSpeed) {
            animator.SetFloat(speedMultiplierParam, newSpeed);
        }
        public void UpdateAnimatorSpeed(float speed) {
            animator.SetFloat(AnimationParameters.MovementSpeed, speed);
        }
        public AnimatorStateInfo GetCurrentAnimationState(int animationLayer) {
            return animator.GetCurrentAnimatorStateInfo(animationLayer);
        }
        /// <summary>
        /// Can be used to check if the animation is finished playing
        /// </summary>
        public bool IsAnimationFinished(int animationLayer) {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
            return stateInfo.normalizedTime >= .9f;
        }
        /// <param name="forceChange">Transition into an animation even if it's already playing</param>
        public void ChangeAnimationState(int stateHashName) {
            animator.CrossFade(
                stateHashName, 
                AnimationParameters.GetAnimationDuration(stateHashName), 
                AnimationParameters.GetAnimationLayer(stateHashName));
        }
        public void EnableRootMotion(bool enable) => animator.applyRootMotion = enable;
        public float GetAnimatorFloat(int parameter) => animator.GetFloat(parameter);

        public bool IsInTransition(int layer) {
            return animator.IsInTransition(layer);
        }
    }
}
