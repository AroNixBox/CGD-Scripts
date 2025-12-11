using System;
using System.Collections.Generic;
using System.IO;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Gameplay.Runtime.Player.Combat;
using UnityEngine;

namespace DataCollection.Runtime {
    /// <summary>
    /// Collects and stores game data during runtime, then saves it to a JSON file.
    /// </summary>
    public class CollectData : MonoBehaviour {
        [Serializable]
        private struct MatchData {
            public string MatchDuration; //done
            public int TotalTurns; //done
            public string Winner; //done
        }

        [Serializable]
        private struct UserData {
            public string UserName; // done
            public List<Vector3> Positions; // done
            public List<double> DamagePerTurn;
            public List<string> UsedWeaponPerTurn;
            public List<double> MovementPercentagePerTurn; // noch nicht eingebaut
            public List<string> TurnDurations; // done
            public int TotalKills;
            public bool Died; // done
            public Vector3 KillerPositionOnDeath;
            public string DeathReason;
            public bool UserRatedMatchAsFair; // noch nicht eingebaut
        }

        [Serializable]
        private struct WeaponData {
            public string WeaponName;
            public int UsageCount;
            public int HitCount;
            public List<double> DamagePerUsage;
            public List<double> DistanceToTargetPerUsage;
        }

        [Serializable]
        private class GameData {
            public MatchData MatchData;
            public List<UserData> UserDatas;
            public List<WeaponData> WeaponDatas;
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
                TotalTurns = 0,
                Winner = "",
            };
        }

        /// <summary>
        /// Increments the turn counter.
        /// </summary>
        public void IncrementTurn() {
            _matchData.TotalTurns++;
        }

        /// <summary>
        /// Sets the winner information.
        /// </summary>
        public void SetWinner(string winnerName) {
            _matchData.Winner = winnerName;
        }

        /// <summary>
        /// Finalizes match data. Call this when the match ends.
        /// </summary>
        public void EndMatchDataCollection() {
            TimeSpan duration = DateTime.Now - _matchStartTime;
            _matchData.MatchDuration = duration.ToString(@"hh\:mm\:ss");
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
                    UserName = userName,
                    Positions = new List<Vector3>(),
                    DamagePerTurn = new List<double>(),
                    UsedWeaponPerTurn = new List<string>(),
                    MovementPercentagePerTurn = new List<double>(),
                    TurnDurations = new List<string>(),
                    TotalKills = 0,
                    Died = false,
                    KillerPositionOnDeath = Vector3.zero,
                    UserRatedMatchAsFair = false
                };
            }
        }

        /// <summary>
        /// Records user position.
        /// </summary>
        public void RecordUserPosition(string userName, Vector3 position) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.Positions.Add(position);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records damage dealt in a turn.
        /// </summary>
        public void RecordDamage(string userName, double damage) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.DamagePerTurn.Add(damage);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records weapon used in a turn.
        /// </summary>
        public void RecordWeaponUsed(string userName, string weaponName) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.UsedWeaponPerTurn.Add(weaponName);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records movement percentage in a turn.
        /// </summary>
        public void RecordMovementPercentage(string userName, double percentage) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.MovementPercentagePerTurn.Add(percentage);
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records turn duration.
        /// </summary>
        public void RecordTurnDuration(string userName, TimeSpan duration) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.TurnDurations.Add(duration.ToString(@"mm\:ss"));
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Increments kill count for a user.
        /// </summary>
        public void RecordKill(string userName) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.TotalKills++;
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records user death information.
        /// </summary>
        public void RecordDeath(string userName, Vector3 killerPosition) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.Died = true;
            data.KillerPositionOnDeath = killerPosition;
            _userDatas[userName] = data;
        }

        /// <summary>
        /// Records user's fairness rating.
        /// </summary>
        public void RecordFairnessRating(string userName, bool isFair) {
            if (!_userDatas.TryGetValue(userName, out UserData data)) return;
            data.UserRatedMatchAsFair = isFair;
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
                    WeaponName = weaponName,
                    UsageCount = 0,
                    HitCount = 0,
                    DamagePerUsage = new List<double>(),
                    DistanceToTargetPerUsage = new List<double>()
                };
            }
        }

        /// <summary>
        /// Records weapon usage.
        /// </summary>
        public void RecordWeaponUsage(string weaponName, double damage, double distanceToTarget, bool hit) {
            if (!_weaponDatas.ContainsKey(weaponName)) InitializeWeapon(weaponName);

            WeaponData data = _weaponDatas[weaponName];
            data.UsageCount++;
            if (hit) {
                data.HitCount++;
            }
            data.DamagePerUsage.Add(damage);
            data.DistanceToTargetPerUsage.Add(distanceToTarget);
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
                MatchData = _matchData,
                UserDatas = new List<UserData>(_userDatas.Values),
                WeaponDatas = new List<WeaponData>(_weaponDatas.Values)
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