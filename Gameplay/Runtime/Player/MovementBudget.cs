namespace Gameplay.Runtime.Player {
    /// <summary>
    /// Time-based movement budget system for turn-based movement.
    /// Similar to Shellshock Live: player has X seconds to move, regardless of distance covered.
    /// </summary>
    public class MovementBudget {
        readonly float _maxTime;
        float _remainingTime;

        public MovementBudget(float maxTime) {
            _maxTime = maxTime;
            _remainingTime = maxTime;
        }

        public void Reset() {
            _remainingTime = _maxTime;
        }

        public bool CanMove() => _remainingTime > 0f;

        public void UpdateMovement(bool isMoving, float deltaTime) {
            if (isMoving && _remainingTime > 0f) {
                _remainingTime = UnityEngine.Mathf.Max(0f, _remainingTime - deltaTime);
            }
        }

        public float GetRemainingTime() => _remainingTime;
        public float GetRemainingPercentage() => _remainingTime / _maxTime;
    }
}

