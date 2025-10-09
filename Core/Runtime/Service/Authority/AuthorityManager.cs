using System.Collections.Generic;
using Common.Runtime._Scripts.Common.Runtime.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime.Authority {
    /// <summary>
    /// Manages turn-based authority between players.
    /// Caches the last owner to allow validation during the period between turn end and next turn start.
    /// </summary>
    public class AuthorityManager : MonoBehaviour {
        [SerializeField] AuthorityData authorityData;
        [SerializeField] List<GameObject> players = new();
        [SerializeField] int startIndex;
        
        // Cache the last owner to validate requests during the "no authority" period
        // (between ResetAuthority() and the next SetAuthority())
        object _lastKnownOwner;

        [Button]
        public void SetAuthorityToFirstPlayer() {
            if (players.IsNullOrEmpty(true)) return;
            if (!players.DoesIndexExist(startIndex, true)) return;
            
            SetAuthorityTo(players[startIndex], bypassValidation: true);
        }

        /// <summary>
        /// Sets the authority to a new player.
        /// </summary>
        /// <param name="newPlayer">The player to grant authority to</param>
        /// <param name="bypassValidation">If true, skips owner validation (used for initialization)</param>
        void SetAuthorityTo(GameObject newPlayer, bool bypassValidation = false) {
            if (newPlayer == null) {
                Debug.LogError("AuthorityManager: Cannot set authority to null player.");
                return;
            }
            
            if (authorityData == null) {
                Debug.LogError("AuthorityManager: AuthorityData is not assigned.");
                return;
            }
            
            if (!players.Contains(newPlayer)) {
                Debug.LogError("AuthorityManager: Tried to set authority to a player that is not registered.");
                return;
            }

            // Validate that only the last owner can trigger authority changes
            if (!bypassValidation && !IsLastKnownOwner(newPlayer)) {
                Debug.LogError("AuthorityManager: Only the last authority owner can set authority to a new player.");
                return;
            }
            
            _lastKnownOwner = newPlayer;
            authorityData.SetAuthority(newPlayer);
        }

        /// <summary>
        /// Advances to the next player in the list.
        /// </summary>
        /// <param name="currentPlayer">The player currently requesting the turn advance</param>
        public void NextPlayer(GameObject currentPlayer) {
            if (currentPlayer == null) {
                Debug.LogError("AuthorityManager: Current player is null.");
                return;
            }
            
            if (players.IsNullOrEmpty(true)) return;
            
            if (!players.Contains(currentPlayer)) {
                Debug.LogError("AuthorityManager: Current player is not in the players list.");
                return;
            }

            // Validate that the requesting player is the last known owner
            if (!IsLastKnownOwner(currentPlayer)) {
                Debug.LogError("AuthorityManager: Only the last authority owner can advance to the next player.");
                return;
            }
            
            var currentIndex = players.IndexOf(currentPlayer);
            var nextPlayer = players[(currentIndex + 1) % players.Count];
            
            SetAuthorityTo(nextPlayer, bypassValidation: true);
        }

        /// <summary>
        /// Requests to end the current turn, clearing authority.
        /// Authority will be empty until the next player is set.
        /// </summary>
        /// <param name="player">The player requesting to end their turn</param>
        public void RequestEndTurn(GameObject player) {
            if (player == null) {
                Debug.LogError("AuthorityManager: Player is null.");
                return;
            }
            
            if (authorityData == null) {
                Debug.LogError("AuthorityManager: AuthorityData is not assigned.");
                return;
            }
            
            // Validate that the player has current authority OR is the last known owner
            // This allows ending turn even during the "transition period"
            if (!authorityData.HasAuthority(player) && !IsLastKnownOwner(player)) {
                Debug.LogWarning("AuthorityManager: Player requested end turn but does not have authority.");
                return;
            }

            // Clear authority - entering the "transition period"
            // _lastKnownOwner remains cached for validation
            authorityData.ResetAuthority();
        }

        /// <summary>
        /// Checks if the given player was the last known authority owner.
        /// Used for validation during the period between ResetAuthority and SetAuthority.
        /// </summary>
        bool IsLastKnownOwner(GameObject player) {
            return _lastKnownOwner != null && ReferenceEquals(_lastKnownOwner, player);
        }
    }
}