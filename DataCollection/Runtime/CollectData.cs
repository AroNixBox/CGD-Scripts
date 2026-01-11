using System;
using System.Collections.Generic;
using System.IO;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Gameplay.Runtime.Player;
using Gameplay.Runtime.Player.Combat;
using UnityEngine;

namespace DataCollection.Runtime {
    /// <summary>
    ///     Collects and stores game data during runtime, then saves it to a JSON file.
    /// </summary>
    public class CollectData : MonoBehaviour {

        [SerializeField] private string saveFileName = "GameData";
        [SerializeField] private bool appendTimestamp = true;
        private readonly Dictionary<AuthorityEntity, string> _entityToUniqueId = new();
        private readonly Dictionary<string, UserData> _userDatas = new();
        private readonly Dictionary<string, WeaponData> _weaponDatas = new();
        private PlayerWeaponStash _activePlayerWeaponStash;

        private AuthorityManager _authorityManager;
        private AuthorityEntity _currentAuthorityEntity;
        private PlayerController _currentPlayerController;

        private bool _initialized;

        private MatchData _matchData;
        private bool _matchStarted;
        private DateTime _matchStartTime;

        private DateTime _turnStartTime;
        public static CollectData Instance { get; private set; }
        [Serializable]
        private struct MatchData {
            public string matchDuration; //done
            public int totalTurns; //done
            public string winner; //done
        }

        [Serializable]
        private struct UserData {
            public string uniqueId; // unique identifier for the user (e.g., "Player1_12345")
            public string userName; // done
            public List<Vector3> positions; // done
            public List<double> damagePerTurn;
            public List<string> usedWeaponPerTurn; // done
            public List<double> movementPercentagePerTurn; // noch nicht eingebaut
            public List<string> turnDurations; // done
            public int totalKills;
            public bool died; // done
            public Vector3 killerPositionOnDeath;
            public bool userRatedMatchAsFair; // noch nicht eingebaut
        }

        [Serializable]
        private struct WeaponData {
            public string weaponName; // done
            public int usageCount; // done
            public int hitCount;
            public List<double> damagePerUsage;
            public List<double> knockbackPerUsage;
            public List<double> distanceToTargetPerUsage;
        }

        [Serializable]
        private class GameData {
            public MatchData matchData;
            public List<UserData> userDatas;
            public List<WeaponData> weaponDatas;
        }

        #region Match Data Collection

        /// <summary>
        ///     Initializes match data collection. Call this at the start of a match.
        /// </summary>
        public void StartMatchDataCollection() {
            _matchStartTime = DateTime.Now;
            _matchData = new MatchData
            {
                totalTurns = 0,
                winner = ""
            };
        }

        /// <summary>
        ///     Increments the turn counter.
        /// </summary>
        public void IncrementTurn() {
            _matchData.totalTurns++;
        }

        /// <summary>
        ///     Sets the winner information.
        /// </summary>
        public void SetWinner(string winnerName) {
            _matchData.winner = winnerName;
        }

        /// <summary>
        ///     Finalizes match data. Call this when the match ends.
        /// </summary>
        public void EndMatchDataCollection() {
            TimeSpan duration = DateTime.Now - _matchStartTime;
            _matchData.matchDuration = duration.ToString(@"hh\:mm\:ss");
        }

        #endregion

        #region User Data Collection

        /// <summary>
        ///     Generates a unique identifier for a user based on their username and entity instance ID.
        /// </summary>
        private string GenerateUniqueId(AuthorityEntity entity) {
            return $"{entity.UserData.Username}_{entity.GetInstanceID()}";
        }

        /// <summary>
        ///     Gets the unique ID for an entity, or returns null if not found.
        /// </summary>
        private string GetUniqueId(AuthorityEntity entity) {
            return _entityToUniqueId.TryGetValue(entity, out var uniqueId) ? uniqueId : null;
        }

        /// <summary>
        ///     Initializes a user for data collection.
        /// </summary>
        public void InitializeUser(AuthorityEntity entity) {
            var uniqueId = GenerateUniqueId(entity);
            _entityToUniqueId[entity] = uniqueId;

            if (!_userDatas.ContainsKey(uniqueId)) {
                _userDatas[uniqueId] = new UserData
                {
                    uniqueId = uniqueId,
                    userName = entity.UserData.Username,
                    positions = new List<Vector3>(),
                    damagePerTurn = new List<double>(),
                    usedWeaponPerTurn = new List<string>(),
                    movementPercentagePerTurn = new List<double>(),
                    turnDurations = new List<string>(),
                    totalKills = 0,
                    died = false,
                    killerPositionOnDeath = Vector3.zero,
                    userRatedMatchAsFair = false
                };
            }
        }

