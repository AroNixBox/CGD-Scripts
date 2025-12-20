using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime.Authority {
    /// <summary>
    /// Generic component for spawning VFX and SFX when an entity spawns.
    /// Attach directly to any AuthorityEntity prefab.
    /// </summary>
    public class EntitySpawnEffects : MonoBehaviour {
        [SerializeField, Required] AuthorityEntity entity;
        
        [Title("Spawn Effects")]
        [SerializeField, Tooltip("VFX prefab to spawn at entity spawn location")]
        GameObject spawnVfxPrefab;
        
        [SerializeField, Tooltip("SFX to play when entity spawns")]
        AudioClip spawnSfx;

        [SerializeField] Vector3 spawnVfxOffset;

        [SerializeField, Range(0f, 1f)] protected float volume = 1f;

        [SerializeField, Required] AudioSource audioSource;

        void OnEnable() {
            if (entity != null) {
                entity.OnSpawned += HandleSpawned;
            }
        }

        void OnDisable() {
            if (entity != null) {
                entity.OnSpawned -= HandleSpawned;
            }
        }

        void HandleSpawned() {
            SpawnVfx();
            PlaySpawnSfx();
        }

        void SpawnVfx() {
            if (spawnVfxPrefab == null) return;
            
            // Stick to parent, since it gets repositioned
            Instantiate(spawnVfxPrefab, transform.position + spawnVfxOffset, Quaternion.identity, transform);
        }

        void PlaySpawnSfx() {
            if (spawnSfx == null) return;
            
            audioSource.PlayOneShot(spawnSfx, volume);
        }
    }
}

