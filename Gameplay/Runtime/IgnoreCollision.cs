using UnityEngine;

namespace Gameplay.Runtime {
    [RequireComponent(typeof(Collider))]
    public class IgnoreCollision : MonoBehaviour {
        [SerializeField] Collider[] collidersToIgnore;
        
        void Start() {
            var thisCollider = GetComponent<Collider>();
            if(thisCollider == null) {
                Debug.LogError("No collider found on " + gameObject.name);
                return;
            }
            
            foreach (var col in collidersToIgnore) {
                Physics.IgnoreCollision(thisCollider, col, true);
            }
        }
    }
}
