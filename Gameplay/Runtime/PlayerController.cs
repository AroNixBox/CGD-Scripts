using System;
using Common.Runtime.Extensions;
using Core.Runtime.Service.Input;
using Extensions.FSM;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime {
    /// <summary>
    /// Calculates Momentum & Velocity based on the Player Input
    /// </summary>
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerController : MonoBehaviour {
        #region fields

        [Header("References")]
        [SerializeField, Required] InputReader inputReader;
        [SerializeField] Transform cameraTransform; // TODO: Could be required later on

        Transform _tr;
        PlayerMover _mover;

        [Header("Movement Settings")]
        [SerializeField] float movementSpeed = 3f;
        [SerializeField] float airFriction = .5f;
        [SerializeField] float airControlRate = 1.5f;
        [SerializeField] float groundFriction = 100f;
        [SerializeField] float gravity = 30f;
        [SerializeField] float slideGravity = 5f;
        [InfoBox("<b><u>Slope Limit</u></b>\n" +
                "Due to active Ragdoll, there is slight friction from the legs while a physical animation is playing \n " +
                "This causes a slight pull-down effect while on a very steep slope.")]
        [SerializeField, Range(0f, 70f)] float slopeLimit = 30f; // Degrees
        
        [InfoBox("<b><u>Local Momentum</u></b>\n" +
                 "<b>Off (World Space):</b> Momentum stays fixed in world direction. Player runs forward then rotates 180°? " +
                 "They keep sliding in the original direction. Great for realistic physics and sliding mechanics.\n" +
                 "<b>On (Local Space):</b> Momentum rotates with the player. Turn around and your momentum follows. " +
                 "Perfect for responsive, arcade-style movement.\n" +
                 "<i>Tip: Enable for tight controls, disable for physics-heavy gameplay.</i>")]
        [SerializeField] bool useLocalMomentum;
        
        StateMachine _stateMachine;

        Vector3 _momentum, _savedVelocity, _savedMovementVelocity;
        public event Action<Vector3> OnLand = delegate { };
        #endregion

        void Awake() {
            _tr = transform;
            _mover = GetComponent<PlayerMover>();
            
            SetupStateMachine();
        }

        void SetupStateMachine() {
            _stateMachine = new StateMachine();
            
            var grounded = new GroundedState(this);
            var falling = new FallingState(this);
            var sliding = new SlidingState(this);
            var rising = new RisingState(this);
            
            At(grounded, rising, IsRising);
            At(grounded, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            At(grounded, falling, () => !_mover.IsGrounded());
            
            At(falling, rising, IsRising);
            At(falling, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            At(falling, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            
            At(sliding, rising, IsRising);
            At(sliding, falling, () => !_mover.IsGrounded());
            At(sliding, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            
            At(rising, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            At(rising, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            At(rising, falling, IsFalling);
            
            _stateMachine.SetState(falling);
        }
        
        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
        void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
        
        // TODO: Figure out if we need this;
        bool IsRising() => VectorMath.GetDotProduct(GetMomentum(), _tr.up) > 0f;
        bool IsFalling() => VectorMath.GetDotProduct(GetMomentum(), _tr.up) < 0f;
        bool IsGroundTooSteep() => Vector3.Angle(_mover.GetGroundNormal(), _tr.up) > slopeLimit;
        void Start() {
            inputReader.EnablePlayerActions();
        }
        void Update() {
            _stateMachine.Tick();
        }

        public void OnGroundContactLost() {
            if(useLocalMomentum) _momentum = _tr.localToWorldMatrix * _momentum;

            var velocity = GetMovementVelocity();
            if (velocity.sqrMagnitude >= 0f && _momentum.sqrMagnitude > 0f) {
                // Find the component of momentum thats alligned with the velocity of our object
                var projectedMomentum = Vector3.Project(_momentum, velocity.normalized);
                // Same direction?
                var dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);
                
                // If the momentum is already sufficient to bring the player to a halt in the air
                // Set velocity to zero, simulating an instant stop
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) {
                    velocity = Vector3.zero;
                } 
                // If the momentum is still prepelling the player forward, reduce the velocity by the momentum
                else if (dot > 0f) {
                    velocity -= projectedMomentum;
                }
                
                _momentum += velocity;
                
                if(useLocalMomentum) _momentum = _tr.worldToLocalMatrix * _momentum;
            }
        }
        
        // TODO: Use for Animator later on
        public Vector3 GetMovementVelocity() => _savedMovementVelocity; // Inputdirection * MovementSpeed
        public Vector3 GetVelocity() => _savedVelocity; // Includes Gravity & Momentum & Movement
        
        public void OnGroundContactRegained() {
            Vector3 collisionVelocity = useLocalMomentum ?
                _tr.localToWorldMatrix * _momentum 
                : _momentum;
            OnLand?.Invoke(collisionVelocity);
        }
        
        public Vector3 GetMomentum() => useLocalMomentum 
            ? _tr.localToWorldMatrix * _momentum 
            : _momentum;
        void FixedUpdate() {
            _mover.CheckForGround();
            HandleMomentum();
            var velocity = _stateMachine.GetCurrentState() is GroundedState 
                ? CalculateMovementVelocity()
                : Vector3.zero;
            velocity += useLocalMomentum 
                ? _tr.localToWorldMatrix * _momentum 
                : _momentum;
            
            // Use the extended sensor range if we are grounded
            _mover.SetExtendedSensorRange(IsGrounded());
            _mover.SetVelocity(velocity);
            
            _savedVelocity = velocity;
            _savedMovementVelocity = CalculateMovementVelocity(); // Based on Input
        }
        void HandleMomentum() {
            if(useLocalMomentum) _momentum = _tr.localToWorldMatrix * _momentum;
            
            var verticalMomentum = VectorMath.ExtractDotVector(_momentum, _tr.up);
            var horizontalMomentum = _momentum - verticalMomentum;
            
            verticalMomentum -= _tr.up * (gravity * Time.deltaTime);
            
            // TODO: Are we in the grounded state, means we reached the ground, stop putting negative force on them
            var currentState = _stateMachine.GetCurrentState();
            if(currentState is GroundedState && VectorMath.GetDotProduct(verticalMomentum, _tr.up) < 0f) {
                verticalMomentum = Vector3.zero;
            }
            
            if (!IsGrounded()) {
                AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
            }

            if (currentState is SlidingState) {
                HandleSliding(ref horizontalMomentum);
            }
            
            var friction = currentState is GroundedState ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            
            // Update final momentum;
            _momentum = horizontalMomentum + verticalMomentum;
            
            // TODO: Could handle jumping here

            if (currentState is SlidingState) {
                // Project the entire horizontal momentum onto plane that is defined by the ground normal
                _momentum = Vector3.ProjectOnPlane(_momentum, _mover.GetGroundNormal());
                // Remove Upward momentum if positive
                if (VectorMath.GetDotProduct(_momentum, _tr.up) > 0f) {
                    _momentum = VectorMath.RemoveDotVector(_momentum, _tr.up);
                }

                // Direction the player should slide downwards
                var slideDirection = Vector3.ProjectOnPlane(-_tr.up, _mover.GetGroundNormal()).normalized;
                // Simulate the gravity that is pulling the player down the slope
                _momentum += slideDirection * (slideGravity * Time.deltaTime);
            }

            if (useLocalMomentum) _momentum = _tr.worldToLocalMatrix* _momentum;
        }
        void HandleSliding(ref Vector3 horizontalMomentum) {
            // Slope direction, in which it descends
            var pointDownVector = Vector3.ProjectOnPlane(_mover.GetGroundNormal(), _tr.up).normalized;
            var movementVelocity = CalculateMovementVelocity();
            
            // Stop crazy acceleration due to player input
            movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }
        void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity) {
            // Figure out what our actual Movement Velocity is and make adjustments
            // Moving faster than our movement speed? => From a fall e.g.
            if (horizontalMomentum.magnitude > movementSpeed) {
                // Evaluate if the Movement Velocity Vector has any component in the direction of our current horizontal momentum
                // Means are we trying to move in the same direction as our momentum is going
                // To not get crazy momentum increases
                if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f) {
                    // If so remove that component from our movement velocity
                    movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                }
                    
                // Add suddle air control without overwhealming the existing momentum
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * .25f);
            }
            else {
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
                // Dont overshoot our movement speed
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
            }
        }
        Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;
        // Based on Camera and players input
        Vector3 CalculateMovementDirection() {
            var direction = cameraTransform == null
                // No camera found, use input as single direction calculation 
                ? _tr.right * inputReader.MoveDirection.x + _tr.forward * inputReader.MoveDirection.y
                // We found a camera, use the direction the camera is facing, not the players direction
                : Vector3.ProjectOnPlane(cameraTransform.right, _tr.up).normalized * inputReader.MoveDirection.x +
                  Vector3.ProjectOnPlane(cameraTransform.forward, _tr.up).normalized * inputReader.MoveDirection.y;

            return direction.magnitude > 1f 
                ? direction.normalized
                : direction;
        }
        bool IsGrounded() => _stateMachine.GetCurrentState() is GroundedState or SlidingState;
    }
}
