using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Gameplay.Runtime {
    public class PlayerAudioController : MonoBehaviour {
        List<AudioSource> _audioSources = new();

        public void PlayAudioClip(AudioResource audioResource) {
            var availableSource = GetOrCreateAudioSource();
            availableSource.resource = audioResource;
            availableSource.Play();
        }

        AudioSource GetOrCreateAudioSource() {
            var availableSource = _audioSources.FirstOrDefault(s => !s.isPlaying);
            
            if (availableSource == null) {
                availableSource = CreateNewAudioSource();
                _audioSources.Add(availableSource);
            }
            
            return availableSource;
        }

        AudioSource CreateNewAudioSource() {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 25f;
            return audioSource;
        }
    }
}