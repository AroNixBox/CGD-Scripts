using Core.Runtime.Service;
using Gameplay.Core.Service;
using UnityEngine;

namespace Gameplay.Runtime._Scripts.Gameplay.Runtime.Audio {
    public class AudioInstance : MonoBehaviour {
        AudioManager _manager;
        void Start() {
            if (!ServiceLocator.TryGet(out _manager)) {
                Debug.LogError("No Audio Manager in Scene, wont work Properly");
                return;
            }
        }
        
        public void PlayClipAtPosition(AudioClip clip) {
            if (clip == null || _manager == null) return;
            const float volume = 1f;
            _manager.PlayClipAtPosition(clip, transform.position, volume);
        }

        public void PlayClipAtPosition(AudioClip clip, Vector3 position, float volume = 1f) {
            if (clip == null || _manager == null) return;
            _manager.PlayClipAtPosition(clip, position, volume);
        }
    }
}