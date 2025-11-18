using System.Collections.Generic;
using Common.Runtime._Scripts.Common.Runtime.Extensions;
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
        AuthorityEntity _lastKnownAuthority;
        
        /*
         * Cache the last owner to validate requests during the "no authority" period
         * (between ResetAuthority() and the next SetAuthority())
         * => Because after a player made his move there is a Bulletcam where no one has authority */


        public void Init() {
            authorityEntities.ForEach(entity => entity.Initialize(this));
            SetAuthorityToFirstPlayer();
        }

        void SetAuthorityToFirstPlayer() {
            if (authorityEntities.IsNullOrEmpty(true)) return;
            if (!authorityEntities.DoesIndexExist(startIndex, true)) return;
            
            SetAuthorityTo(authorityEntities[startIndex], null, true);
        }

        /// <summary>
        /// Sets the authority to a new player.
        /// </summary>
        /// <param name="newAuthorityEntity">The player to grant authority to</param>
        /// <param name="bypassValidation">If true, skips checking if the request comes from the actual authority</param>
        void SetAuthorityTo(AuthorityEntity newAuthorityEntity, AuthorityEntity authoritySetRequester,bool bypassValidation = false) {
            if (newAuthorityEntity == null) {
                Debug.LogError("AuthorityManager: Cannot set authority to null player.");
                return;
            }
            
            if (!authorityEntities.Contains(newAuthorityEntity)) {
                Debug.LogError("AuthorityManager: Tried to set authority to a player that is not registered.");
                return;
            }

            // Validate that only the last owner can trigger authority changes
            if (!bypassValidation && !IsLastKnownOwner(authoritySetRequester)) {
                Debug.LogError($"Only the last authority ({_lastKnownAuthority?.name}) can set authority, not {authoritySetRequester?.name}.");
                return;
            }

            _lastKnownAuthority = newAuthorityEntity;
            _currentAuthority = newAuthorityEntity;
        }

        /// <summary>
        /// Advances to the next player in the list.
        /// </summary>
        /// <param name="currentAuthorityEntity">The player currently requesting the turn advance</param>
        public void GiveNextEntityAuthority(AuthorityEntity currentAuthorityEntity) {
            if (currentAuthorityEntity == null) {
                Debug.LogError("AuthorityManager: Current player is null.");
                return;
            }
            
            if (authorityEntities.IsNullOrEmpty(true)) return;
            
            if (!authorityEntities.Contains(currentAuthorityEntity)) {
                Debug.LogError("AuthorityManager: Current player is not in the players list.");
                return;
            }
            
            var currentIndex = authorityEntities.IndexOf(currentAuthorityEntity);
            var nextPlayer = authorityEntities[(currentIndex + 1) % authorityEntities.Count];
            
            SetAuthorityTo(nextPlayer, currentAuthorityEntity);
        }

        /// <summary>
        /// Requests to end the current turn, clearing authority.
        /// Authority will be empty until the next player is set.
        /// </summary>
        /// <param name="authorityEntity">The player requesting to end their turn</param>
        public void ResetAuthority(AuthorityEntity authorityEntity) {
            if (authorityEntity == null) {
                Debug.LogError("AuthorityEntity is null.");
                return;
            }
            
            // Validate that the player has current authority OR is the last known owner
            // This allows ending turn even during the "transition period", where there is no current authority
            if (!HasAuthority(authorityEntity) && !IsLastKnownOwner(authorityEntity)) {
                Debug.LogWarning("AuthorityEntity requested end turn but does not have authority.");
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
            return _lastKnownAuthority != null && ReferenceEquals(_lastKnownAuthority, authorityEntity);
        }
    }
}