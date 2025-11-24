using System;
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
        public event Action OnLastEntityRemaining = delegate { };
        AuthorityEntity _currentAuthority;
        // Track the last authority for GiveNextAuthority(), bec. _currentAuthority gets reset when in Bullet-Cam time
        int _nextAuthorityIndex;

        public void Init() {
            authorityEntities.ForEach(entity => entity.Initialize(this));
            SetAuthorityToFirstPlayer();

            return;
            
            void SetAuthorityToFirstPlayer() {
                if (authorityEntities.IsNullOrEmpty(true)) return;
                if (!authorityEntities.DoesIndexExist(startIndex, true)) return;
            
                GiveNextEntityAuthority();
            }
        }
        

        /// <summary>
        /// Advances to the next player in the list.
        /// </summary>
        public void GiveNextEntityAuthority() {
            if (_nextAuthorityIndex == -1) return; // Dont go in
            if (authorityEntities.IsNullOrEmpty(true)) return;
            // Does Index even exist?
            if (_nextAuthorityIndex >= authorityEntities.Count) return;
            
            var nextPlayer = authorityEntities[_nextAuthorityIndex];
            GiveAuthorityTo(nextPlayer);
            
            return;

            void GiveAuthorityTo(AuthorityEntity newAuthorityEntity) {
                if (newAuthorityEntity == null) {
                    Debug.LogError("AuthorityManager: Cannot set authority to null player.");
                    return;
                }
            
                if (!authorityEntities.Contains(newAuthorityEntity)) {
                    Debug.LogError("AuthorityManager: Tried to set authority to a player that is not registered.");
                    return;
                }

                var currentIndex = authorityEntities.IndexOf(newAuthorityEntity);
                _nextAuthorityIndex = (currentIndex + 1) % authorityEntities.Count;
                _currentAuthority = newAuthorityEntity;
            }
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
            
            _currentAuthority = null;
        }

        [Button]
        public void UnregisterEntity(int index) {
            // Does index even exist?
            if (index < 0 || index >= authorityEntities.Count) return;
            
            UnregisterEntity(authorityEntities[index]);
        }

        public void UnregisterEntity(AuthorityEntity authorityEntity) {
            if (!ValidateEntityForUnregister(authorityEntity)) return;

            var removedIndex = authorityEntities.IndexOf(authorityEntity);
            var hadAuthority = HasAuthority(authorityEntity);
    
            authorityEntities.Remove(authorityEntity);
            AdjustNextAuthorityIndex(removedIndex);
            HandleGameEndCondition();
            HandleAuthorityTransfer(hadAuthority);
        }
        bool ValidateEntityForUnregister(AuthorityEntity authorityEntity) {
            if (authorityEntity == null) {
                Debug.LogError("AuthorityEntity is null.");
                return false;
            }

            return authorityEntities.Contains(authorityEntity);
        }

        void AdjustNextAuthorityIndex(int removedIndex) {
            // Entity before next Index gets removed, shift index by 1 to the left
            if (removedIndex < _nextAuthorityIndex && _nextAuthorityIndex > 0) {
                _nextAuthorityIndex--;
            }
        }

        void HandleGameEndCondition() {
            if (authorityEntities.Count > 1) return;
            
            _nextAuthorityIndex = -1;
            OnLastEntityRemaining.Invoke();
            Debug.Log("<color=red>Last Entity has been Reached, Game Over?</color>");
        }

        void HandleAuthorityTransfer(bool hadAuthority) {
            if (hadAuthority) {
                _currentAuthority = null;
                GiveNextEntityAuthority();
            }
        }

        public bool HasAuthority(AuthorityEntity authorityEntity) {
            return ReferenceEquals(_currentAuthority, authorityEntity);
        }
    }
}