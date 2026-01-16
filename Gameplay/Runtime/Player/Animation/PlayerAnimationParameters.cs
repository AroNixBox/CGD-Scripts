using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Runtime.Player.Animation {
    public static class AnimationParameters {
        // Params
        public static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");
        
        // States
        public static readonly int Locomotion = Animator.StringToHash("Locomotion");
        public static readonly int CastSpell = Animator.StringToHash("Cast Spell");
        public static readonly int Fall = Animator.StringToHash("Fall");
        public static readonly int Land = Animator.StringToHash("Land");
        public static readonly int TPose = Animator.StringToHash("T-Pose");
        public static readonly int Win = Animator.StringToHash("Win");
        
        static readonly Dictionary<int, int> AnimationLayers = new (){
            {Locomotion, 0}, 
            {Fall, 0},
            {Land, 0},
            {CastSpell, 0},
            {TPose, 0},
            {Win, 0}
        };
        public static int GetAnimationLayer(int animationHash) {
            return AnimationLayers.GetValueOrDefault(animationHash, 0);
        }
        
        static readonly Dictionary<int, float> AnimationDurations = new (){
            // .1f Seconds for fixing "snap", because crossFade sometimes lets one animation end
            // when the other starts, the ended one snaps into the startPos of the new one
            {Locomotion, 0.25f},
            {Fall, 0.25f},
            {Land, 0.25f},
            {CastSpell, 0.125f},
            {TPose, 0f},
            {Win, .25f},
        };
        
        public static float GetAnimationDuration(int animationHash) {
            return AnimationDurations.GetValueOrDefault(animationHash, 0.25f);
        }
    }
}