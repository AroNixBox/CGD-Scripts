using Core.Runtime.Authority;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Input {
    [RequireComponent(typeof(AuthorityEntity))]
    public class TestInputController : MonoBehaviour { 
        [SerializeField, Required] AuthorityManager authorityManager;
        AuthorityEntity _authorityEntity;

        void Awake() {
            _authorityEntity = GetComponent<AuthorityEntity>();
        }

        [Button]
        void DoSomething() {
            if(authorityManager is null) return;
            if (!authorityManager.HasAuthority(_authorityEntity)) {
                Debug.LogWarning("We do not have authority, cannot do something!");
                return;
            }
            
            Debug.Log("We have authority, doing something!");
        }
        
        // TODO: Should not be here, but rather when the player presses the shoot button
        [Button]
        void EndTurn() => authorityManager?.RequestEndTurn(_authorityEntity);
        
        // TODO: Should not be here in PlayerInputController, but rather be called from after the bulletcam ends e.G.
        [Button]
        void StartNewTurn() => authorityManager.NextPlayer(_authorityEntity);
    }
}
