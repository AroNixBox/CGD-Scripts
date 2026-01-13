using UnityEngine;

namespace Core.Runtime.Visuals {
    public class CharacterVisuals : MonoBehaviour {
        [SerializeField] Transform headBone;
        public Transform HeadBone => headBone;
    }
}

