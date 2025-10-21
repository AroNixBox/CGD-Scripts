using System;
using Core.Runtime.Authority;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime {
    public class AoeForce : MonoBehaviour {
        [SerializeField, Required] Vector3 forceOriginOffset;
        const float ExplosionRadius = 4.5f;
        const float ExplosionForce = 150;
        const float UpwardsModifier = 1f;

        async void Start() {
            await System.Threading.Tasks.Task.Delay(300);
            ApplyForce();
        }

        void ApplyForce() {
            var hitColliders = new Collider[100];
            var forceOrigin = transform.position + forceOriginOffset.z * transform.forward;
            var size = Physics.OverlapSphereNonAlloc(forceOrigin, ExplosionRadius, hitColliders);

            for(var i = 0; i < size; i++) {
                var rb = hitColliders[i].attachedRigidbody;

                if (rb == null) { continue; }
                
                Debug.Log($"Applying explosion force to {rb.name}");

                var entity = rb.GetComponent<AuthorityEntity>();
                
                const float forceRadius = ExplosionRadius * .5f;
                
                if (entity == null) {
                    rb.AddExplosionForce(ExplosionForce * .25f, forceOrigin, forceRadius, UpwardsModifier,
                        ForceMode.Impulse);
                    continue;
                }
                
                rb.AddExplosionForce(ExplosionForce, forceOrigin, forceRadius, UpwardsModifier,
                    ForceMode.Impulse);
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + forceOriginOffset.z * transform.forward, ExplosionRadius);
        }
    }
}
