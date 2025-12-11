using System;
using System.Collections.Generic;
using System.IO;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Gameplay.Runtime.Player.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataCollection.Runtime {
    /// <summary>
    /// Collects and stores game data during runtime, then saves it to a JSON file.
    /// </summary>
    public class CollectData : MonoBehaviour {
        [Serializable]
        private struct MatchData {
            public string matchDuration; //done
            public int totalTurns; //done
            public string winner; //done
        }

        [Serializable]
        private struct UserData {
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
            public List<double> distanceToTargetPerUsage;
        }

        [Serializable]
        private class GameData {
            public MatchData matchData;
            public List<UserData> userDatas;
            public List<WeaponData> weaponDatas;
        }

        private MatchData _matchData;
        private readonly Dictionary<string, UserData> _userDatas = new();
        private readonly Dictionary<string, WeaponData> _weaponDatas = new();
        private DateTime _matchStartTime;
        public static CollectData Instance { get; private set; }

        [SerializeField] private string saveFileName = "GameData";
        [SerializeField] private bool appendTimestamp = true;

        private AuthorityManager _authorityManager;

        private bool _initialized = false;
        
        private DateTime _turnStartTime;
        private PlayerWeaponStash _activePlayerWeaponStash;
        private AuthorityEntity _currentAuthorityEntity;

        #region Match Data Collection

        /// <summary>
        /// Initializes match data collection. Call this at the start of a match.
        /// </summary>
        public void StartMatchDataCollection() {
            _matchStartTime = DateTime.Now;
            _matchData = new MatchData
            {
                totalTurns = 0,
                winner = "",
            };
        }

        /// <summary>
        /// Increments the turn counter.
        /// </summary>
        public void IncrementTurn() {
            _matchData.totalTurns++;
        }

        /// <summary>
        /// Sets the winner information.
        /// </summary>
        public void SetWinner(string winnerName) {
            _matchData.winner = winnerName;
        }

        /// <summary>
        /// Finalizes match data. Call this when the match ends.
        /// </summary>
        public void EndMatchDataCollection() {
            TimeSpan duration = DateTime.Now - _matchStartTime;
            _matchData.matchDuration = duration.ToString(@"hh\:mm\:ss");
        }

        #endregion

        #region User Data Collection

        /// <summary>
        /// Initializes a user for data collection.
        /// </summary>
        public void InitializeUser(string userName) {
            if (!_userDatas.ContainsKey(userName)) {
                _userDatas[userName] = new UserData
                {
                    userName = userName,
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
        /// Records user position.
        /// </summary>
        public void RecordUserPosition(string userName, Vector3 position) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.positions.Add(position);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records damage dealt in a turn.
        /// </summary>
        public void RecordDamage(string userName, double damage) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.damagePerTurn.Add(damage);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records weapon used in a turn.
        /// </summary>
        public void RecordWeaponUsed(string userName, string weaponName) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.usedWeaponPerTurn.Add(weaponName);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records movement percentage in a turn.
        /// </summary>
        public void RecordMovementPercentage(string userName, double percentage) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.movementPercentagePerTurn.Add(percentage);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records turn duration.
        /// </summary>
        public void RecordTurnDuration(string userName, TimeSpan duration) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.turnDurations.Add(duration.ToString(@"mm\:ss"));
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Increments kill count for a user.
        /// </summary>
        public void RecordKill(string userName) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.totalKills++;
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records user death information.
        /// </summary>
        public void RecordDeath(string userName, Vector3 killerPosition) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.died = true;
            data.killerPositionOnDeath = killerPosition;
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records user's fairness rating.
        /// </summary>
        public void RecordFairnessRating(string userName, bool isFair) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.userRatedMatchAsFair = isFair;
            _userDatas[userName] = data;
        }

        #endregion

        #region Weapon Data Collection

        /// <summary>
        /// Initializes a weapon for data collection.
        /// </summary>
        public void InitializeWeapon(string weaponName) {
            if (!_weaponDatas.ContainsKey(weaponName)) {
                _weaponDatas[weaponName] = new WeaponData
                {
                    weaponName = weaponName,
                    usageCount = 0,
                    hitCount = 0,
                    damagePerUsage = new List<double>(),
                    distanceToTargetPerUsage = new List<double>()
                };
            }
        }

        /// <summary>
        /// Records weapon usage.
        /// </summary>
        public void RecordWeaponUsage(string weaponName, double damage, double distanceToTarget, bool hit) {
            if (!_weaponDatas.ContainsKey(weaponName)) InitializeWeapon(weaponName);

            WeaponData data = _weaponDatas[weaponName];
            data.usageCount++;
            if (hit) {
                data.hitCount++;
            }
            data.damagePerUsage.Add(damage);
            data.distanceToTargetPerUsage.Add(distanceToTarget);
            _weaponDatas[weaponName] = data;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// Saves all collected data to a JSON file.
        /// </summary>
        public void SaveDataToJson() {
            var gameData = new GameData
            {
                matchData = _matchData,
                userDatas = new List<UserData>(_userDatas.Values),
                weaponDatas = new List<WeaponData>(_weaponDatas.Values)
            };

            string json = JsonUtility.ToJson(gameData, true);
            string fileName = saveFileName;

            if (appendTimestamp) {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                fileName = $"{saveFileName}_{timestamp}";
            }

            string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");

            try {
                File.WriteAllText(filePath, json);
                Debug.Log($"Game data saved successfully to: {filePath}");
            }
            catch (Exception e) {
                Debug.LogError($"Failed to save game data: {e.Message}");
            }
        }

        /// <summary>
        /// Clears all collected data.
        /// </summary>
        public void ClearData() {
            _matchData = new MatchData();
            _userDatas.Clear();
            _weaponDatas.Clear();
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
            InitializeUser(entity.UserData.Username);
        }

        private void HandleTurnStart(AuthorityEntity entity) {
            IncrementTurn();
            _turnStartTime = DateTime.Now;
            _currentAuthorityEntity = entity;
            if (!entity.gameObject.TryGetComponent(out _activePlayerWeaponStash)) return;
            _activePlayerWeaponStash.OnSuccessfulShot += HandleOnActivePlayerFired;
            
        }

        private void HandleTurnEnd(AuthorityEntity entity) {
            TimeSpan turnDuration = DateTime.Now - _turnStartTime;
            RecordTurnDuration(entity.UserData.Username, turnDuration);
            RecordUserPosition(entity.UserData.Username, entity.transform.position);
            if (_activePlayerWeaponStash != null)
                _activePlayerWeaponStash.OnSuccessfulShot -= HandleOnActivePlayerFired;
        }

        private void HandlePlayerDied(AuthorityEntity entity) {
            // get killer
            RecordDeath(entity.UserData.Username, Vector3.zero);
        }

        private void HandleGameOver(AuthorityEntity winnerEntity) {
            SetWinner(winnerEntity.UserData.Username);
            EndMatchDataCollection();
            SaveDataToJson();
            ClearData();
        }

        private void HandleOnActivePlayerFired(Projectile projectile) {
            string weaponName = projectile.name;
            double damage = 0; // Placeholder, replace with actual damage calculation
            double distanceToTarget = Vector3.Distance(projectile.transform.position, Vector3.zero); // Placeholder, replace with actual target position
            bool hit = true; // Placeholder, replace with actual hit detection

            RecordWeaponUsage(weaponName, damage, distanceToTarget, hit);
            RecordWeaponUsed(_currentAuthorityEntity.UserData.Username, weaponName);
        }

        #endregion
    }
}