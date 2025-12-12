using System;
using Common.Runtime.Extensions;
using Core.Runtime.Authority;
using Core.Runtime.Service.Input;
using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using Gameplay.Runtime.Player.Combat;
using Gameplay.Runtime.Player.States;
using Gameplay.Runtime.Player.States.GroundedSubStates;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player {
    /// <summary>
    /// Calculates Momentum & Velocity based on the Player Input
    /// </summary>
    [RequireComponent(typeof(PlayerMover), typeof(AuthorityEntity))]
    public class PlayerController : MonoBehaviour {
        #region fields

        [Header("References")]
        [field: SerializeField, Required] public InputReader InputReader { get; private set; }
        [field: SerializeField, Required] public PlayerCameraControls PlayerCameraControls { get; private set; }
        [field: SerializeField, Required] public PlayerAnimatorController AnimatorController { get; private set; }
        [field: SerializeField, Required] public PlayerWeaponStash WeaponStash { get; private set; }
        [SerializeField, Required] Transform modelRoot;
        [SerializeField, Required] Transform visualModel;
        public Transform VisualModel => visualModel;
        
        public AuthorityEntity AuthorityEntity { get; private set; }

        Transform _tr;
        PlayerMover _mover;
        MovementBudget _movementBudget;
        
        public MovementBudget MovementBudget => _movementBudget;

        [Header("Settings")]
        [Title("Movement")]
        [InfoBox("<i>When Changing the MovementSpeed, dont forget to change the Locomotion-Animation-State Thresholds on the Animator</i>", InfoMessageType.Warning)]
        [SerializeField] float movementSpeed = 3f;
        [SerializeField] float maxMovementTimePerTurn = 5f;
        [SerializeField] float airFriction = .5f;
        [SerializeField] float airControlRate = 1.5f;
        [SerializeField] float groundFriction = 100f;
        [SerializeField] float gravity = 30f;
        [SerializeField] float slideGravity = 5f;
        [InfoBox("Minimum time the player has to be falling before landing state is required to transition to grounded.\n")]
        [SerializeField] float fallTimeUntilLandingRequired = 1f;
        [SerializeField, Range(0f, 70f)] float slopeLimit = 30f;
        [InfoBox("<b><u>Rotation Speed</u></b>\n" +
                 "Only the visual model is rotated, has nothing to do with any physics or player movement.\n" +
                 "<i>Tip: Higher values make the model snap quickly, lower values create a smoother turn.</i>")]
        [SerializeField, Range(0f, 10f)] float rotationSpeed = 5f;
        
        [InfoBox("<b><u>Local Momentum</u></b>\n" +
                 "<b>Off (World Space):</b> Momentum stays fixed in world direction. Player runs forward then rotates 180°? " +
                 "They keep sliding in the original direction. Great for realistic physics and sliding mechanics.\n" +
                 "<b>On (Local Space):</b> Momentum rotates with the player. Turn around and your momentum follows. " +
                 "Perfect for responsive, arcade-style movement.\n" +
                 "<i>Tip: Enable for tight controls, disable for physics-heavy gameplay.</i>")]
        [SerializeField] bool useLocalMomentum;
        
        [Title("Combat")]
        [SerializeField, Tooltip("Seconds the Player stays in CombatRecoveryState (and Bulletcam stays there too) after bullet expires")]
        float postImpactDelay = 1.5f;

        public float PostImpactDelay => postImpactDelay;

        [Title("Debug")] 
        [SerializeField] bool debugMode = true;
        [SerializeField] float debugBaseStateDrawHeight = 2f;
        [SerializeField] float debugBaseStateDrawRadius = .25f;
        
        StateMachine _stateMachine;
        
        private float _speedMultiplier = 1f;
        
        public float MovementSpeed => movementSpeed;
        public float SpeedMultiplier => _speedMultiplier;
        
        public float EffectiveMovementSpeed => movementSpeed * _speedMultiplier;

        Vector3 _momentum, _savedVelocity, _savedMovementVelocity, _externalForces;
        public event Action<Vector3> OnLand = delegate { }; // TODO: Call when entering Grounded State
        
        #endregion

        #region events

        public Action OnCombatStanceStateEntered = delegate { };
        public Action OnCombatStanceStateExited = delegate { };
        public Action OnLocomotionStateEntered = delegate { };
        public Action OnLocomotionStateExited = delegate { };
        public event Action<float, float> OnMovementBudgetChanged = delegate { }; // currentTime, maxTime

        #endregion
        void Awake() {
            _tr = transform;
            _mover = GetComponent<PlayerMover>();
            AuthorityEntity = GetComponent<AuthorityEntity>();
            _movementBudget = new MovementBudget(maxMovementTimePerTurn);
            
            SetupStateMachine();
        }

        void SetupStateMachine() {
            _stateMachine = new StateMachine();
            
            // States are independent of authority
            // Player can fall even without authority
            var grounded = new GroundedState(this);
            var falling = new FallingState(this);
            var sliding = new SlidingState(this);
            var landing = new LandingState(this);
            
            At(grounded, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            At(grounded, falling, () => !_mover.IsGrounded());
            
            At(falling, landing, () => _mover.IsGrounded() && !IsGroundTooSteep() && FallingLongEnough());
            At(falling, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep() && !FallingLongEnough());
            At(falling, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            
            At(sliding, falling, () => !_mover.IsGrounded());
            // Directly going to grounded
            At(sliding, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            
            At(landing, grounded, () => landing.HasLanded());
            At(landing, falling, () => !_mover.IsGrounded());
            
            _stateMachine.SetState(falling);

            return;
            
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
            void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
        
            // Can be used to check if we are moving downwards
            bool IsFalling() => VectorMath.GetDotProduct(GetMomentum(), _tr.up) < 0f;
            bool IsGroundTooSteep() => Vector3.Angle(_mover.GetGroundNormal(), _tr.up) > slopeLimit;
            
            bool FallingLongEnough() => falling.GetFallingTime() > fallTimeUntilLandingRequired;
        }
        
        // TODO: Use for Animator later on
        public Vector3 GetMovementVelocity() => _savedMovementVelocity; // Inputdirection * MovementSpeed
        public Vector3 GetVelocity() => _savedVelocity; // Includes Gravity & Momentum & Movement
        void Start() {
            InputReader.EnablePlayerActions();
        }
        void Update() {
            _stateMachine.Tick(Time.deltaTime);
            
            // Update movement budget
            var currentState = _stateMachine.GetCurrentState();
            if (currentState is GroundedState groundedState) {
                var currentSubState = groundedState.GetCurrentState();
                if (currentSubState is LocomotionState) {
                    bool isMoving = InputReader.MoveDirection.sqrMagnitude > 0.01f;
                    _movementBudget.UpdateMovement(isMoving, Time.deltaTime);
                    OnMovementBudgetChanged?.Invoke(_movementBudget.GetRemainingTime(), maxMovementTimePerTurn);
                }
            }
        }

        /// <summary>
        /// Converts horizontal movement velocity into momentum when leaving the ground.
        /// Prevents velocity stacking by removing overlapping momentum components and ensures smooth air transitions.
        /// </summary>
        public void OnGroundContactLost() {
            if(useLocalMomentum) _momentum = _tr.localToWorldMatrix * _momentum;

            var velocity = _savedMovementVelocity;
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
            
            var velocity = Vector3.zero;
            var currentState = _stateMachine.GetCurrentState();
            if (currentState is GroundedState and ISubStateMachine groundedSubStateMachine) {
                var currentSubState = groundedSubStateMachine.GetCurrentState();
                // Only use player input if we are in the Locomotion State
                if (currentSubState is LocomotionState) {
                    velocity = CalculateMovementVelocity();
                    if (velocity.magnitude > 0.1f) 
                        modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, Quaternion.LookRotation(velocity.normalized), Time.deltaTime * rotationSpeed);
                }
                // If wed like to rotate the model from first person-mode
                // else if (currentSubState is CombatStanceState) {
                //     // Rotate the player model towards look direction
                //     var activeCamera = PlayerCameraControls.GetActiveCameraTransform();
                //     // Rotate towards active camera forward direction, ignoring vertical difference
                //     var lookDirection = Vector3.ProjectOnPlane(activeCamera.forward, _tr.up).normalized;
                //     modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);
                // }
            }
            
            
            velocity += useLocalMomentum 
                ? _tr.localToWorldMatrix * _momentum 
                : _momentum;
            
            
            // Use the extended sensor range if we are grounded
            var grounded = _stateMachine.GetCurrentState() is GroundedState or SlidingState or LandingState;
            _mover.SetExtendedSensorRange(grounded);
            
            _mover.SetVelocity(velocity);
            
            _savedVelocity = velocity;
            _savedMovementVelocity = CalculateMovementVelocity(); // Based on Input
        }
        void HandleMomentum() {
            if(useLocalMomentum) _momentum = _tr.localToWorldMatrix * _momentum;
            
            var verticalMomentum = VectorMath.ExtractDotVector(_momentum, _tr.up);
            var horizontalMomentum = _momentum - verticalMomentum;
            
            verticalMomentum -= _tr.up * (gravity * Time.deltaTime);
            
            var currentState = _stateMachine.GetCurrentState();
            if(currentState is GroundedState or LandingState 
               && VectorMath.GetDotProduct(verticalMomentum, _tr.up) < 0f) {
                verticalMomentum = Vector3.zero;
            }

            { // Handle player air control when in the air
                var inAir = currentState is FallingState;
                if (inAir) {
                    HandleAirControl(ref horizontalMomentum);
                } 
            }
            
            if (currentState is SlidingState) { // handle sliding control, when on a too steep slope
                HandleSliding(ref horizontalMomentum);
            }
            
            var friction = currentState is GroundedState or LandingState ? groundFriction : airFriction;
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
            var movementVelocity = CalculateMovementVelocity(); // controlled sliding movement
            
            // Stop crazy acceleration due to player input
            movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }
        void HandleAirControl(ref Vector3 horizontalMomentum) {
            var movementVelocity = CalculateMovementVelocity(); // Controlled falling
            // Figure out what our actual Movement Velocity is and make adjustments
            // Moving faster than our movement speed? => From a fall e.g.
            if (horizontalMomentum.magnitude > EffectiveMovementSpeed) {
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
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, EffectiveMovementSpeed);
            }
        }
        Vector3 CalculateMovementVelocity() {
            if (!_movementBudget.CanMove()) return Vector3.zero;
            return CalculateMovementDirection() * EffectiveMovementSpeed;
        }
        // Based on Camera and players input
        // TODO: Check if we need to Vector3.zero if no auth, because technicall we dont have any input then
        Vector3 CalculateMovementDirection() {
            var activeCamera = PlayerCameraControls.GetActiveCameraTransform();
            var direction = Vector3.ProjectOnPlane(activeCamera.right, _tr.up).normalized * InputReader.MoveDirection.x +
                  Vector3.ProjectOnPlane(activeCamera.forward, _tr.up).normalized * InputReader.MoveDirection.y;
            
            return direction.magnitude > 1f 
                ? direction.normalized
                : direction;
        }
        public void ApplyExternalForce(Vector3 force) {
            _momentum += force;
        }
        
        void OnDrawGizmos() {
            if(!debugMode) return;
            var currentState = _stateMachine?.GetCurrentState();
            if(currentState == null) return;
        
            var drawHeight = debugBaseStateDrawHeight;
            var drawRadius = debugBaseStateDrawRadius;
            
            if (currentState is ISubStateMachine subStateMachine) {
                var subStateMachineDrawHeight = drawHeight;
                var subStateMachineDrawSize = drawRadius * 2;
                
                // Draw SubStateMachine as Cube
                Gizmos.color = subStateMachine.GizmoState();
                Gizmos.DrawCube(transform.position + Vector3.up * subStateMachineDrawHeight, Vector3.one * subStateMachineDrawSize);
        
                drawHeight += subStateMachineDrawHeight * .25f; // place slightly above
                drawRadius *= .5f; // smaller sphere, because we are in a substate  
                
                currentState = subStateMachine.GetCurrentState();
            }
            
            Gizmos.color = currentState.GizmoState();
            Gizmos.DrawSphere(transform.position + Vector3.up * drawHeight, drawRadius);
        }
        
        public void SetSpeedMultiplier(float speedMultiplier) {
            _speedMultiplier = speedMultiplier;
            AnimatorController.UpdateAnimatorSpeed(EffectiveMovementSpeed);
        }
        
        public void ResetSpeedMultiplier() {
            _speedMultiplier = 1f;
            AnimatorController.UpdateAnimatorSpeed(EffectiveMovementSpeed);
        }
    }
}
