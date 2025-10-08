using System.Collections.Generic;
using Common.Runtime._Scripts.Common.Runtime.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime.Authority {
    public class AuthorityManager : MonoBehaviour {
        [SerializeField] AuthorityData authorityData;
        [SerializeField] List<GameObject> players = new();
        [SerializeField] int startIndex;

        /// <summary>
        /// The information of the current index will not always represent the current active with authority,
        /// due to that the turn can be ended. So there is no active authority between a turn being ended and the next turn being started.
        /// </summary>
        int _currentIndex = -1;

        [Button]
        void SetAuthorityToFirstPlayer() {
            if(players.IsNullOrEmpty(true)) return;
            if (!players.DoesIndexExist(startIndex, true)) return;
            
            _currentIndex = startIndex;
            SetAuthorityTo(players[_currentIndex]);
        }

        void SetAuthorityTo(GameObject newPlayer) {
            if (newPlayer is null) return;
            if (authorityData is null) return;
            if (players.IsNullOrEmpty(true)) return;
            
            if (!players.Contains(newPlayer)) {
                Debug.LogError("AuthorityManager: Tried to set authority to a player that is not in the players list.");
                return;
            }
            
            authorityData.SetAuthority(newPlayer);
        }
        
        public void NextPlayer(GameObject previousPlayer) {
            if(previousPlayer is null) return;
            if (!players.Contains(previousPlayer)) {
                Debug.LogError("Tried to set authority from a player that is not in the players list.");
                return;
            }
            
            if(authorityData is null) return;
            
            if (players.IsNullOrEmpty(true)) return;
            var nextIndexWrapped = (_currentIndex + 1) % players.Count; // Wrap
            if(!players.DoesIndexExist(nextIndexWrapped, true)) return;
            
            if(!authorityData.HasAuthority(previousPlayer)) {
                Debug.LogWarning($"Tried changing authority without the previous player having authority. Previous player: {previousPlayer.name} | Current authority: {authorityData.Owner}");
                return;
            }
            
            SetAuthorityTo(players[nextIndexWrapped]);
            _currentIndex = nextIndexWrapped;
        }
        
        public void RequestEndTurn(GameObject player) {
            if (player is null) return;
            if (players.IsNullOrEmpty(true)) return;
            if (authorityData is null) return;
            
            // Dont let another player that doesnt have authority end the turn of the current player
            if (!authorityData.HasAuthority(player)) {
                Debug.LogWarning("AuthorityManager: Player requested end turn but does not have authority.");
                return;
            }

            authorityData.ResetAuthority();
        }
    }
}