using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Hazards {
    /// <summary>
    /// Generic component for spawning VFX and SFX when an entity spawns.
    /// Attach directly to any AuthorityEntity prefab.
    /// </summary>
    public class SpawnHarzardEffect : MonoBehaviour {
        [Title("Spawn Effects")]
        [SerializeField, Required] public Player.Combat.Projectile projectilePrefab;
        [SerializeField, Required] Player.Combat.ProjectileImpactData impactData;

        [SerializeField, Tooltip("VFX prefab to spawn at entity spawn location")] GameObject vfx;
        [SerializeField] Vector3 vfxOffset;

        [SerializeField, Tooltip("SFX to play when entity spawns")] AudioClip sfx;
        [SerializeField, Range(0f, 1f)] protected float volume = 1f;

        public void Spawn() {
            Debug.Log("Spawning Hazard Effect");
            SpawnVfx();
            PlaySpawnSfx();

            var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.InitProxyProjectile(impactData);
        }

        void SpawnVfx() {
            if (vfx == null) return;

            Instantiate(vfx, transform.position + vfxOffset, Quaternion.identity);
        }

        void PlaySpawnSfx() {
            if (sfx == null) return;
            
            AudioSource.PlayClipAtPoint(sfx, transform.position, volume);
        }
    }
}

