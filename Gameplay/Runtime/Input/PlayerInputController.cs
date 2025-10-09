using Core.Runtime.Authority;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Input {
    public class PlayerInputController : MonoBehaviour { 
        [SerializeField, Required] AuthorityData authorityData;
        [SerializeField, Required] AuthorityManager authorityManager;
        
        [Button]
        void DoSomething() {
            if(authorityData is null) return;
            if(authorityManager is null) return;
            if (!authorityData.HasAuthority(gameObject)) {
                Debug.LogWarning("We do not have authority, cannot do anything!");
                return;
            }
            
            Debug.Log("We have authority, doing something!");
        }
        
        // TODO: Should not be here, but rather when the player presses the shoot button
        [Button]
        void EndTurn() {
            if(authorityData is null) return;
            if(authorityManager is null) return;
            if (!authorityData.HasAuthority(gameObject)) {
                Debug.LogWarning("We do not have authority, cannot end turn!");
                return;
            }
            
            authorityManager.RequestEndTurn(gameObject);
        }
        
        // TODO: Should not be here in PlayerInputController, but rather be called from after the bulletcam ends e.G.
        [Button]
        void StartNewTurn() {
            if(authorityData is null) return;
            if(authorityManager is null) return;
            
            authorityManager.NextPlayer(gameObject);
        }
    }
}
