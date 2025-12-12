using System;
using System.Collections.Generic;
using Common.Runtime._Scripts.Common.Runtime.Extensions;
using Core.Runtime.Backend;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime.Authority {
    /// <summary>
    /// Manages turn-based authority between players.
    /// Caches the last owner to allow validation during the period between turn end and next turn start.
    /// </summary>
    public class AuthorityManager : MonoBehaviour {
        [SerializeField, Required] AuthorityEntity authorityEntityPrefab;
        [SerializeField, Required] List<Transform> authorityEntitiesSpawnPoints;
        [SerializeField, Tooltip("Delay in seconds between each entity spawn")]
        float spawnInterval = 1.5f;
        [SerializeField] int startIndex;
        readonly List<AuthorityEntity> _authorityEntities = new();
        readonly List<Transform> _availableSpawnPoints = new();

        #region Events

        public static event Action<AuthorityEntity> OnEntitySpawned = delegate { };
        public event Action<AuthorityEntity> OnEntityAuthorityGained = delegate { }; // Start Turn
        public event Action<AuthorityEntity> OnEntityAuthorityRevoked = delegate { }; // End Turn
        public static event Action<AuthorityEntity> OnEntityDied = delegate { };
        public event Action<AuthorityEntity> OnLastEntityRemaining = delegate { }; // 

        #endregion
        
        AuthorityEntity _currentAuthority;
        // Track the last authority for GiveNextAuthority(), bec. _currentAuthority gets reset when in Bullet-Cam time
        int _nextAuthorityIndex;

        void Awake() {
            ServiceLocator.Register(this);
        }

        async void OnEnable() {
            if (!ServiceLocator.TryGet(out GameManager gameManager)) {
                await UniTask.WaitUntil(() => ServiceLocator.TryGet(out gameManager));
                if (gameManager == null) return;
            }

            gameManager.OnGameInit += HandleInit;
            gameManager.OnGameStart += StartFlow;
        }

        void OnDisable() {
            if (!ServiceLocator.TryGet(out GameManager gameManager))
                return;

            gameManager.OnGameInit += HandleInit;
            gameManager.OnGameStart += StartFlow;
        }
        
        void HandleInit(object sender, GameManager.GameInitEventArgs args) {
            args.CompletionTasks.Add(InitializeEntities(args.UserDatas));
        }

        async UniTask InitializeEntities(List<UserData> userDatas) {
            // Initialize available spawnpoints pool
            _availableSpawnPoints.Clear();
            _availableSpawnPoints.AddRange(authorityEntitiesSpawnPoints);

            foreach (var userData in userDatas) {
                // Get unique spawnpoint (recycle if all are used)
                var spawnPoint = GetNextSpawnPoint();
                
                // Spawn Entity & Init
                var spawnedEntity = Instantiate(authorityEntityPrefab, spawnPoint.position, spawnPoint.rotation);
                spawnedEntity.Initialize(this, userData);
                
                _authorityEntities.Add(spawnedEntity);
                OnEntitySpawned.Invoke(spawnedEntity);
                
                // Wait before spawning next entity
                await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), ignoreTimeScale: false);
            }
        }

        Transform GetNextSpawnPoint() {
            // If all spawnpoints used, refill the pool
            if (_availableSpawnPoints.Count == 0) {
                _availableSpawnPoints.AddRange(authorityEntitiesSpawnPoints);
            }

            // Pick random from available and remove it
            var randomIndex = UnityEngine.Random.Range(0, _availableSpawnPoints.Count);
            var selectedSpawnPoint = _availableSpawnPoints[randomIndex];
            _availableSpawnPoints.RemoveAt(randomIndex);

            return selectedSpawnPoint;
        }
        
        void StartFlow() {
            // First Player Auth
            if (_authorityEntities.IsNullOrEmpty(true)) return;
            if (!_authorityEntities.DoesIndexExist(startIndex, true)) return;
           
            GiveNextEntityAuthority();
        }
        
        void OnDestroy() => ServiceLocator.Unregister<AuthorityManager>();

        
        /// <summary>
        /// Advances to the next player in the list.
        /// </summary>
        [Button, BoxGroup("Debug")]
        public void GiveNextEntityAuthority() {
            if (_nextAuthorityIndex == -1) return; // Dont go in
            if (_authorityEntities.IsNullOrEmpty(true)) return;
            // Does Index even exist?
            if (_nextAuthorityIndex >= _authorityEntities.Count) return;
            
            var nextPlayer = _authorityEntities[_nextAuthorityIndex];
            GiveAuthorityTo(nextPlayer);
            
            return;

            void GiveAuthorityTo(AuthorityEntity newAuthorityEntity) {
                if (newAuthorityEntity == null) {
                    Debug.LogError("AuthorityManager: Cannot set authority to null player.");
                    return;
                }
            
                if (!_authorityEntities.Contains(newAuthorityEntity)) {
                    Debug.LogError("AuthorityManager: Tried to set authority to a player that is not registered.");
                    return;
                }

                var currentIndex = _authorityEntities.IndexOf(newAuthorityEntity);
                _nextAuthorityIndex = (currentIndex + 1) % _authorityEntities.Count;
                if (_currentAuthority != null)
                    OnEntityAuthorityRevoked.Invoke(_currentAuthority);
                _currentAuthority = newAuthorityEntity;
                OnEntityAuthorityGained.Invoke(newAuthorityEntity);
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
            if (_currentAuthority != null)
                OnEntityAuthorityRevoked.Invoke(_currentAuthority);
            
            _currentAuthority = null;
        }

        [Button, BoxGroup("Debug")]
        public void UnregisterEntity(int index) {
            // Does index even exist?
            if (index < 0 || index >= _authorityEntities.Count) return;
            
            UnregisterEntity(_authorityEntities[index]);
        }

        public void UnregisterEntity(AuthorityEntity authorityEntity) {
            if (!ValidateEntityForUnregister(authorityEntity)) return;

            var removedIndex = _authorityEntities.IndexOf(authorityEntity);
            var hadAuthority = HasAuthority(authorityEntity);
    
            _authorityEntities.Remove(authorityEntity);
            AdjustNextAuthorityIndex(removedIndex);
            HandleGameEndCondition();
            HandleAuthorityTransfer(hadAuthority);
            OnEntityDied.Invoke(authorityEntity);
        }
        bool ValidateEntityForUnregister(AuthorityEntity authorityEntity) {
            if (authorityEntity == null) {
                Debug.LogError("AuthorityEntity is null.");
                return false;
            }

            return _authorityEntities.Contains(authorityEntity);
        }

        void AdjustNextAuthorityIndex(int removedIndex) {
            // Entity before next Index gets removed, shift index by 1 to the left
            if (removedIndex < _nextAuthorityIndex && _nextAuthorityIndex > 0) {
                _nextAuthorityIndex--;
            }
        }

        void HandleGameEndCondition() {
            if (_authorityEntities.Count != 1) return;
            
            _nextAuthorityIndex = -1;
            OnLastEntityRemaining.Invoke(_authorityEntities[0]);
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

        public int EntityCount => _authorityEntities.Count;
    }
}