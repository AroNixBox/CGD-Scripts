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
        [SerializeField] List<AuthorityEntity> authorityEntities = new();
        [SerializeField] int startIndex;
        AuthorityEntity _currentAuthority;
        AuthorityEntity _nextAuthority;
        
        /* Cache the last owner to validate requests during the "no authority" period
         * (between ResetAuthority() and the next SetAuthority())
         * => Because after a player made his move there is a Bulletcam where no one has authority
         * TODO: Maybe Reset the entire Player Statemachine/ Discord it and when regaining authority restart it?
         * But then we would have no gravity / physics during that time as well... so maybe not.
         * Is it really necessary to disable the entire State Machine or is it enough to cut the inputs?
        */
        object _lastKnownOwner;

        [Button]
        public void SetAuthorityToFirstPlayer() {
            if (authorityEntities.IsNullOrEmpty(true)) return;
            if (!authorityEntities.DoesIndexExist(startIndex, true)) return;
            
            SetAuthorityTo(authorityEntities[startIndex], bypassValidation: true);
        }

        /// <summary>
        /// Sets the authority to a new player.
        /// </summary>
        /// <param name="newAuthorityEntity">The player to grant authority to</param>
        /// <param name="bypassValidation">If true, skips owner validation (used for initialization)</param>
        void SetAuthorityTo(AuthorityEntity newAuthorityEntity, bool bypassValidation = false) {
            if (newAuthorityEntity == null) {
                Debug.LogError("AuthorityManager: Cannot set authority to null player.");
                return;
            }
            
            if (!authorityEntities.Contains(newAuthorityEntity)) {
                Debug.LogError("AuthorityManager: Tried to set authority to a player that is not registered.");
                return;
            }

            // Validate that only the last owner can trigger authority changes
            if (!bypassValidation && !IsLastKnownOwner(newAuthorityEntity)) {
                Debug.LogError("AuthorityManager: Only the last authority owner can set authority to a new player.");
                return;
            }
            
            _lastKnownOwner = newAuthorityEntity;
            _currentAuthority = newAuthorityEntity;
        }

        /// <summary>
        /// Advances to the next player in the list.
        /// </summary>
        /// <param name="currentAuthorityEntity">The player currently requesting the turn advance</param>
        public void NextPlayer(AuthorityEntity currentAuthorityEntity) {
            if (currentAuthorityEntity == null) {
                Debug.LogError("AuthorityManager: Current player is null.");
                return;
            }
            
            if (authorityEntities.IsNullOrEmpty(true)) return;
            
            if (!authorityEntities.Contains(currentAuthorityEntity)) {
                Debug.LogError("AuthorityManager: Current player is not in the players list.");
                return;
            }

            // Validate that the requesting player is the last known owner
            if (!IsLastKnownOwner(currentAuthorityEntity)) {
                Debug.LogError("AuthorityManager: Only the last authority owner can advance to the next player.");
                return;
            }
            
            var currentIndex = authorityEntities.IndexOf(currentAuthorityEntity);
            var nextPlayer = authorityEntities[(currentIndex + 1) % authorityEntities.Count];
            
            SetAuthorityTo(nextPlayer, bypassValidation: true);
        }

        /// <summary>
        /// Requests to end the current turn, clearing authority.
        /// Authority will be empty until the next player is set.
        /// </summary>
        /// <param name="currentAuthorityEntity">The player requesting to end their turn</param>
        public void RequestEndTurn(AuthorityEntity currentAuthorityEntity) {
            if (currentAuthorityEntity == null) {
                Debug.LogError("AuthorityManager: Player is null.");
                return;
            }
            
            // Validate that the player has current authority OR is the last known owner
            // This allows ending turn even during the "transition period"
            if (!HasAuthority(currentAuthorityEntity) && !IsLastKnownOwner(currentAuthorityEntity)) {
                Debug.LogWarning("AuthorityManager: Player requested end turn but does not have authority.");
                return;
            }

            // Clear authority - entering the "transition period"
            // _lastKnownOwner remains cached for validation
            _currentAuthority = null;
        }
        
        public bool HasAuthority(AuthorityEntity candidate) => ReferenceEquals(_currentAuthority, candidate);

        /// <summary>
        /// Checks if the given player was the last known authority owner.
        /// Used for validation during the period between ResetAuthority and SetAuthority.
        /// </summary>
        bool IsLastKnownOwner(AuthorityEntity authorityEntity) {
            return _lastKnownOwner != null && ReferenceEquals(_lastKnownOwner, authorityEntity);
        }
    }
}