using Core.Runtime.Authority;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Runtime {
    public class AoeForce : MonoBehaviour {
        [SerializeField, Required] Vector3 forceOriginOffset;
        [SerializeField] float forceDelay = 0.3f;
        [SerializeField] float explosionRadius = 4.5f;
        [SerializeField] float explosionForce = 150;
        [SerializeField] float upwardsModifier = 1f;

        void Start() => _ = ApplyForce();

        async UniTask ApplyForce() {
            await UniTask.Delay((int)(forceDelay * 1000));
            
            var hitColliders = new Collider[100];
            var forceOrigin = transform.position + forceOriginOffset.z * transform.forward;
            var size = Physics.OverlapSphereNonAlloc(forceOrigin, explosionRadius, hitColliders);

            for(var i = 0; i < size; i++) {
                var rb = hitColliders[i].attachedRigidbody;

                if (rb == null) { continue; }
                
                var entity = rb.GetComponent<AuthorityEntity>();
                
                var forceRadius = explosionRadius * .5f;

                if (entity == null) continue;
                
                rb.AddExplosionForce(explosionForce * .25f, forceOrigin, forceRadius, upwardsModifier,
                    ForceMode.Impulse);
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + forceOriginOffset.z * transform.forward, explosionRadius);
        }
    }
}
