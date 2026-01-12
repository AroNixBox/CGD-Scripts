using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Data")]
    public class ProjectileData: ScriptableObject {
        // TODO: ShotPattern (How many are fired, etc..)

        [SerializeField, HideInInspector] Projectile projectilePrefab;
        public Projectile ProjectilePrefab => projectilePrefab;

        [BoxGroup("General Settings")]
        [ShowInInspector, PreviewField(Alignment = ObjectFieldAlignment.Left)]
        [LabelText("Projectile Prefab"), AssetsOnly]
        [ValidateInput("HasProjectileComponent", "Prefab must have a 'Projectile' component!")]
        [PropertyOrder(-1)]
        GameObject ProjectilePrefabPreview {
            get => projectilePrefab != null ? projectilePrefab.gameObject : null;
            set {
                if (value == null) projectilePrefab = null;
                else if (value.TryGetComponent(out Projectile p)) projectilePrefab = p;
            }
        }

        [BoxGroup("General Settings")]
        [Title("Shared Configuration")]
        [GUIColor(0.9f, 1f, 0.9f)]
        [SerializeField, Required]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)] 
        ProjectileImpactData impactData;
        public ProjectileImpactData ImpactData => impactData;

        bool HasProjectileComponent(GameObject go) {
            if (go == null) return true;
            return go.GetComponent<Projectile>() != null;
        }

        [BoxGroup("Physics")]
        [SerializeField, Required, MinValue(0.001f)] 
        float mass = 1;
        public float Mass => mass;

        [BoxGroup("Physics")]
        [SerializeField, Required, MinValue(0f)] 
        float drag;
        public float Drag => drag;

        [BoxGroup("Visuals & Audio")]
        [SerializeField, AssetsOnly] 
        GameObject muzzleEffect;
        public GameObject MuzzleEffect => muzzleEffect;

        [BoxGroup("Visuals & Audio")]
        [SerializeField, AssetsOnly] 
        AudioClip firedSound;
        public AudioClip FiredSound => firedSound;
    }
}