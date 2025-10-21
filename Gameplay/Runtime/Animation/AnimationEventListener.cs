using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Runtime {
    public class AnimationEventListener : MonoBehaviour {
        [SerializeField] List<AnimationEventAction> specialAnimEvents = new();
        void OnAction(string eventName) {
            var matchedEvent = specialAnimEvents.Find(e => e.eventName == eventName);

            matchedEvent?.action?.Invoke();
            matchedEvent?.InstantiateObject();
        }
    }
}
