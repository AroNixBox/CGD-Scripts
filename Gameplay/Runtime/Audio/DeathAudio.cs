using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Audio {
    public class DeathAudio : BaseAudio {
        [SerializeField, Required, ValidateInput(nameof(ValidateDamageable), "Reference needs to implement IDamageable", InfoMessageType.Error)]
        MonoBehaviour damageable;
        bool ValidateDamageable(MonoBehaviour mb) => mb != null && mb is IDamageable;

        IDamageable _damageable;

        void Awake() {
            if (damageable is null) Debug.LogError("No damageable referenced", transform);
            
            _damageable = damageable as IDamageable;
        }

        void OnEnable() {
            if (_damageable != null) _damageable.OnHealthDepleted += PlayDeathSound;
        }

        void OnDisable() {
            if (_damageable != null) _damageable.OnHealthDepleted -= PlayDeathSound;
        }

        void PlayDeathSound(float _) {
            var pos = damageable != null ? damageable.transform.position : transform.position;
            PlaySoundAtPosition(pos);
        }
    }
}