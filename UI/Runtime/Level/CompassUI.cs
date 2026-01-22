using UnityEngine;

namespace UI.Runtime {
    public class CompassUI : MonoBehaviour {
        [SerializeField] private Transform viewDirection;
        [SerializeField] private RectTransform compassElements;
        [SerializeField] private float compassSize;

        private void LateUpdate() {
            Vector3 forwardVector = Vector3.ProjectOnPlane(viewDirection.forward, Vector3.up).normalized;
            float forwardSignedAngle = Vector3.SignedAngle(forwardVector, Vector3.forward, Vector3.up);
            float compassOffset = (forwardSignedAngle / 180f) * compassSize;
            compassElements.anchoredPosition = new Vector3(compassOffset, 0);
        }
    }
}
