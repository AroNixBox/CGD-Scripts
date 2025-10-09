using Common.Runtime.Extensions;
using UnityEngine;

namespace Gameplay.Runtime._Scripts.Gameplay.Runtime {
    public class ActiveRagdollController : MonoBehaviour {
        // TODO: Set them in Pairs?
        // TODO: Somehow the important setting in a SO, so if we wanna do that again, we can just
        // read them from a SO
        public Transform[] animatedTransforms;
        public ConfigurableJoint[] joints;
        Quaternion[] _initialJointLocalRotations;

        void Start() {
            _initialJointLocalRotations = new Quaternion[joints.Length];
            
            for (int i = 0; i < joints.Length; i++) {
                _initialJointLocalRotations[i] = joints[i].transform.localRotation;
                
                // TODO: Without these, active ragdoll doesnt work properly, set them in the inspector?
                var slerpDrive = joints[i].slerpDrive;
                slerpDrive.positionSpring = 1000f;
                slerpDrive.positionDamper = 50f;
                slerpDrive.maximumForce = Mathf.Infinity;
                joints[i].slerpDrive = slerpDrive;
            }
        }

        void FixedUpdate() {
            for (int i = 0; i < joints.Length; i++) {
                Quaternion targetLocalRotation = animatedTransforms[i + 1].localRotation;
                joints[i].SetTargetRotationLocal(targetLocalRotation, _initialJointLocalRotations[i]);
            }
        }
    }
}