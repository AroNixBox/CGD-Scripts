using UnityEngine;

namespace Gameplay.Runtime.Audio {
    public abstract class BaseAudio : MonoBehaviour {
        [SerializeField] protected AudioClip clip;
        [SerializeField, Range(0f, 1f)] protected float volume = 1f;

        protected virtual void PlaySoundAtPosition(Vector3 position) {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        protected virtual void PlaySoundAtThisPosition() {
            PlaySoundAtPosition(transform.position);
        }
    }
}
