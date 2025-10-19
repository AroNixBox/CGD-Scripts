using Core.Runtime.Authority;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime {
    public class ShockwaveExploder : MonoBehaviour {
        [Header("Shockwave Settings")]
        public float explosionForce = 20;
        public float explosionRadius = 10f;
        public float upwardsModifier = 1f;
        

        void Update() {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                TriggerShockwave();
            }
        }

        void TriggerShockwave() {
            var hitColliders = new Collider[100];
            Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, hitColliders);

            foreach (Collider col in hitColliders) {
                Rigidbody rb = col.attachedRigidbody;

                if (rb == null || rb.transform.IsChildOf(transform) || rb.transform == transform) {
                    continue;
                }

                AuthorityEntity entity = rb.GetComponent<AuthorityEntity>();
                if (entity == null) {
                    rb.AddExplosionForce(explosionForce * .25f, transform.position, explosionRadius, upwardsModifier,
                        ForceMode.Impulse);
                    continue;
                }
                
                // EXPLOSION FORCE! 💥
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier,
                    ForceMode.Impulse);
            }

            System.Collections.IEnumerator Reenable(Rigidbody rb, PlayerMover mover, PlayerController controller, float delay) {
                yield return new WaitForSeconds(delay);
                mover.enabled = true;
                controller.enabled = true;
                rb.freezeRotation = true;
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
