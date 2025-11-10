using Common.Runtime.Extensions;
using UnityEngine;

namespace Gameplay.Runtime.Player {
    public class ActiveRagdollAnimator : MonoBehaviour {
        public Transform[] animatedTransforms;
        public ConfigurableJoint[] joints;
        Quaternion[] _initialJointLocalRotations;

        void Start() {
            _initialJointLocalRotations = new Quaternion[joints.Length];
            
            for (int i = 0; i < joints.Length; i++) {
                _initialJointLocalRotations[i] = joints[i].transform.localRotation;
            }
        }

        void FixedUpdate() {
            for (int i = 0; i < joints.Length; i++) {
                Quaternion targetLocalRotation = animatedTransforms[i].localRotation;
                joints[i].SetTargetRotationLocal(targetLocalRotation, _initialJointLocalRotations[i]);
            }
        }
    }
}