        /// <summary>
        ///     Records user position.
        /// </summary>
        public void RecordUserPosition(string uniqueId, Vector3 position) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.positions.Add(position);
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Records damage dealt in a turn.
        /// </summary>
        public void RecordDamage(string uniqueId, double damage) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.damagePerTurn.Add(damage);
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Records weapon used in a turn.
        /// </summary>
        public void RecordWeaponUsed(string uniqueId, string weaponName) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.usedWeaponPerTurn.Add(weaponName);
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Records movement percentage in a turn.
        /// </summary>
        public void RecordMovementPercentage(string uniqueId, double percentage) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.movementPercentagePerTurn.Add(percentage);
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Records turn duration.
        /// </summary>
        public void RecordTurnDuration(string uniqueId, TimeSpan duration) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.turnDurations.Add(duration.ToString(@"mm\:ss"));
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Increments kill count for a user.
        /// </summary>
        public void RecordKill(string uniqueId) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.totalKills++;
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Records user death information.
        /// </summary>
        public void RecordDeath(string uniqueId, Vector3 killerPosition) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.died = true;
            data.killerPositionOnDeath = killerPosition;
            _userDatas[uniqueId] = data;
        }

        /// <summary>
        ///     Records user's fairness rating.
        /// </summary>
        public void RecordFairnessRating(string uniqueId, bool isFair) {
            if (!_userDatas.TryGetValue(uniqueId, out UserData data)) return;
            data.userRatedMatchAsFair = isFair;
            _userDatas[uniqueId] = data;
        }

        #endregion

        #region Weapon Data Collection

        /// <summary>
        ///     Initializes a weapon for data collection.
        /// </summary>
        public void InitializeWeapon(string weaponName) {
            if (!_weaponDatas.ContainsKey(weaponName)) {
                _weaponDatas[weaponName] = new WeaponData
                {
                    weaponName = weaponName,
                    usageCount = 0,
                    hitCount = 0,
                    damagePerUsage = new List<double>(),
                    knockbackPerUsage = new List<double>(),
                    distanceToTargetPerUsage = new List<double>()
                };
            }
        }

        /// <summary>
        ///     Records weapon usage.
        /// </summary>
        public void RecordWeaponUsage(string weaponName, double damage, double knockback, double distanceToTarget, bool hit) {
            if (!_weaponDatas.ContainsKey(weaponName)) InitializeWeapon(weaponName);

            WeaponData data = _weaponDatas[weaponName];
            data.usageCount++;
            if (hit) {
                data.hitCount++;
            }
            data.damagePerUsage.Add(damage);
            data.knockbackPerUsage.Add(knockback);
            data.distanceToTargetPerUsage.Add(distanceToTarget);
            _weaponDatas[weaponName] = data;
        }

        #endregion

        #region Save Data

        /// <summary>
        ///     Saves all collected data to a JSON file.
        /// </summary>
        public void SaveDataToJson() {
            var gameData = new GameData
            {
                matchData = _matchData,
                userDatas = new List<UserData>(_userDatas.Values),
                weaponDatas = new List<WeaponData>(_weaponDatas.Values)
            };

            var json = JsonUtility.ToJson(gameData, true);
            var fileName = saveFileName;

            if (appendTimestamp) {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                fileName = $"{saveFileName}_{timestamp}";
            }

            var filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");

            try {
                File.WriteAllText(filePath, json);
                Debug.Log($"Game data saved successfully to: {filePath}");
            }
            catch (Exception e) {
                Debug.LogError($"Failed to save game data: {e.Message}");
            }
        }

        /// <summary>
        ///     Clears all collected data.
        /// </summary>
        public void ClearData() {
            _matchData = new MatchData();
            _userDatas.Clear();
            _weaponDatas.Clear();
            _entityToUniqueId.Clear();
            _matchStarted = false;
        }

        #endregion

        #region Unity Events

        private void OnApplicationQuit() {
            SaveDataToJson();
        }
        private void Awake() {
            // Ensure singleton instance
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            if (!ServiceLocator.TryGet(out _authorityManager))
                throw new NullReferenceException("Authority-Manager not registered");
            _initialized = true;
            OnEnable();
        }

        private void OnEnable() {
            if (!_initialized) return;
            AuthorityManager.OnEntitySpawned += HandlePlayerSpawned;
            _authorityManager.OnEntityAuthorityGained += HandleTurnStart;
            _authorityManager.OnEntityAuthorityRevoked += HandleTurnEnd;
            AuthorityManager.OnEntityDied += HandlePlayerDied;
            _authorityManager.OnLastEntityRemaining += HandleGameOver;
        }

