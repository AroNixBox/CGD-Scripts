using System;
using Core.Runtime.Service;
using UnityEngine;

namespace Gameplay.Core.Service {
    public class AudioManager : MonoBehaviour {
        void Awake() { 
            // TESTING PURPOSE
            if (ServiceLocator.TryGet(out AudioManager audioManager)) {
                Debug.LogError($"Duplicate AudioManager detected. Keeping '{audioManager.gameObject.name}' and destroying this instance.", transform);
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        void OnDestroy() {
            // Only unregister if this instance is the currently registered one.
            if (ServiceLocator.TryGet(out AudioManager current) && current == this) {
                ServiceLocator.Unregister<AudioManager>();
            }        
        }

        public void PlayClipAtPosition(AudioClip clip, Vector3 position, float volume = 1f) {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}