        private void OnDisable() {
            AuthorityManager.OnEntitySpawned -= HandlePlayerSpawned;
            _authorityManager.OnEntityAuthorityGained -= HandleTurnStart;
            _authorityManager.OnEntityAuthorityRevoked -= HandleTurnEnd;
            AuthorityManager.OnEntityDied -= HandlePlayerDied;
            _authorityManager.OnLastEntityRemaining -= HandleGameOver;
        }

        #endregion

        #region Event Handlers

        private void HandlePlayerSpawned(AuthorityEntity entity) {
            InitializeUser(entity);
        }

        private void HandleTurnStart(AuthorityEntity entity) {
            // Start match data collection on first turn
            if (!_matchStarted) {
                StartMatchDataCollection();
                _matchStarted = true;
            }

            IncrementTurn();
            _turnStartTime = DateTime.Now;
            _currentAuthorityEntity = entity;

            // Get the PlayerController to access MovementBudget
            entity.gameObject.TryGetComponent(out _currentPlayerController);

            if (!entity.gameObject.TryGetComponent(out _activePlayerWeaponStash)) return;
            _activePlayerWeaponStash.OnSuccessfulShot += HandleOnActivePlayerFired;

        }

        private void HandleTurnEnd(AuthorityEntity entity) {
            var uniqueId = GetUniqueId(entity);
            if (uniqueId == null) return;

            TimeSpan turnDuration = DateTime.Now - _turnStartTime;
            RecordTurnDuration(uniqueId, turnDuration);
            RecordUserPosition(uniqueId, entity.transform.position);

            // Calculate and record movement usage percentage
            if (_currentPlayerController != null && _currentPlayerController.MovementBudget != null) {
                // GetRemainingPercentage returns 1.0 for full, 0.0 for empty
                // So used percentage is (1 - remaining) * 100
                var usedMovementPercentage = (1.0 - _currentPlayerController.MovementBudget.GetRemainingPercentage()) * 100.0;
                RecordMovementPercentage(uniqueId, usedMovementPercentage);
            }

            if (_activePlayerWeaponStash != null)
                _activePlayerWeaponStash.OnSuccessfulShot -= HandleOnActivePlayerFired;

            _currentPlayerController = null;
        }

        private void HandlePlayerDied(AuthorityEntity entity) {
            var victimUniqueId = GetUniqueId(entity);
            if (victimUniqueId == null) return;

            // In a turn-based game, the player with authority is the killer
            // (they are the one who fired the shot that killed the victim)
            Vector3 killerPosition = Vector3.zero;

            if (_currentAuthorityEntity != null && _currentAuthorityEntity != entity) {
                // Record the killer's position at the time of the kill
                killerPosition = _currentAuthorityEntity.transform.position;

                // Increment the killer's kill count
                var killerUniqueId = GetUniqueId(_currentAuthorityEntity);
                if (killerUniqueId != null) {
                    RecordKill(killerUniqueId);
                }
            }

            // Record the victim's death with the killer's position
            RecordDeath(victimUniqueId, killerPosition);
        }

        private void HandleGameOver(AuthorityEntity winnerEntity) {
            SetWinner(GetUniqueId(winnerEntity));
            EndMatchDataCollection();
            SaveDataToJson();
            ClearData();
        }

        private void HandleOnActivePlayerFired(Projectile projectile) {
            var weaponName = projectile.name.Replace("(Clone)", "").Trim();
            var uniqueId = GetUniqueId(_currentAuthorityEntity);
            if (uniqueId == null) return;

            Vector3 startPosition = projectile.transform.position;

            // Subscribe to the projectile's impact event to wait until the shot actually lands
            projectile.OnImpact += OnProjectileImpact;

            void OnProjectileImpact(Vector3 impactPosition, ImpactResult impactResult) {
                projectile.OnImpact -= OnProjectileImpact;

                double distanceToTarget = Vector3.Distance(startPosition, impactPosition);
                // wasActiveImpact indicates if the projectile hit something (true) or expired via lifetime (false)
                var hit = impactResult.TargetsHit > 0;
                double damage = impactResult.TotalDamageDealt;
                double knockback = impactResult.TotalKnockbackApplied;

                // Record weapon statistics (global weapon data)
                RecordWeaponUsage(weaponName, damage, knockback, distanceToTarget, hit);

                // Record per-user data: weapon used and damage dealt this turn
                RecordWeaponUsed(uniqueId, weaponName);
                RecordDamage(uniqueId, damage);
            }
        }

        #endregion
    }
}